namespace FsShaderGraphComponents

open Microsoft.FSharp.Reflection

open System
open System.Text
open System.Collections.Generic
open System.Linq
open System.Drawing
open System.Windows.Forms

open Grasshopper
open Grasshopper.Kernel
open Grasshopper.Kernel.Attributes
open Grasshopper.Kernel.Types
open Grasshopper.Kernel.Special
open Grasshopper.GUI.Canvas

open Rhino.Geometry

open ShaderGraphResources

open System.Diagnostics


/// type that signals Grasshopper to continue loading. Here we
/// do necessary initialisation
type Priority() = 
    inherit GH_AssemblyPriority()
    override u.PriorityLoad() = GH_LoadingInstruction.Proceed

/// Grasshopper plug-in assembly information.
type Info() =
    inherit GH_AssemblyInfo()

    override u.Name = "Shader Nodes"
    override u.Description = "Create shader graphs for Cycles for Rhino"
    override u.Id = new Guid("6a051e83-3727-465e-b5ef-74d027a6f73b")
    override u.Icon = Icons.ShaderGraph
    override u.AuthorName = "Nathan 'jesterKing' Letwory"
    override u.AuthorContact = "nathan@mcneel.com"

// ---------------------------------------

/// interface that shader nodes need to implement to be able to
/// participate in shader XML generation.
type ICyclesNode =
    /// Get the XML name of the node tag.
    abstract member NodeName : string
    /// Get the XML representation of the node. NodeName, NickName, Parameter list. Returns XML string
    abstract member GetXml : string -> string -> List<IGH_Param> -> string

/// Simple color representation with ints (R, G, B)
type IntColor = int * int * int
/// Socket connection info (tocomponent, tosocket, fromsocket, fromcomponent)
type SocketsInfo = obj * IGH_Param * IGH_Param * obj

module Utils =
  let nfi = ccl.Utilities.Instance.NumberFormatInfo

  let toString (x:'a) = 
    match FSharpValue.GetUnionFields(x, typeof<'a>) with
    | case, _ -> case.Name

  let fromString<'a> (s:string) =
    match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
    |[|case|] -> Some(FSharpValue.MakeUnion(case,[||]) :?> 'a)
    |_ -> None

  /// Get XML-compliant name for given string
  let GetXmlName n =
    let mutable sb = new StringBuilder()
    sb <- sb.Append(n.ToString().ToLowerInvariant())
    sb <- sb.Replace(' ', '_')
    sb <- sb.Replace(':', '_')
    sb <- sb.Replace('(', '_')
    sb <- sb.Replace(')', '_')
    sb <- sb.Replace(')', '_')
    sb <- sb.Replace(')', '_')
    sb.ToString()

  /// Give message if true, else empty string ""
  let SetMessage t m = match t with true -> m | _ -> ""

  let Samples = 50

  let Logarithm (a:float) (b:float) = (/) (log a) (log b)

  let GreaterThan a b = match (>) a b with true -> 1.0 | _ -> 0.0
  let LessThen a b = match (<) a b with true -> 1.0 | _ -> 0.0

  /// Give first (R) component of triplet (IntColor)
  let R (_r:int, _:int, _:int) = _r
  /// Give second (G) component of triplet (IntColor)
  let G (_:int, _g:int, _:int) = _g
  /// Give third (B) component of triplet (IntColor)
  let B (_:int, _:int, _b:int) = _b

  let FromC (x:obj, _:IGH_Param, _:IGH_Param, _:obj) = x
  let FromS (_:obj, x:IGH_Param, _:IGH_Param, _:obj) = x
  let ToS (_:obj, _:IGH_Param, x:IGH_Param, _:obj) = x
  let ToC (_:obj, _:IGH_Param, _:IGH_Param, x:obj) = x

  let rnd = new Random()

  /// Convert a byte channel to float
  let RGBChanToFloat (b:byte) = (float32 b)/255.0f

  let IntColorFromColor (c:Color) =
    ((int c.R), (int c.G), (int c.B))

  let ColorXml (c:Color) =
    String.Format(nfi, "{0} {1} {2}", RGBChanToFloat(c.R), RGBChanToFloat(c.G), RGBChanToFloat(c.B))

  /// Read color from given component data access at index idx. component
  /// message will be set to msg if reading the data failed.
  /// Returns an IntColor.
  let readColor(u:GH_Component, DA:IGH_DataAccess, idx:int, msg) : IntColor =
    let mutable c = new GH_Colour()
    let r = DA.GetData(idx, &c)
    u.Message <- SetMessage (not r) msg
    IntColorFromColor(c.Value)

  /// Read float from given component data access at index idx. component
  /// message will be set to msg if reading the data failed.
  /// Returns a float.
  let readFloat(u:GH_Component, DA:IGH_DataAccess, idx:int, msg) : float =
    let mutable f = new GH_Number()
    let r = DA.GetData(idx, &f)
    u.Message <- SetMessage (not r) msg
    f.Value

  /// Create a GH_Colour from given IntColor
  let createColor c = new GH_Colour(Color.FromArgb((R c), (G c), (B c)))

  /// Average out given IntColor with Utils.Samples
  let AvgColor c = ((R c) / Samples, (G c) / Samples, (B c) / Samples)

  /// Weight two IntColors given fac. A fac of 0.0 will yield c2,
  /// a fac of 1.0 will yield c1
  let WeightColors c1 c2 fac : IntColor =
    let choosecolor a b =
      match rnd.NextDouble() with i when i < fac -> a | _ -> b
    let cadder a b = ((R a) + (R b), (G a) + (G b), (B a) + (B b))

    List.init Samples (fun _ -> choosecolor c1 c2) |> List.reduce cadder |> AvgColor

  /// Cast an object as 'T, or null if that fails
  let castAs<'T when 'T : null> (o:obj) =
    match o with :? 'T as res -> res | _ -> null

  let GetDataXml (inp:IGH_Param) =
    match inp.SourceCount=1 with
    | true -> ("", "")
    | false ->
      let in1 = inp.VolatileData.Branch(0).[0]
      let intype = in1.GetType()
      match intype with
      | intype when intype.IsEquivalentTo(typeof<GH_Colour>) ->
          let c = castAs<GH_Colour>(in1)
          (inp.Name, ColorXml(c.Value))
      | intype when intype.IsEquivalentTo(typeof<GH_Vector>) ->
          let c = castAs<GH_Vector>(in1)
          (inp.Name, c.Value.ToString().Replace("(", "").Replace(")", "").Replace(",", " "))
      | _ ->
          (inp.Name.ToLowerInvariant(), String.Format(nfi, "{0}", in1))

  /// Get data XML representation from given input list
  let GetInputsXml (inputs:List<IGH_Param>) =
    String.Concat([for i in inputs -> 
                    let t = GetDataXml(i)
                    match (fst t) with "" -> "" | _ -> (fst t).ToLowerInvariant() + "=\""+ (snd t) + "\" "
    ])

  let GetNodeXml node name data =
    node + " name=\"" + name + "\" " + data

type BlendNode() =
  inherit GH_Component("Blend", "blend", "Blend two BSDF nodes", "Shader", "Operation")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddColourParameter("Closure1", "1", "First closure input", GH_ParamAccess.item, Color.Aquamarine) |> ignore
    mgr.AddColourParameter("Closure2", "2", "Second closure input", GH_ParamAccess.item, Color.DeepPink) |> ignore
    mgr.AddNumberParameter("Fac", "F", "0.0 full first closure, 1.0 full second", GH_ParamAccess.item, 0.5) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Closure", "C", "Blend of Closure1 and Closure2", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("133f2f95-926f-4ab4-bc8b-5f96e106d3e4")

  override u.Icon = Icons.Blend

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- ""
    let c1 = Utils.readColor(u, DA, 0, "Couldn't read Closure 1")
    let c2 = Utils.readColor(u, DA, 1, "Couldn't read Closure 2")
    let f = Utils.readFloat(u, DA, 2, "Couldn't read Fac")
    let nsc = Utils.WeightColors c1 c2 f
    DA.SetData(0, Utils.createColor nsc) |> ignore

  interface ICyclesNode with
    member u.NodeName = "mix_closure"

    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      Utils.GetNodeXml node nickname x

type TransparentBsdf() =
  inherit GH_Component("Transparent BSDF", "transparent", "Transparent BSDF node for shader graph", "Shader", "BSDF")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddColourParameter("Color", "C", "transparent color", GH_ParamAccess.item, Color.Gray) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("BSDF", "BSDF", "Transparent BSDF", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("15f77ebf-ae59-4c49-80b1-362a7168f85f")

  override u.Icon = Icons.Diffuse

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- ""
    let c = Utils.readColor(u, DA, 0, "Couldn't read transparent color")

    DA.SetData(0, Utils.createColor c) |> ignore

  interface ICyclesNode with
    member u.NodeName = "transparent_bsdf"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      Utils.GetNodeXml node nickname x

type DiffuseBsdf() =
  inherit GH_Component("Diffuse BSDF", "diffuse", "Diffuse BSDF node for shader graph", "Shader", "BSDF")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddColourParameter("Color", "C", "diffuse color", GH_ParamAccess.item, Color.Gray) |> ignore
    mgr.AddNumberParameter("Roughness", "R", "Roughness of diffuse bsdf", GH_ParamAccess.item, 0.0) |> ignore
    mgr.AddVectorParameter("Normal", "N", "Normal", GH_ParamAccess.item, Vector3d.Zero) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("BSDF", "BSDF", "Diffuse BSDF", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("e79bd4ac-1aa0-450d-aa4a-495cfeb8cb13")

  override u.Icon = Icons.Diffuse

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- ""
    let c = Utils.readColor(u, DA, 0, "Couldn't read diffuse color")
    let f = Utils.readFloat(u, DA, 1, "Couldn't read diffuse roughness")

    DA.SetData(0, Utils.createColor c) |> ignore

  interface ICyclesNode with
    member u.NodeName = "diffuse_bsdf"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      Utils.GetNodeXml node nickname x

type TextureCoordinate() =
  inherit GH_Component("Texture Coordinate", "texcoord", "Texture Coordinate for point being sampled", "Shader", "Input")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    ()

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddVectorParameter("Generated", "G", "Generated", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("Normal", "N", "Normal", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("UV", "UV", "UV", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("Object", "O", "Object", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("Camera", "C", "Camera", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("Window", "W", "Window", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("Reflection", "R", "Reflection", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("WcsBox", "WCS", "WcsBox", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvSpherical", "Sph", "EnvSpherical", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvEmap", "Emap", "EnvEmap", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvBox", "Ebox", "EnvBox", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvLightProbe", "LP", "EnvLightProbe", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvCubemap", "Cube", "EnvCubemap", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvCubemapVerticalCross", "CubeV", "EnvCubemapVerticalCross", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvCubemapHorizontalCross", "CubeH", "EnvCubemapHorizontalCross", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvHemi", "Hemi", "EnvHemi", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("d78aa03c-713b-43b8-a478-7edfe75cf148")

  override u.Icon = Icons.TC

  override u.SolveInstance(DA: IGH_DataAccess) =
    DA.SetData(0, Vector3d.Zero) |> ignore
    DA.SetData(1, Vector3d.Zero) |> ignore
    DA.SetData(2, Vector3d.Zero) |> ignore
    DA.SetData(3, Vector3d.Zero) |> ignore
    DA.SetData(4, Vector3d.Zero) |> ignore
    DA.SetData(5, Vector3d.Zero) |> ignore
    DA.SetData(6, Vector3d.Zero) |> ignore
    DA.SetData(7, Vector3d.Zero) |> ignore
    DA.SetData(8, Vector3d.Zero) |> ignore
    DA.SetData(9, Vector3d.Zero) |> ignore
    DA.SetData(10, Vector3d.Zero) |> ignore
    DA.SetData(11, Vector3d.Zero) |> ignore
    DA.SetData(12, Vector3d.Zero) |> ignore
    DA.SetData(13, Vector3d.Zero) |> ignore
    DA.SetData(14, Vector3d.Zero) |> ignore
    DA.SetData(15, Vector3d.Zero) |> ignore

  interface ICyclesNode with
    member u.NodeName = "texture_coordinate"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      Utils.GetNodeXml node nickname x

type Distribution = Sharp | Beckmann | GGX | Asihkmin_Shirley with
  member u.toString = Utils.toString u
  static member fromString s = Utils.fromString<Distribution> s

type GlassBsdf() =
  inherit GH_Component("Glass BSDF", "glass", "Glass BSDF node for shader graph", "Shader", "BSDF")

  member val Distribution = Sharp with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddColourParameter("Color", "C", "glass color", GH_ParamAccess.item, Color.DarkBlue) |> ignore
    mgr.AddNumberParameter("Roughness", "R", "Roughness of glass bsdf", GH_ParamAccess.item, 0.0) |> ignore
    mgr.AddNumberParameter("IOR", "I", "IOR of glass bsdf", GH_ParamAccess.item, 1.4) |> ignore
    mgr.AddVectorParameter("Normal", "N", "Normal", GH_ParamAccess.item, Vector3d.Zero) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("BSDF", "BSDF", "Glass BSDF", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("4db00f7b-fa70-4130-813d-a9f7cd193795")

  override u.Icon = Icons.Glossy

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- ""
    let c = Utils.readColor(u, DA, 0, "Couldn't read glass color")
    let f = Utils.readFloat(u, DA, 1, "Couldn't read glass roughness")

    u.Message <- u.Distribution.toString.Replace('_', ' ')

    DA.SetData(0, Utils.createColor c) |> ignore

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Distribution", u.Distribution.toString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Distribution") then
      u.Distribution <-
        let d = Distribution.fromString (reader.GetString "Distribution")
        match d with | None -> Sharp | _ -> d.Value

    base.Read(reader)

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let sharphandler _ _ = u.Distribution <- Sharp; u.ExpireSolution true
    let beckmannhandler _ _ = u.Distribution <- Beckmann; u.ExpireSolution true
    let ggxhandler _ _ = u.Distribution <- GGX; u.ExpireSolution true
    let asihkminhandler _ _ = u.Distribution <- Asihkmin_Shirley; u.ExpireSolution true
    GH_DocumentObject.Menu_AppendItem(menu, "Sharp", sharphandler, true, u.Distribution = Sharp) |> ignore
    GH_DocumentObject.Menu_AppendItem(menu, "Beckmann", beckmannhandler, true, u.Distribution = Beckmann) |> ignore
    GH_DocumentObject.Menu_AppendItem(menu, "GGX", ggxhandler, true, u.Distribution = GGX) |> ignore
    GH_DocumentObject.Menu_AppendItem(menu, "Asihkmin Shirley", asihkminhandler, true, u.Distribution = Asihkmin_Shirley) |> ignore

  interface ICyclesNode with
    member u.NodeName = "glass_bsdf"
    member u.GetXml node nickname inputs =
      let x = (Utils.GetInputsXml inputs) + String.Format(" distribution=\"{0}\"", u.Distribution.toString.Replace('_', ' '))
      Utils.GetNodeXml node nickname x

type GlossyBsdf() =
  inherit GH_Component("Glossy BSDF", "glossy", "Glossy BSDF node for shader graph", "Shader", "BSDF")

  member val Distribution = Sharp with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddColourParameter("Color", "C", "glossy color", GH_ParamAccess.item, Color.DarkBlue) |> ignore
    mgr.AddNumberParameter("Roughness", "R", "Roughness of glossy bsdf", GH_ParamAccess.item, 0.0) |> ignore
    mgr.AddVectorParameter("Normal", "N", "Normal", GH_ParamAccess.item, Vector3d.Zero) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("BSDF", "BSDF", "Glossy BSDF", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("84e014b7-a76a-4b4f-8d37-25696cbebc04")

  override u.Icon = Icons.Glossy

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- ""
    let c = Utils.readColor(u, DA, 0, "Couldn't read glossy color")
    let f = Utils.readFloat(u, DA, 1, "Couldn't read glossy roughness")

    u.Message <- u.Distribution.toString.Replace('_', ' ')

    DA.SetData(0, Utils.createColor c) |> ignore

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Distribution", u.Distribution.toString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Distribution") then
      u.Distribution <-
        let d = Distribution.fromString (reader.GetString "Distribution")
        match d with | None -> Sharp | _ -> d.Value

    base.Read(reader)

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let sharphandler _ _ = u.Distribution <- Sharp; u.ExpireSolution true
    let beckmannhandler _ _ = u.Distribution <- Beckmann; u.ExpireSolution true
    let ggxhandler _ _ = u.Distribution <- GGX; u.ExpireSolution true
    let asihkminhandler _ _ = u.Distribution <- Asihkmin_Shirley; u.ExpireSolution true
    GH_DocumentObject.Menu_AppendItem(menu, "Sharp", sharphandler, true, u.Distribution = Sharp) |> ignore
    GH_DocumentObject.Menu_AppendItem(menu, "Beckmann", beckmannhandler, true, u.Distribution = Beckmann) |> ignore
    GH_DocumentObject.Menu_AppendItem(menu, "GGX", ggxhandler, true, u.Distribution = GGX) |> ignore
    GH_DocumentObject.Menu_AppendItem(menu, "Asihkmin Shirley", asihkminhandler, true, u.Distribution = Asihkmin_Shirley) |> ignore

  interface ICyclesNode with
    member u.NodeName = "glossy_bsdf"
    member u.GetXml node nickname inputs =
      let x = (Utils.GetInputsXml inputs) + String.Format(" distribution=\"{0}\"", u.Distribution.toString.Replace('_', ' '))
      Utils.GetNodeXml node nickname x

type EmissionBsdf() =
  inherit GH_Component("Emission BSDF", "emission", "Emission BSDF node for shader graph", "Shader", "BSDF")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddColourParameter("Color", "C", "emission color", GH_ParamAccess.item, Color.NavajoWhite) |> ignore
    mgr.AddNumberParameter("Strength", "S", "Roughness of emission bsdf", GH_ParamAccess.item, 0.0) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Emission", "E", "Emission BSDF", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("aa365407-8e36-4400-b1a7-46cde5b21de6")

  override u.Icon = Icons.Emission

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- ""
    let c = Utils.readColor(u, DA, 0, "Couldn't read emission color")
    let f = Utils.readFloat(u, DA, 1, "Couldn't read emission strength")

    DA.SetData(0, Utils.createColor c) |> ignore

  interface ICyclesNode with
    member u.NodeName = "emission"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      Utils.GetNodeXml node nickname x

type LayerWeightNode() =
  inherit GH_Component("Layer Weight", "layer weight", "Layer weight", "Shader", "Input")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddNumberParameter("Blend", "B", "Blend factor 0.0-1.0", GH_ParamAccess.item, 0.5) |> ignore
    mgr.AddVectorParameter("Normal", "N", "Roughness of emission bsdf", GH_ParamAccess.item, Vector3d.Zero) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddNumberParameter("Fresnel", "Fr", "Fresnel", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Facing", "Fa", "Fresnel", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("5576ff9f-99f7-4611-aa42-dcc4b6c621ac")

  override u.Icon = Icons.Emission

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- ""

    DA.SetData(0, 0.5) |> ignore
    DA.SetData(1, 0.5) |> ignore

  interface ICyclesNode with
    member u.NodeName = "layer_weight"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      Utils.GetNodeXml node nickname x

[<AbstractClass>]
type MathBaseNode(name, nickname, description, category, subcategory, outputdescription, op) =
  inherit GH_Component(name, nickname, description, category, subcategory)

  let mutable m_useclamp = false

  member u.outputdesc = outputdescription
  member u.op = op

  member u.UseClamp
    with get() = m_useclamp
    and set(value) = m_useclamp <- value

//  override u.CreateAttributes() =
//    u.m_attributes <- new MathBaseAttributes(u)

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddNumberParameter("Value1", "1", "First value", GH_ParamAccess.item, 0.0) |> ignore
    mgr.AddNumberParameter("Value2", "2", "Second value", GH_ParamAccess.item, 0.0) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddNumberParameter("Value", "R", u.outputdesc, GH_ParamAccess.item) |> ignore

  override u.SolveInstance(DA : IGH_DataAccess) =
    let v1 = Utils.readFloat(u, DA, 0, "Couldn't read Value1")
    let v2 = Utils.readFloat(u, DA, 1, "Couldn't read Value2")

    let r = match u.UseClamp with true -> u.Message <- "UseClamp = true"; max (min (u.op v1 v2) 1.0) 0.0 | _ -> u.Message <- "UseClamp = false"; u.op v1 v2

    DA.SetData(0, r) |> ignore

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetBoolean("UseClamp", u.UseClamp)
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("UseClamp") then
      u.UseClamp <- reader.GetBoolean("UseClamp")

    base.Read(reader)

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let useclamp_handler _ _ = u.UseClamp <- not u.UseClamp; u.ExpireSolution true
    GH_DocumentObject.Menu_AppendItem(menu, "Use Clamp", useclamp_handler, true, u.UseClamp) |> ignore

  interface ICyclesNode with
    member u.NodeName = "math"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      Utils.GetNodeXml node nickname (x + String.Format(" operation=\"{0}\"", u.Name))

type MathAdd() =
  inherit MathBaseNode("Add", "add", "Add two floats", "Shader", "Math", "Value1+Value2", (+))
  override u.ComponentGuid = new Guid("ec3b4eb3-7cd5-43c8-8ef7-deb2200df882")
  override u.Icon = Icons.Add

type MathSubtract() =
  inherit MathBaseNode("Subtract", "subtract", "Subtract two floats", "Shader", "Math", "Value1-Value2", (-))
  override u.ComponentGuid = new Guid("c2b99ede-3050-483d-ab90-35a1548d2d22")
  override u.Icon = Icons.Sub

type MathMultiply() =
  inherit MathBaseNode("Multiply", "multiply", "Multiply two floats", "Shader", "Math", "Value1×Value2", (*))
  override u.ComponentGuid = new Guid("4a360292-b84b-4808-ad8e-67f2b77b0e15")
  override u.Icon = Icons.Mult

type MathDivide() =
  inherit MathBaseNode("Divide", "divide", "Divide two floats", "Shader", "Math", "Value1/Value2", (/))
  override u.ComponentGuid = new Guid("623ee461-9576-4981-a85a-7aa4a30e2e98")
  override u.Icon = Icons.Div

type MathPower() =
  inherit MathBaseNode("Power", "power", "Power two floats", "Shader", "Math", "Value1**Value2", ( ** ) )
  override u.ComponentGuid = new Guid("2e74876b-33f9-4262-9791-cf53466a63e3")
  override u.Icon = Icons.Pow

type MathLogarithm() =
  inherit MathBaseNode("Logarithm", "logarithm", "Logarithm two floats", "Shader", "Math", "log(Value1)/log(Value2)", Utils.Logarithm)
  override u.ComponentGuid = new Guid("72b96bf7-350e-4408-bdec-d61ea6b4d677")
  override u.Icon = Icons.Log

type MathMinimum() =
  inherit MathBaseNode("Minimum", "min", "Minimum two floats", "Shader", "Math", "min(Value1,Value2)", min)
  override u.ComponentGuid = new Guid("825f2014-0e51-4d60-bb2f-8aa40aea7b91")
  override u.Icon = Icons.Min

type MathMaximum() =
  inherit MathBaseNode("Maximum", "max", "Maximum two floats", "Shader", "Math", "max(Value1,Value2)", max)
  override u.ComponentGuid = new Guid("1706489e-c7a3-453d-971a-1429a51aa783")
  override u.Icon = Icons.Max

type MathLessThan() =
  inherit MathBaseNode("Less Than", "less", "v1 < v2", "Shader", "Math", "Value1 < Value2", Utils.LessThen)
  override u.ComponentGuid = new Guid("94d49a6f-2520-430d-b2ee-6eda864b568b")
  override u.Icon = Icons.LessThan

type MathGreaterThan() =
  inherit MathBaseNode("Greater Than", "greater", "Value1 > Value2", "Shader", "Math", "Value1 > Value2", Utils.GreaterThan)
  override u.ComponentGuid = new Guid("7a833ba4-28f2-4676-8fa0-c746a9dd6b02")
  override u.Icon = Icons.GreaterThan


/// The output node for the shader system. This node is responsible for
/// driving the XML generation of a shader graph.
type OutputNode() =
  inherit GH_Component("Output", "output", "Output node for shader graph", "Shader", "Output")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddColourParameter("Surface", "surface", "connect surface shader tree here.", GH_ParamAccess.item, Color.Black) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Pixel", "P", "Pixel colour through evaluation of the tree", GH_ParamAccess.item) |> ignore
    mgr.AddTextParameter("Xml", "X", "tree as xml", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("14df22af-d119-4f69-a536-34a30ddb175e")

  override u.Icon = Icons.Output

  override u.SolveInstance(DA : IGH_DataAccess) =
    u.Message <- ""

    let doc = u.OnPingDocument()
    // create dictionary of existing nodes and a flag to remember if
    // we have already handled it when generating XML. The flag
    // is there to prevent the same tags from appearing more than once
    let nd = new Dictionary<Guid, bool>()
    for o in doc.Objects do nd.[o.InstanceGuid] <- false

    /// <summary>
    /// Generate XML representation for given <c>GH_Component</c>
    /// </summary>
    /// <param name="n">GH_Component to generate XML representation of</param>
    let ComponentToXml (n:GH_Component) =

      /// Get the NodeName from a <c>GH_Component</c>. If <c>GH_Component</c> doesn't
      /// implement <c>ICyclesNode</c> this will be the empty string "".
      let nodename (b1 : GH_Component) = match box b1 with :? ICyclesNode as cn -> cn.NodeName | _ -> b1.Name

      /// Get the XML from a <c>GH_Component</c>. If <c>GH_Component</c> doesn't
      /// implement <c>ICyclesNode</c> this will be the empty string "".
      let getxml (b1 : GH_Component, nodename, nickname, inps : List<IGH_Param>) =
        match box b1 with
        | :? ICyclesNode as cn -> cn.GetXml nodename nickname inps
        | _ -> Utils.GetNodeXml b1.Name (b1.InstanceGuid.ToString()) ""

      let dontdoit = nd.[n.InstanceGuid] || n.ComponentGuid=u.ComponentGuid
      let nodn = nodename(n)
      let nn =
        match n.ComponentGuid.ToString() with
        | "14df22af-d119-4f69-a536-34a30ddb175e" -> "output"
        | _ -> n.InstanceGuid.ToString()

      let xml = getxml(n, nodn, nn, n.Params.Input)
      match dontdoit with true -> "" | _ ->
                                      match nn with
                                        | "" -> ""
                                        | "output" -> ""
                                        | _ ->
                                          nd.[n.InstanceGuid] <- true
                                          "<" + xml + "/>"

    let ValueNodeXml (n:GH_NumberSlider) =
      let dontdoit = nd.[n.InstanceGuid] || n.ComponentGuid=u.ComponentGuid
      let nn = n.InstanceGuid.ToString()
      match dontdoit with
      | true -> ""
      | _ ->
        nd.[n.InstanceGuid] <- true
        String.Format(ccl.Utilities.Instance.NumberFormatInfo, "<value name=\"{0}\" value=\"{1}\" />\n", nn, n.CurrentValue)

    let ColorNodeXml (n:GH_ColourPickerObject) =
      let dontdoit = nd.[n.InstanceGuid] || n.ComponentGuid=u.ComponentGuid
      let nn = n.InstanceGuid.ToString()
      match dontdoit with
      | true -> ""
      | _ ->
        nd.[n.InstanceGuid] <- true
        String.Format(ccl.Utilities.Instance.NumberFormatInfo, "<color name=\"{0}\" value=\"{1}\" />\n", nn, Utils.ColorXml(n.Colour))

    /// Get all XML node tags for all the nodes that are connected to the
    /// given node.
    let CollectNodeTags (n:GH_Component) =
      /// tail-recursively generate all tags
      let rec colnodetags (_n:obj, acc) =
        match _n with
        | null -> acc
        | _ ->
          let ntype = _n.GetType()
          match ntype with
          | ntype when ntype.IsEquivalentTo(typeof<GH_NumberSlider>) -> acc + ValueNodeXml(_n :?> GH_NumberSlider)
          | ntype when ntype.IsEquivalentTo(typeof<GH_ColourPickerObject>) -> acc + ColorNodeXml(_n :?> GH_ColourPickerObject)
          | _ ->
            let n = Utils.castAs<GH_Component>(_n)
            let compxml = ComponentToXml(n)
            let lf =
              match compxml with
              | "" -> ""
              | _ -> "\n"
            let comp_attrs = [
              for inp in n.Params.Input do
                for s in inp.Sources -> 
                  let st = s.GetType()
                  let tst =
                    match st with
                    | st when st.IsEquivalentTo(typeof<GH_NumberSlider>) -> Utils.castAs<obj>(s)
                    | st when st.IsEquivalentTo(typeof<GH_ColourPickerObject>) -> Utils.castAs<obj>(s)
                    | _ -> 
                      let attrp = Utils.castAs<GH_ComponentAttributes>(s.Attributes.Parent)
                      match attrp with
                      | null -> null
                      | _ -> Utils.castAs<obj>(attrp.Owner)
                  tst]

            /// generate string for inputs of this component
            /// this essentially recurses back into colnodetags.
            /// Tail recursive.
            let rec compstr lst accum =
              match lst with
              | [] -> accum
              | (x:obj)::xs ->
                match x with
                | null -> accum
                | _ ->
                  let nodetags = colnodetags (x, accum)
                  // recurse
                  compstr xs nodetags
            // start iterating over all attributes of attached nodes
            // given this component XML
            compstr comp_attrs acc + compxml+lf

      colnodetags (n, "")

    let CollectConnectTags (n:GH_Component) =
      let doneconns = new Dictionary<SocketsInfo, bool>()
      /// create <connect> tag
      /// <param name="toinp">GH_Component connected to</param>
      /// <param name="tosock">Socket on toinp connected to</param>
      /// <param name="fromsock">Socket on from connected from</param>
      /// <param name="from">GH_Component or other connected from</param>
      let connecttag (sinf:SocketsInfo) = //_toinp:obj, tosock:IGH_Param, fromsock:IGH_Param, from:obj) =
        let MapGhToCycles (comp:obj) (sock:IGH_Param) =
          let t = comp.GetType()
          match t with
          | t when t.IsEquivalentTo(typeof<GH_ColourPickerObject>) -> ("color", "color")
          | t when t.IsEquivalentTo(typeof<GH_NumberSlider>) -> ("value", "value")
          | _ ->
            let n = Utils.castAs<GH_Component>(comp)
            (n.Name, sock.Name.ToLowerInvariant())
        let dontdoit = doneconns.ContainsKey(sinf)
        match dontdoit with
        | true -> ""
        | _ ->
          doneconns.[sinf] <- true
          let toinp = (Utils.ToC sinf) :?> GH_Component
          let from = (Utils.FromC sinf)
          let fromsock = (Utils.FromS sinf)
          let tosock = (Utils.ToS sinf)
          let nn =
            match toinp.ComponentGuid.ToString() with
            | "14df22af-d119-4f69-a536-34a30ddb175e" -> "output"
            | _ -> toinp.InstanceGuid.ToString()

          let fromstr =
            let t = from.GetType()
            match t with
            | t when t.IsEquivalentTo(typeof<GH_ColourPickerObject>) ->
                                                                      let cp = from :?> GH_ColourPickerObject
                                                                      cp.InstanceGuid.ToString()
            | t when t.IsEquivalentTo(typeof<GH_NumberSlider>) ->
                                                                      let ns = from :?> GH_NumberSlider
                                                                      ns.InstanceGuid.ToString()
            | _ -> 
              let c = from :?> GH_Component
              c.InstanceGuid.ToString()
          let (fromcompname, fromsockname) = MapGhToCycles from fromsock
          let (tocompname, tosockname) = MapGhToCycles toinp tosock

          match fromstr with
          | "" -> ""
          | _ -> String.Format("<connect from=\"{0} {1}\" to=\"{2} {3}\" />\n", fromstr, fromsockname, nn, tosockname)

      let rec colcontags (_n:obj, acc) =
        match _n with
        | null -> acc
        | _ ->
          let ntype = _n.GetType()
          match ntype with
          | ntype when ntype.IsEquivalentTo(typeof<GH_NumberSlider>) -> acc
          | ntype when ntype.IsEquivalentTo(typeof<GH_ColourPickerObject>) -> acc
          | _ ->
            let n = Utils.castAs<GH_Component>(_n)

            let comp_attrs = [
              for inp in n.Params.Input do
                for s in inp.Sources -> 
                  let st = s.GetType()
                  let tst =
                    match st with
                    | st when st.IsEquivalentTo(typeof<GH_NumberSlider>) -> Utils.castAs<obj>(s)
                    | st when st.IsEquivalentTo(typeof<GH_ColourPickerObject>) -> Utils.castAs<obj>(s)
                    | _ -> 
                      let attrp = Utils.castAs<GH_ComponentAttributes>(s.Attributes.Parent)
                      match attrp with
                      | null -> null
                      | _ -> Utils.castAs<obj>(attrp.Owner)
                  (tst, s, inp, Utils.castAs<obj>(n))]

            let rec conrec lst accum =
              match lst with
              | [] -> accum
              | (x:SocketsInfo)::xs ->
                match x with
                | (null, _, _, _) -> accum
                | (_, _, _, _) -> 
                  let thistag = connecttag (x) //(Utils.ToC x), (Utils.ToS x), (Utils.FromS x), (Utils.FromC x))
                  let contags = colcontags ((Utils.FromC x), accum)
                  conrec xs contags + thistag
            conrec comp_attrs acc
      colcontags (n, "")

    let nodetagsxml = CollectNodeTags (u) + "\n"
    // reset dictionary flags
    for o in doc.Objects do nd.[o.InstanceGuid] <- false
    let connecttagsxml = CollectConnectTags (u)

    let s = Utils.readColor(u, DA, 0, "Couldn't read Surface")

    DA.SetData(0, Utils.createColor s) |> ignore
    DA.SetData(1, nodetagsxml + connecttagsxml) |> ignore

  interface ICyclesNode with
    member u.NodeName = "output"
    // the output node doesn't generate XML for the shader representation
    // so we return just empty string for XML
    member u.GetXml n nn l = "HAHAHA"

type FsCompTester() =
  inherit GH_Component("FsCompTester", "FSC", "Testbed component", "F#", "Tester")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddColourParameter("Hep", "hep", "connect surface shader tree here.", GH_ParamAccess.item) |> ignore
    mgr.AddColourParameter("Hop", "hop", "connect surface shader tree here.", GH_ParamAccess.item, Color.Black) |> ignore
    mgr.HideParameter(1)
    mgr.Param(0).Optional <- true
    mgr.Param(1).Optional <- true

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Pixel", "P", "Pixel colour through evaluation of the tree", GH_ParamAccess.item) |> ignore
    mgr.AddTextParameter("Xml", "X", "tree as xml", GH_ParamAccess.item) |> ignore
    mgr.HideParameter(0)

  override u.ComponentGuid = new Guid("be3ab036-61b1-4d1d-9dde-b41dfbdf2ecb")

  override u.Icon = Icons.Output

  override u.SolveInstance(DA : IGH_DataAccess) =
    ()

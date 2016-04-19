namespace TextureNodes

open FsShaderGraphComponents

open System
open System.Drawing
open System.Windows.Forms

open Grasshopper.Kernel
open Grasshopper.Kernel.Types

open Rhino.Geometry

open ShaderGraphResources

type NoiseTextureNode() =
  inherit GH_Component("Noise", "noise", "Noise", "Shader", "Texture")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.Zero) |> ignore
    mgr.AddNumberParameter("Scale", "S", "Scale", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Detail", "D", "Detail", GH_ParamAccess.item, 2.0) |> ignore
    mgr.AddNumberParameter("Distortion", "Dist", "Distortion", GH_ParamAccess.item, 0.0) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Fac", "F", "Fac", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("c3632808-8f29-48bd-afc2-0b85ad5763c0")

  override u.Icon = Icons.Emission

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- ""

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  interface ICyclesNode with
    member u.NodeName = "noise_texture"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      "<" + Utils.GetNodeXml node nickname x + " />"

type GradientTypes =
  | Linear
  | Quadratic
  | Easing
  | Diagonal
  | Radial
  | Quadratic_Sphere
  | Spherical
  member u.toString = Utils.toString u
  member u.toStringR = (u.toString)
  static member fromString s = Utils.fromString<GradientTypes> s

type GradientTextureNode() =
  inherit GH_Component("Gradient", "gradient", "Gradient", "Shader", "Texture")

  member val Gradient = Linear with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.Zero) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Fac", "F", "Fac", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("e9d63595-4a09-4351-93f4-acd2a0248a9b")

  override u.Icon = Icons.Emission

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Gradient", u.Gradient.toString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Gradient") then
      u.Gradient <-
        let d = GradientTypes.fromString (reader.GetString "Gradient")
        match d with None -> Linear | _ -> d.Value

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.Gradient.toStringR

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let append_menu (gt:GradientTypes) =
      GH_DocumentObject.Menu_AppendItem(menu, gt.toStringR, (fun _ _ -> u.Gradient <- gt; u.ExpireSolution true), true, u.Gradient = gt) |> ignore
    append_menu Linear
    append_menu Easing
    append_menu Quadratic
    append_menu Diagonal
    append_menu Radial
    append_menu Quadratic_Sphere
    append_menu Spherical

  interface ICyclesNode with
    member u.NodeName = "gradient_texture"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      let t = String.Format(" interpolation=\"{0}\" ", u.Gradient.toStringR)
      "<" + Utils.GetNodeXml node nickname (x+t) + " />"

type MusgraveTypes =
  | Multifractal
  | FBM
  | Hybrid_Multifractal
  | Ridged_Multifractal
  | Hetero_Terrain
  member u.toString = Utils.toString u
  member u.toStringR = (u.toString).Replace("FB", "fB").Replace("_", " ")
  static member fromString s = Utils.fromString<MusgraveTypes> s

type MusgraveTextureNode() =
  inherit GH_Component("Musgrave", "musgrave", "Musgrave", "Shader", "Texture")

  member val Musgrave = FBM with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.Zero) |> ignore
    mgr.AddNumberParameter("Scale", "S", "Scale", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Detail", "D", "Detail", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Dimension", "Dim", "Dimension", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Lacunarity", "L", "Lacunarity", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Offset", "O", "Offset", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Gain", "G", "Gain", GH_ParamAccess.item, 5.0) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Fac", "F", "Fac", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("3ed2f77b-373c-4eb8-b6fb-253dff125065")

  override u.Icon = Icons.Emission

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Musgrave", u.Musgrave.toString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Musgrave") then
      u.Musgrave <-
        let d = MusgraveTypes.fromString (reader.GetString "Musgrave")
        match d with None -> FBM | _ -> d.Value

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.Musgrave.toStringR

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let append_menu (gt:MusgraveTypes) =
      GH_DocumentObject.Menu_AppendItem(menu, gt.toStringR, (fun _ _ -> u.Musgrave <- gt; u.ExpireSolution true), true, u.Musgrave = gt) |> ignore
    append_menu Multifractal
    append_menu FBM
    append_menu Hybrid_Multifractal
    append_menu Ridged_Multifractal
    append_menu Hetero_Terrain

  interface ICyclesNode with
    member u.NodeName = "musgrave_texture"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      let t = String.Format(" musgrave_type=\"{0}\" ", u.Musgrave.toStringR)
      "<" + Utils.GetNodeXml node nickname (x+t) + " />"

type ImageTextureNode() =
  inherit GH_Component("Image", "image", "Image", "Shader", "Texture")

  member val ImageFile = "" with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.Zero) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Alpha", "A", "Alpha", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("078f4865-e362-4ed1-818d-94fd9432ac77")

  override u.Icon = Icons.Emission

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Image", u.ImageFile) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Image") then
      u.ImageFile <- reader.GetString "Image"

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.ImageFile

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    GH_DocumentObject.Menu_AppendTextItem(menu, u.ImageFile,
      (fun sender evargs -> u.ImageFile <- sender.Text; u.ExpireSolution(true)),
      (fun sender evargs -> ()),
      false) |> ignore

  interface ICyclesNode with
    member u.NodeName = "image_texture"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      let t = String.Format(" src=\"{0}\" ", u.ImageFile)
      "<" + Utils.GetNodeXml node nickname (x+t) + " />"

type EnvironmentTextureNode() =
  inherit GH_Component("Environment", "environment", "Environment", "Shader", "Texture")

  member val EnvironmentFile = "" with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.Zero) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Alpha", "A", "Alpha", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("07e09721-e982-4ea4-8358-1dfc4d26cda4")

  override u.Icon = Icons.Emission

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Environment", u.EnvironmentFile) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Environment") then
      u.EnvironmentFile <- reader.GetString "Environment"

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.EnvironmentFile

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    GH_DocumentObject.Menu_AppendTextItem(menu, u.EnvironmentFile,
      (fun sender evargs -> u.EnvironmentFile <- sender.Text; u.ExpireSolution(true)),
      (fun sender evargs -> ()),
      false) |> ignore

  interface ICyclesNode with
    member u.NodeName = "environment_texture"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      let t = String.Format(" src=\"{0}\" ", u.EnvironmentFile)
      "<" + Utils.GetNodeXml node nickname (x+t) + " />"


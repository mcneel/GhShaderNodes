namespace FsShaderGraphComponents

open Microsoft.FSharp.Reflection

open System
open System.Collections.Generic
open System.Linq
open System.Drawing
open System.Windows.Forms

open Grasshopper.Kernel
open Grasshopper.Kernel.Attributes
open Grasshopper.Kernel.Types
open Grasshopper.Kernel.Special
open Grasshopper.Kernel.Parameters

open RhinoCyclesCore.Materials

open ShaderGraphResources
open RhinoCyclesCore.Environments

/// type that signals Grasshopper to continue loading. Here we
/// do necessary initialisation
type Priority() = 
    inherit GH_AssemblyPriority()
    override u.PriorityLoad() =
      u |> ignore
      GH_LoadingInstruction.Proceed

/// Grasshopper plug-in assembly information.
type Info() =
    inherit GH_AssemblyInfo()

    override u.Name =
      u |> ignore
      "Shader Nodes"
    override u.Description =
      u |> ignore
      "Create shader graphs for Cycles for Rhino"
    override u.Id =
      u |> ignore
      new Guid("6a051e83-3727-465e-b5ef-74d027a6f73b")
    override u.Icon =
      u |> ignore
      Icons.ShaderGraph
    override u.AuthorName =
      u |> ignore
      "Nathan 'jesterKing' Letwory"
    override u.AuthorContact = 
      u |> ignore
      "nathan@mcneel.com"

// ---------------------------------------

/// interface that shader nodes need to implement to be able to
/// participate in shader XML generation.
type ICyclesNode =
    /// Get the XML name of the node tag.
    abstract member NodeName : string
    /// Get the XML representation of the node. NodeName, NickName, Parameter list, iteration. Returns XML string
    abstract member GetXml : string -> string -> List<IGH_Param> -> int -> string

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

  /// Give message if true, else empty string ""
  let SetMessage t m = match t with true -> m | _ -> ""

  let Samples = 50

  let Logarithm (a:float) (b:float) = (/) (log a) (log b)

  let GreaterThan a b = match (>) a b with true -> 1.0 | _ -> 0.0
  let LessThen a b = match (<) a b with true -> 1.0 | _ -> 0.0

  /// Give first (R) component of triplet (IntColor)
  let R (r:int, _:int, _:int) = r
  /// Give second (G) component of triplet (IntColor)
  let G (_:int, g:int, _:int) = g
  /// Give third (B) component of triplet (IntColor)
  let B (_:int, _:int, b:int) = b

  let FromC (x:obj, _:IGH_Param, _:IGH_Param, _:obj) = x
  let FromS (_:obj, x:IGH_Param, _:IGH_Param, _:obj) = x
  let ToS (_:obj, _:IGH_Param, x:IGH_Param, _:obj) = x
  let ToC (_:obj, _:IGH_Param, _:IGH_Param, x:obj) = x

  let rnd = new Random()

  /// Convert a byte channel to float
  let RGBChanToFloat (b:byte) = (float32 b)/255.0f

  let (|IntColor|) (c:Color) = 
    ((int c.R), (int c.G), (int c.B))
  let IntColorFromColor (c:Color) =
    ((int c.R), (int c.G), (int c.B))

  let ColorXml (c:Color) =
    String.Format(nfi, "{0} {1} {2}", RGBChanToFloat(c.R), RGBChanToFloat(c.G), RGBChanToFloat(c.B))

  /// Read color from given component data access at index idx. component
  /// message will be set to msg if reading the data failed.
  /// Returns an IntColor.
  let readColor(u:GH_Component, da:IGH_DataAccess, idx:int, msg) : IntColor =
    let mutable c = new GH_Colour()
    let r = da.GetData(idx, &c)
    u.Message <- SetMessage (not r) msg
    IntColorFromColor(c.Value)

  /// Read float from given component data access at index idx. component
  /// message will be set to msg if reading the data failed.
  /// Returns a float.
  let readFloat(u:GH_Component, da:IGH_DataAccess, idx:int, msg) : float =
    let mutable f = new GH_Number()
    let r = da.GetData(idx, &f)
    u.Message <- SetMessage (not r) msg
    f.Value

  let randomColor = Color.FromArgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255))

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

  let GetDataXml (inp:IGH_Param, iteration: int) =
    match inp.SourceCount=1 with
    | true -> ("", "")
    | false ->
      let idx =
        match iteration<inp.VolatileDataCount with
        | true -> iteration
        | _ -> inp.VolatileDataCount - 1
      let in1 = inp.VolatileData.StructureProxy.[0].[idx]
      match in1 with
      | :? GH_Colour ->
          let c = castAs<GH_Colour>(in1)
          (inp.Name, ColorXml(c.Value))
      | :? GH_Vector ->
          let c = castAs<GH_Vector>(in1)
          (inp.Name, c.Value.ToString().Replace("(", "").Replace(")", "").Replace(",", " "))
      | _ ->
          (inp.Name.ToLowerInvariant(), String.Format(nfi, "{0}", in1))

  /// Get data XML representation from given input list
  let GetInputsXml (inputs:List<IGH_Param>, iteration:int) =
    String.Concat([for i in inputs -> 
                    let t = GetDataXml(i, iteration)
                    match (fst t) with
                    | "" -> ""
                    | _ -> (fst t).Replace(" ", "_").ToLowerInvariant() + "=\""+ (snd t) + "\" "
    ])

  let GetNodeXml node name data =
    node + " name=\"" + name + "\" " + data

type Interpolation = None | Linear | Closest | Cubic | Smart with
  member u.ToString = Utils.toString u
  member u.ToStringR = (u.ToString).Replace("_", "-")
  static member FromString s = Utils.fromString<Interpolation> s

type EnvironmentProjection = Equirectangular | Mirror_Ball | Wallpaper with
  member u.ToString = Utils.toString u
  member u.ToStringR = (u.ToString).Replace("_", " ")
  static member FromString s = Utils.fromString<EnvironmentProjection> ((s:string).Replace(" ", "_"))

type TextureProjection = Flat | Box | Sphere | Tube with
  member u.ToString = Utils.toString u
  member u.ToStringR = (u.ToString).Replace("_", "-")
  static member FromString s = Utils.fromString<TextureProjection> s

type TextureExtension = Repeat | Extend | Clip with
  member u.ToString = Utils.toString u
  member u.ToStringR = (u.ToString).Replace("_", "-")
  static member FromString s = Utils.fromString<TextureExtension> s

type ColorSpace = None | Color with
  member u.ToString = Utils.toString u
  member u.ToStringR = (u.ToString).Replace("_", "-")
  static member FromString s = Utils.fromString<ColorSpace> s

/// Distributions used in several nodes: Glass, Glossy, Refraction
type Distribution = Sharp | Beckmann | GGX | Ashihkmin_Shirley | Multiscatter_GGX with
  member u.ToString = Utils.toString u
  member u.ToStringR = (u.ToString).Replace("_", "-")
  static member FromString s = Utils.fromString<Distribution> s

type Falloff = Cubic | Gaussian | Burley with
  member u.ToString = Utils.toString u
  static member FromString s = Utils.fromString<Falloff> s

/// The output node for the shader system. This node is responsible for
/// driving the XML generation of a shader graph.
type OutputNode() =
  inherit GH_Component("Output", "output", "Output node for shader graph", "Shader", "Output")

  let mutable matId = ResizeArray<Guid>() //Collections.Generic.List<Guid>()

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    u |> ignore
    mgr.AddColourParameter(
      "Surface", "S", "connect surface shader tree here.", GH_ParamAccess.item, Color.Black) |> ignore
    mgr.AddColourParameter(
      "Volume", "V", "connect volume shader nodes here.", GH_ParamAccess.item, Color.GreenYellow) |> ignore
    mgr.AddNumberParameter(
      "Displacement", "D", "connect displacement nodes here.", GH_ParamAccess.item, 0.0) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    u |> ignore
    mgr.AddTextParameter("Xml", "X", "tree as xml", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid =
    u |> ignore
    new Guid("14df22af-d119-4f69-a536-34a30ddb175e")

  override u.Icon =
    u |> ignore
    Icons.Output

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let rms = Rhino.RhinoDoc.ActiveDoc.RenderMaterials.Where(fun x ->
          not (isNull(Utils.castAs<XmlMaterial>(x)))).Select(fun i -> i.Name, i.Id).Distinct()
    let appendMenu name id =
      let handleMenuClick _ _ =
        match matId.Contains id with
        | false -> matId.Add id |> ignore
        | true -> matId.Remove id |> ignore
        u.ExpireSolution true
      GH_DocumentObject.Menu_AppendItem(menu, name, handleMenuClick, true, matId.Contains id) |> ignore
    rms |> Seq.iter (fun x -> appendMenu (fst x) (snd x)) |> ignore

  member u.IsBackground =
    match u.Params.Input.[0].SourceCount>0 with
    | false -> false
    | true ->
      let rec hasBgNode (n:GH_Component) (acc:bool) (*: bool list*) =
        //let s = n.Params.Input.[0].Sources.[0]
        [for inp in n.Params.Input ->
          match inp.SourceCount>0 with
          | false -> acc
          | true ->
            let s = inp.Sources.[0]
            match s with
            | :? GH_NumberSlider -> acc
            | :? GH_ColourPickerObject -> acc
            | _ -> 
              let attrp = Utils.castAs<GH_ComponentAttributes>(s.Attributes.Parent)
              match attrp with
              | null -> acc
              | _ ->
                match attrp.Owner.ComponentGuid = new Guid("dd68810b-0a0e-4c54-b08e-f46b41e79f32") with
                | true -> acc
                | false -> hasBgNode (Utils.castAs<GH_Component>(attrp.Owner)) acc
        ].Any(fun x -> x)
        
      hasBgNode u false


  override u.SolveInstance(da : IGH_DataAccess) =
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
    let componentToXml (n:GH_Component, iteration) =

      /// Get the NodeName from a <c>GH_Component</c>. If <c>GH_Component</c> doesn't
      /// implement <c>ICyclesNode</c> this will be the empty string "".
      let nodename (b1 : GH_Component) = match box b1 with :? ICyclesNode as cn -> cn.NodeName | _ -> b1.Name

      /// Get the XML from a <c>GH_Component</c>. If <c>GH_Component</c> doesn't
      /// implement <c>ICyclesNode</c> this will be the empty string "".
      let getxml (b1 : GH_Component, nodename, nickname, inps : List<IGH_Param>, iteration) =
        match box b1 with
        | :? ICyclesNode as cn -> cn.GetXml nodename nickname inps iteration
        | _ ->
          "<" + (Utils.GetNodeXml b1.Name (b1.InstanceGuid.ToString()) "") + " />"

      let dontdoit = nd.[n.InstanceGuid] || n.ComponentGuid=u.ComponentGuid
      let nodn = nodename(n)
      let nn =
        match n.ComponentGuid.ToString() with
        | "14df22af-d119-4f69-a536-34a30ddb175e" -> "output"
        | _ -> n.InstanceGuid.ToString()

      let xml = getxml(n, nodn, nn, n.Params.Input, iteration)
      match dontdoit with true -> "" | _ ->
                                      match nn with
                                        | "" -> ""
                                        | "output" -> ""
                                        | _ ->
                                          nd.[n.InstanceGuid] <- true
                                          xml

    let valueNodeXml (n:GH_NumberSlider) =
      let dontdoit = nd.[n.InstanceGuid] || n.ComponentGuid=u.ComponentGuid
      let nn = n.InstanceGuid.ToString() + "_" + n.ImpliedNickName.ToLowerInvariant().Replace(" ", "_")
      match dontdoit with
      | true -> ""
      | _ ->
        nd.[n.InstanceGuid] <- true
        String.Format(ccl.Utilities.Instance.NumberFormatInfo,
                      "<value name=\"{0}\" value=\"{1}\" />\n", nn, n.CurrentValue)

    let vectorNodeXml (n:Param_Vector) =
      let dontdoit = nd.[n.InstanceGuid] || n.ComponentGuid=u.ComponentGuid
      let nn = n.InstanceGuid.ToString() + "_vector"
      match dontdoit with
      | true -> ""
      | _ ->
        nd.[n.InstanceGuid] <- true
        let v = n.VolatileData.StructureProxy.[0].[0]
        let v' = v :?> GH_Vector
        let v3d = v'.QC_Vec()
        String.Format(ccl.Utilities.Instance.NumberFormatInfo,
                      "<combine_xyz name=\"{0}\" X=\"{1}\" Y=\"{2}\" Z=\"{3}\" />\n", nn, v3d.X, v3d.Y, v3d.Z)

    let colorNodeXml (n:GH_ColourPickerObject) =
      let dontdoit = nd.[n.InstanceGuid] || n.ComponentGuid=u.ComponentGuid
      let nn = n.InstanceGuid.ToString()
      match dontdoit with
      | true -> ""
      | _ ->
        nd.[n.InstanceGuid] <- true
        String.Format(ccl.Utilities.Instance.NumberFormatInfo,
                      "<color name=\"{0}\" value=\"{1}\" />\n", nn, Utils.ColorXml(n.Colour))

    /// Get all XML node tags for all the nodes that are connected to the
    /// given node.
    let collectNodeTags (n:GH_Component, iteration) =
      /// tail-recursively generate all tags
      let rec colnodetags (_n:obj, acc) =
        match _n with
        | null -> acc
        | _ ->
          match _n with
          | :? GH_NumberSlider as _n' ->
            acc + valueNodeXml(_n')
          | :? Param_Vector as _n' ->
            acc + vectorNodeXml(_n')
          | :? GH_ColourPickerObject as _n' ->
            acc + colorNodeXml(_n')
          | _ ->
            let n' = Utils.castAs<GH_Component>(_n)
            let compxml = componentToXml(n', iteration)
            let lf =
              match compxml with
              | "" -> ""
              | _ -> "\n"
            let compAttrs = [
              for inp in n'.Params.Input do
                let s =
                  match iteration < inp.SourceCount with
                  | true -> inp.Sources.[iteration]
                  | false -> inp.Sources.LastOrDefault()
                let tst =
                  match isNull s with
                  | true -> null
                  | _ ->
                    let s' =
                      match s with
                      | :? GH_NumberSlider -> s :> obj
                      | :? GH_ColourPickerObject -> s :> obj
                      | s when isNull s -> null
                      | _ -> 
                        let attrp = Utils.castAs<GH_ComponentAttributes>(s.Attributes.Parent)
                        match attrp with
                        | null ->
                            match s with
                            | :? Param_Vector -> s :> obj
                            | _ -> null
                        | _ -> attrp.Owner :> obj
                    s'
                yield tst]
            let filteredCompAttrs = compAttrs |> List.filter (isNull >> not)

            /// generate string for inputs of this component
            /// this essentially recurses back into colnodetags.
            /// Tail recursive.
            let rec compstr lst accum =
              match lst with
              | [] -> accum
              | (x:obj)::xs ->
                  let nodetags = colnodetags (x, accum)
                  // recurse
                  compstr xs nodetags
            // start iterating over all attributes of attached nodes
            // given this component XML
            compstr filteredCompAttrs acc + compxml+lf

      colnodetags (n, "")

    let collectConnectTags (n:GH_Component, iteration:int) =
      let doneconns = new Dictionary<SocketsInfo, bool>()
      /// create <connect> tag
      /// <param name="toinp">GH_Component connected to</param>
      /// <param name="tosock">Socket on toinp connected to</param>
      /// <param name="fromsock">Socket on from connected from</param>
      /// <param name="from">GH_Component or other connected from</param>
      let connecttag (sinf:SocketsInfo) = //_toinp:obj, tosock:IGH_Param, fromsock:IGH_Param, from:obj) =
        let mapGhToCycles (comp:obj) (sock:IGH_Param) =
          match comp with
          | :? GH_ColourPickerObject -> ("color", "color")
          | :? GH_NumberSlider -> ("value", "value")
          | _ ->
            let n = Utils.castAs<GH_Component>(comp)
            (n.Name, sock.Name.ToLowerInvariant().Replace(" ", "_"))
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
            match from  with
            | :? GH_ColourPickerObject ->
                                let cp = from :?> GH_ColourPickerObject
                                cp.InstanceGuid.ToString()
            | :? GH_NumberSlider ->
                                let ns = from :?> GH_NumberSlider
                                ns.InstanceGuid.ToString() + "_" + ns.ImpliedNickName.ToLowerInvariant().Replace(" ", "_")
            | _ -> 
              let c = from :?> GH_Component
              c.InstanceGuid.ToString()
          let (_, fromsockname) = mapGhToCycles from fromsock
          let (_, tosockname) = mapGhToCycles toinp tosock

          match fromstr with
          | "" -> ""
          | _ -> String.Format("<connect from=\"{0} {1}\" to=\"{2} {3}\" />\n", fromstr, fromsockname, nn, tosockname)

      let rec colcontags (_n:obj, acc) =
        match _n with
        | null -> acc
        | _ ->
          match _n with
          | :? GH_NumberSlider -> acc
          | :? GH_ColourPickerObject -> acc
          | _ ->
            let n = Utils.castAs<GH_Component>(_n)

            let compAttrs = [
              for inp in n.Params.Input do
                let s =
                  match iteration < inp.SourceCount with
                  | true -> inp.Sources.[iteration]
                  | false -> inp.Sources.LastOrDefault()
                let tst =
                  match isNull s with
                  | true -> null
                  | false ->
                    match s with
                    | :? GH_NumberSlider -> Utils.castAs<obj>(s)
                    | :? GH_ColourPickerObject -> Utils.castAs<obj>(s)
                    | s when isNull s -> null
                    | _ -> 
                      let attrp = Utils.castAs<GH_ComponentAttributes>(s.Attributes.Parent)
                      match attrp with
                      | null -> null
                      | _ -> Utils.castAs<obj>(attrp.Owner)
                yield (tst, s, inp, Utils.castAs<obj>(n))]

            let filteredCompAttrs = compAttrs |> List.filter (fun (x, _, _, _) -> x |> isNull |> not)

            let rec conrec lst accum =
              match lst with
              | [] -> accum
              | (x:SocketsInfo)::xs ->
                  let thistag = connecttag (x)
                  let contags = colcontags ((Utils.FromC x), accum)
                  conrec xs contags + thistag
            conrec filteredCompAttrs acc
      colcontags (n, "")

    let nodetagsxml = collectNodeTags (u, da.Iteration) + "\n"
    // reset dictionary flags
    for o in doc.Objects do nd.[o.InstanceGuid] <- false
    let connecttagsxml = collectConnectTags (u, da.Iteration)

    // let s = Utils.readColor(u, da, 0, "Couldn't read Surface")
    // let v = Utils.readColor(u, da, 1, "Couldn't read Volume");

    match u.IsBackground with
    | true ->
      let mutable env = Utils.castAs<XmlEnvironment>(Rhino.RhinoDoc.ActiveDoc.CurrentEnvironment.ForBackground)
      match env with
      | null -> u.Message <- "NO BG"
      | _ ->
        env.SetParameter("xml", nodetagsxml + connecttagsxml) |> ignore
        Rhino.RhinoDoc.ActiveDoc.CurrentEnvironment.ForBackground <- env
      ()
    | false ->
      let mutable m = 
        match matId.Count with
        | 0 -> null
        | _ ->
          let midx =
            match da.Iteration < matId.Count with
            | true -> da.Iteration
            | false -> matId.Count - 1
          Rhino.RhinoDoc.ActiveDoc.RenderMaterials.Where(fun m -> m.Id = matId.[midx]).FirstOrDefault()
      match m with
      | null ->
        u.Message <- "NO MATERIAL"
      | _ ->
        let m' = m :?> XmlMaterial
        m'.BeginChange(Rhino.Render.RenderContent.ChangeContexts.Ignore)
        m'.SetParameter("xml", nodetagsxml + connecttagsxml) |> ignore
        m'.EndChange()
        match matId.Count > 1 with
        | true -> u.Message <- "multiple materials set"
        | _ -> u.Message <- m.Name
        for mm in Rhino.RhinoDoc.ActiveDoc.Materials.Where(fun x -> x.RenderMaterialInstanceId = m.Id) do
          mm.DiffuseColor <- Utils.randomColor
          mm.CommitChanges() |> ignore


    da.SetData(0, nodetagsxml + connecttagsxml) |> ignore

  interface ICyclesNode with
    member u.NodeName =
      u |> ignore
      "output"
    // the output node doesn't generate XML for the shader representation
    // so we return just empty string for XML
    member u.GetXml n nn l i =
      (u, n, nn, l, i) |> ignore
      "HAHAHA"

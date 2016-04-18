namespace FsShaderGraphComponents

open Microsoft.FSharp.Reflection

open System
open System.Text
open System.Collections.Generic
open System.Linq
open System.Drawing
open System.Windows.Forms

open Grasshopper.Kernel
open Grasshopper.Kernel.Attributes
open Grasshopper.Kernel.Types
open Grasshopper.Kernel.Special

open RhinoCyclesCore.Materials

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
                    match (fst t) with "" -> "" | _ -> (fst t).Replace(" ", "_").ToLowerInvariant() + "=\""+ (snd t) + "\" "
    ])

  let GetNodeXml node name data =
    node + " name=\"" + name + "\" " + data

/// Distributions used in several nodes: Glass, Glossy, Refraction
type Distribution = Sharp | Beckmann | GGX | Asihkmin_Shirley with
  member u.toString = Utils.toString u
  member u.toStringR = (u.toString).Replace("_", " ")
  static member fromString s = Utils.fromString<Distribution> s

type Falloff = Cubic | Gaussian | Burley with
  member u.toString = Utils.toString u
  static member fromString s = Utils.fromString<Falloff> s

/// The output node for the shader system. This node is responsible for
/// driving the XML generation of a shader graph.
type OutputNode() =
  inherit GH_Component("Output", "output", "Output node for shader graph", "Shader", "Output")

  member val MatId = Guid.Empty with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddColourParameter("Surface", "S", "connect surface shader tree here.", GH_ParamAccess.item, Color.Black) |> ignore
    mgr.AddColourParameter("Volume", "V", "connect volume shader nodes here.", GH_ParamAccess.item, Color.GreenYellow) |> ignore
    mgr.AddNumberParameter("Displacement", "D", "connect displacement nodes here.", GH_ParamAccess.item, 0.0) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddTextParameter("Xml", "X", "tree as xml", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("14df22af-d119-4f69-a536-34a30ddb175e")

  override u.Icon = Icons.Output

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let append_menu name id =
      GH_DocumentObject.Menu_AppendItem(menu, name, (fun _ _ -> u.MatId <- id; u.ExpireSolution true), true, u.MatId = id) |> ignore
    //let rms = Rhino.RhinoDoc.ActiveDoc.RenderMaterials.Where(fun rm -> rm.TypeName = "Cycles Xml")
    let mc = Rhino.RhinoDoc.ActiveDoc.Materials.Count
    let rms = Rhino.RhinoDoc.ActiveDoc.Materials.Select(fun i -> i.Name, i.Id )
    for rm in rms do append_menu (fst rm) (snd rm)

  member u.IsBackground =
    match u.Params.Input.[0].SourceCount>0 with
    | false -> false
    | true ->
      let s = u.Params.Input.[0].Sources.[0]
      let st = s.GetType()
      match st with
      | st when st.IsEquivalentTo(typeof<GH_NumberSlider>) -> false
      | st when st.IsEquivalentTo(typeof<GH_ColourPickerObject>) -> false
      | _ -> 
        let attrp = Utils.castAs<GH_ComponentAttributes>(s.Attributes.Parent)
        match attrp with
        | null -> false
        | _ -> attrp.Owner.ComponentGuid = new Guid("dd68810b-0a0e-4c54-b08e-f46b41e79f32")


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
        | _ ->
          "<" + (Utils.GetNodeXml b1.Name (b1.InstanceGuid.ToString()) "") + " />"

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
                                          xml

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
                  let thistag = connecttag (x)
                  let contags = colcontags ((Utils.FromC x), accum)
                  conrec xs contags + thistag
            conrec comp_attrs acc
      colcontags (n, "")

    let nodetagsxml = CollectNodeTags (u) + "\n"
    // reset dictionary flags
    for o in doc.Objects do nd.[o.InstanceGuid] <- false
    let connecttagsxml = CollectConnectTags (u)

    let s = Utils.readColor(u, DA, 0, "Couldn't read Surface")
    let v = Utils.readColor(u, DA, 1, "Couldn't read Volume");

    let mutable m = Rhino.RhinoDoc.ActiveDoc.Materials.Where(fun m -> m.Id = u.MatId).FirstOrDefault()
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
      match m with
      | null ->
        u.Message <- "NO MATERIAL"
      | _ ->
        let rm = m.RenderMaterial
        Utils.castAs<XmlMaterial>(rm).SetParameter("xml", nodetagsxml + connecttagsxml) |> ignore
        u.Message <- m.Name
        m.DiffuseColor <- Utils.randomColor
        rm.SimulateMaterial(&m, true)
        m.CommitChanges() |> ignore


    DA.SetData(0, nodetagsxml + connecttagsxml) |> ignore

  interface ICyclesNode with
    member u.NodeName = "output"
    // the output node doesn't generate XML for the shader representation
    // so we return just empty string for XML
    member u.GetXml n nn l = "HAHAHA"

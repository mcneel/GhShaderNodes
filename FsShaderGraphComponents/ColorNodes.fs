namespace ColorNodes

open FsShaderGraphComponents

open System
open System.Linq
open System.Drawing
open System.Windows.Forms

open Grasshopper.Kernel
open Grasshopper.Kernel.Types

open Rhino.Geometry

open ShaderGraphResources

type BlendTypes =
  | Mix
  | Add
  | Multiply
  | Screen
  | Overlay
  | Subtract
  | Divide
  | Difference
  | Darken
  | Lighten
  | Dodge
  | Burn
  | Hue
  | Saturation
  | Value
  | Color
  | Soft_Light
  | Linear_Light with
  member u.toString = Utils.toString u
  member u.toStringR = (u.toString).Replace("_", " ")
  static member fromString s = Utils.fromString<BlendTypes> s


type MixRgbNode() =
  inherit GH_Component("Mix", "mix", "Mix two color nodes, using specified blend type", "Shader", "Color")

  member val Blend = Mix with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddColourParameter("Color1", "1", "First color input", GH_ParamAccess.item, Color.Gainsboro) |> ignore
    mgr.AddColourParameter("Color2", "2", "Second color input", GH_ParamAccess.item, Color.GreenYellow) |> ignore
    mgr.AddNumberParameter("Fac", "F", "0.0 full first color, 1.0 full second", GH_ParamAccess.item, 0.5) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Blend of Color1 and Color2", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("c3a397a6-f760-4ea1-9700-5722eee58489")

  override u.Icon = Icons.Blend

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let append_menu (bt:BlendTypes) =
      GH_DocumentObject.Menu_AppendItem(menu, bt.toStringR, (fun _ _ -> u.Blend <- bt; u.ExpireSolution true), true, u.Blend = bt) |> ignore
    append_menu Mix
    append_menu Add
    append_menu Multiply
    append_menu Screen
    append_menu Overlay
    append_menu Subtract
    append_menu Divide
    append_menu Difference
    append_menu Darken
    append_menu Lighten
    append_menu Dodge
    append_menu Burn
    append_menu Hue
    append_menu Saturation
    append_menu Value
    append_menu Color
    append_menu Soft_Light
    append_menu Linear_Light

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Blend", u.Blend.toString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Blend") then
      u.Blend <-
        let d = BlendTypes.fromString (reader.GetString "Blend")
        match d with | Option.None -> Mix | _ -> d.Value

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    let c1 = Utils.readColor(u, DA, 0, "Couldn't read Color 1")
    let c2 = Utils.readColor(u, DA, 1, "Couldn't read Color 2")
    let f = Utils.readFloat(u, DA, 2, "Couldn't read Fac")
    let nsc = Utils.WeightColors c1 c2 f
    u.Message <- u.Blend.toStringR
    DA.SetData(0, Utils.createColor nsc) |> ignore

  interface ICyclesNode with
    member u.NodeName = "mix"

    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      let t = String.Format(" type=\"{0}\" ", u.Blend.toStringR)
      "<" + (Utils.GetNodeXml node nickname (x+t)) + "/>"

type InterpolationTypes =
  | Constant
  | Linear
  | Ease
  member u.toString = Utils.toString u
  member u.toStringR = (u.toString)
  static member fromString s = Utils.fromString<InterpolationTypes> s

type ColorRampNode() =
  inherit GH_Component("Color Ramp", "color ramp", "Convert a float to a color according a gradient specification (RGB only)", "Shader", "Color")

  member val Interpolation = Linear with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddColourParameter("Stop Colours", "SC", "List of colours", GH_ParamAccess.list) |> ignore
    mgr.AddNumberParameter("Stop Positions", "SP", "List of stop positions", GH_ParamAccess.list) |> ignore
    mgr.AddNumberParameter("Fac", "F", "0.0 left side, 1.0 right side", GH_ParamAccess.item, 0.5) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Blend of Color1 and Color2", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Alpha", "A", "0.0 left side, 1.0 right side", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("dc8abb5a-5a92-4148-8118-b397929d7bb3")

  override u.Icon = Icons.Blend

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Interpolation", u.Interpolation.toString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Interpolation") then
      u.Interpolation <-
        let d = InterpolationTypes.fromString (reader.GetString "Interpolation")
        match d with Option.None -> Linear | _ -> d.Value

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.Interpolation.toStringR
    DA.SetData(0, Utils.createColor (128, 128, 128)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let append_menu (it:InterpolationTypes) =
      GH_DocumentObject.Menu_AppendItem(menu, it.toStringR, (fun _ _ -> u.Interpolation <- it; u.ExpireSolution true), true, u.Interpolation= it) |> ignore
    append_menu Linear
    append_menu Ease
    append_menu Constant

  interface ICyclesNode with
    member u.NodeName = "color_ramp"

    member u.GetXml node nickname inputs =
      let i = inputs.Where(fun x -> x.Name = "Fac").ToList()
      let x = Utils.GetInputsXml i
      let t = String.Format(" interpolation=\"{0}\" ", u.Interpolation.toStringR)
      let stops =
        match inputs.[0].VolatileDataCount = inputs.[1].VolatileDataCount with
        | false -> ""
        | true ->
          let cl = seq[for i in inputs.[0].VolatileData.Branch(0) do
                        let c = Utils.castAs<GH_Colour>(i)
                        yield Utils.ColorXml(c.Value)]
          let sl = seq[for i in inputs.[1].VolatileData.Branch(0) do
                        let nr = Utils.castAs<GH_Number>(i)
                        yield nr.Value]
          Seq.zip cl sl |> Seq.map (fun x -> String.Format("\t<stop color=\"{0}\" position=\"{1}\" />", fst x, snd x)) |> String.concat "\n"
      "<" + (Utils.GetNodeXml node nickname (x+t)) + ">\n" + stops + "\n</color_ramp>"

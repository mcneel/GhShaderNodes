namespace ColorNodes

open FsShaderGraphComponents

open System
open System.Windows.Forms

open Grasshopper.Kernel

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
  inherit CyclesNode(
    "Mix", "mix",
    "Mix two color nodes, using specified blend type",
    "Shader", "Color",
    typeof<ccl.ShaderNodes.MixNode>)

  member val Blend = Mix with get, set

  override u.ComponentGuid = u |> ignore; new Guid("c3a397a6-f760-4ea1-9700-5722eee58489")

  override u.Icon = u |> ignore; Icons.Blend

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendMenu (bt:BlendTypes) =
      GH_DocumentObject.Menu_AppendItem(menu, bt.toStringR,
        (fun _ _ -> u.Blend <- bt; u.ExpireSolution true),
        true, u.Blend = bt) |> ignore
    appendMenu Mix
    appendMenu Add
    appendMenu Multiply
    appendMenu Screen
    appendMenu Overlay
    appendMenu Subtract
    appendMenu Divide
    appendMenu Difference
    appendMenu Darken
    appendMenu Lighten
    appendMenu Dodge
    appendMenu Burn
    appendMenu Hue
    appendMenu Saturation
    appendMenu Value
    appendMenu Color
    appendMenu Soft_Light
    appendMenu Linear_Light

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Blend", u.Blend.toString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Blend") then
      u.Blend <-
        let d = BlendTypes.fromString (reader.GetString "Blend")
        match d with | Option.None -> Mix | _ -> d.Value
    base.Read(reader)


type InterpolationTypes =
  | Constant
  | Linear
  | Ease
  member u.toString = Utils.toString u
  member u.toStringR = (u.toString)
  static member fromString s = Utils.fromString<InterpolationTypes> s

type ColorRampNode() =
  inherit CyclesNode(
    "Color Ramp", "color ramp",
    "Convert a float to a color according a gradient specification (RGB only)",
    "Shader", "Color", typeof<ccl.ShaderNodes.ColorRampNode>)

  member val Interpolation = Linear with get, set

  override u.ComponentGuid = u |> ignore; new Guid("dc8abb5a-5a92-4148-8118-b397929d7bb3")

  override u.Icon = u |> ignore; Icons.Blend

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Interpolation", u.Interpolation.toString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Interpolation") then
      u.Interpolation <-
        let d = InterpolationTypes.fromString (reader.GetString "Interpolation")
        match d with Option.None -> Linear | _ -> d.Value

    base.Read(reader)

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendMenu (it:InterpolationTypes) =
      GH_DocumentObject.Menu_AppendItem(
        menu, it.toStringR,
        (fun _ _ -> u.Interpolation <- it; u.ExpireSolution true),
        true, u.Interpolation= it) |> ignore
    appendMenu Linear
    appendMenu Ease
    appendMenu Constant

  (*interface ICyclesNode with
    member u.NodeName = xmlNodeName + "-" + u.InstanceGuid.ToString()

    member u.GetXml node nickname inputs iteration =
      let i = inputs.Where(fun x -> x.Name = "Fac").ToList()
      let x = Utils.GetInputsXml (i, iteration)
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
          Seq.zip cl sl
          |> Seq.map (fun x -> String.Format("\t<stop color=\"{0}\" position=\"{1}\" />", fst x, snd x))
          |> String.concat "\n"
      "<" + (Utils.GetNodeXml node nickname (x+t)) + ">\n" + stops + "\n</color_ramp>"*)

type ColorToLuminanceNode() =
  inherit CyclesNode(
    "Color2Luminance",
    "color -> luminance",
    "Convert input color to luminance value", "Shader", "Color", typeof<ccl.ShaderNodes.RgbToLuminanceNode>)

  override u.ComponentGuid = u |> ignore; new Guid("677f0004-2f9a-48da-897e-1deae4552b4f")
  override u.Icon = u |> ignore; Icons.Blend

  override u.SolveInstance(DA: IGH_DataAccess) =
    u |> ignore
    DA.SetData(0, 0.5) |> ignore

type ColorToLuminanceCNode() =
  inherit CyclesNode(
    "Color2LuminanceC",
    "color -> luminance C",
    "Convert input color to luminance (color)", "Shader", "Color", typeof<ccl.ShaderNodes.RgbToLuminanceNode>)

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    u |> ignore
    mgr.AddColourParameter("Val", "V", "Luminance Color", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = u |> ignore; new Guid("8e29a696-bf5e-4604-bfec-d0e504ee541d")

  override u.Icon = u |> ignore; Icons.Blend

  override u.SolveInstance(DA: IGH_DataAccess) =
    u |> ignore
    DA.SetData(0, Utils.createColor(128, 128, 128)) |> ignore

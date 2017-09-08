namespace ColorNodes

open FsShaderGraphComponents

open System
open System.Windows.Forms

open Grasshopper.Kernel
open Grasshopper.Kernel.Types

open ShaderGraphResources

type MixRgbNode() =
  inherit CyclesNode(
    "Mix", "mix",
    "Mix two color nodes, using specified blend type",
    "Shader", "Color",
    typeof<ccl.ShaderNodes.MixNode>)

  member val Blend =ccl.ShaderNodes.MixNode.BlendTypes.Mix with get, set

  override u.ComponentGuid = u |> ignore; new Guid("c3a397a6-f760-4ea1-9700-5722eee58489")

  override u.Icon = u |> ignore; Icons.Blend

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendMenu bt =
      GH_DocumentObject.Menu_AppendItem(menu, bt.ToString().Replace("_", " "),
        (fun _ _ ->
          u.Blend <- bt
          let mn = u.ShaderNode :?> ccl.ShaderNodes.MixNode
          mn.BlendType <- bt
          u.ExpireSolution true),
        true, u.Blend = bt) |> ignore
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Mix
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Add
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Multiply
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Screen
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Overlay
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Subtract
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Divide
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Difference
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Darken
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Lighten
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Dodge
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Burn
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Hue
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Saturation
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Value
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Color
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Soft_Light
    appendMenu ccl.ShaderNodes.MixNode.BlendTypes.Linear_Light

  override u.SolveInstance(DA: IGH_DataAccess) =
    base.SolveInstance DA
    u.Message <- u.Blend.ToString().Replace("_", " ")

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Blend", u.Blend.ToString()) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Blend") then
      u.Blend <-
        let (d, bt) = Enum.TryParse(reader.GetString "Blend")
        match d with | false -> ccl.ShaderNodes.MixNode.BlendTypes.Mix | _ -> bt
      let mn = u.ShaderNode :?> ccl.ShaderNodes.MixNode
      mn.BlendType <- u.Blend
    base.Read(reader)

type ColorRampNode() =
  inherit CyclesNode(
    "Color Ramp", "color ramp",
    "Convert a float to a color according a gradient specification (RGB only)",
    "Shader", "Color", typeof<ccl.ShaderNodes.ColorRampNode>)

  let mutable cpidx = 1
  let mutable spidx = 2

  member val Interpolation = ccl.ShaderNodes.ColorBand.Interpolations.Ease with get, set

  override u.ComponentGuid = u |> ignore; new Guid("dc8abb5a-5a92-4148-8118-b397929d7bb3")

  override u.Icon = u |> ignore; Icons.Blend

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    u |> ignore
    base.RegisterInputParams mgr
    cpidx <- mgr.AddColourParameter("Stop Colours", "SC", "List of colours", GH_ParamAccess.list)
    spidx <- mgr.AddNumberParameter("Stop Positions", "SP", "List of stop positions", GH_ParamAccess.list)

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Interpolation", u.Interpolation.ToString()) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Interpolation") then
      u.Interpolation <-
        let (d,i) = Enum.TryParse<ccl.ShaderNodes.ColorBand.Interpolations>(reader.GetString "Interpolation")
        match d with false -> ccl.ShaderNodes.ColorBand.Interpolations.Linear | _ -> i

    base.Read(reader)

  override u.SolveInstance(DA:IGH_DataAccess) =
    base.SolveInstance DA
    let crn = u.ShaderNode :?> ccl.ShaderNodes.ColorRampNode
    crn.ColorBand.Interpolation <- u.Interpolation
    crn.ins.Fac.Value <- Utils.readFloat32(u, DA, 0, "Couldn't read Fac")
    crn.ColorBand.Stops.Clear()
    match u.Params.Input.[cpidx].VolatileDataCount = u.Params.Input.[spidx].VolatileDataCount with
    | false -> ()
    | true ->
      let cl = seq[for i in u.Params.Input.[cpidx].VolatileData.Branch(0) do
                    yield i]
      let sl = seq[for i in u.Params.Input.[spidx].VolatileData.Branch(0) do
                    yield i]
      let addColorStop oc os =
        let c = Utils.castAs<GH_Colour>(oc)
        let s = Utils.castAs<GH_Number>(os)
        let ncs = new ccl.ShaderNodes.ColorStop()
        ncs.Color <- Utils.float4FromColor(c.Value)
        ncs.Position <- (float32)s.Value
        crn.ColorBand.Stops.Add(ncs)
      Seq.map2 addColorStop cl sl |> Seq.iter ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendMenu it =
      GH_DocumentObject.Menu_AppendItem(
        menu, it.ToString().Replace("_", " "),
        (fun _ _ -> u.Interpolation <- it; u.ExpireSolution true),
        true, u.Interpolation= it) |> ignore
    appendMenu ccl.ShaderNodes.ColorBand.Interpolations.Linear
    appendMenu ccl.ShaderNodes.ColorBand.Interpolations.Ease
    appendMenu ccl.ShaderNodes.ColorBand.Interpolations.Constant

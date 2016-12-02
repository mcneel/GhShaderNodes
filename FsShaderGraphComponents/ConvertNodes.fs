namespace ColorNodes

open FsShaderGraphComponents

open System
open System.Windows.Forms

open Grasshopper.Kernel

open ShaderGraphResources

type CombineXyzNode() =
  inherit CyclesNode(
    "Combine XYZ", "combine xyz",
    "Combine three float values into a vector",
    "Shader", "Converter",
    typeof<ccl.ShaderNodes.CombineXyzNode>)
  override u.ComponentGuid = u |> ignore; new Guid("a2db4f23-2152-4584-b584-bdb3c16b0f33")

type CombineRgbNode() =
  inherit CyclesNode(
    "Combine RGB", "combine RGB",
    "Combine three float values into an RGB color",
    "Shader", "Converter",
    typeof<ccl.ShaderNodes.CombineRgbNode>)
  override u.ComponentGuid = u |> ignore; new Guid("6bc90415-6f69-47f8-8e2e-b89f6390c273")

type CombineHsvNode() =
  inherit CyclesNode(
    "Combine HSV", "combine HSV",
    "Combine three float values into an HSV color",
    "Shader", "Converter",
    typeof<ccl.ShaderNodes.CombineHsvNode>)
  override u.ComponentGuid = u |> ignore; new Guid("e229219e-9a14-48c4-a36d-75152b81471a")

type ColorToLuminanceNode() =
  inherit CyclesNode(
    "Color2Luminance",
    "color -> luminance",
    "Convert input color to luminance value", "Shader", "Converter", typeof<ccl.ShaderNodes.RgbToLuminanceNode>)

  override u.ComponentGuid = u |> ignore; new Guid("677f0004-2f9a-48da-897e-1deae4552b4f")
  override u.Icon = u |> ignore; Icons.Blend

  override u.SolveInstance(DA: IGH_DataAccess) =
    u |> ignore
    DA.SetData(0, 0.5) |> ignore

type ColorToLuminanceCNode() =
  inherit CyclesNode(
    "Color2LuminanceC",
    "color -> luminance C",
    "Convert input color to luminance (color)", "Shader", "Converter", typeof<ccl.ShaderNodes.RgbToLuminanceNode>)

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    u |> ignore
    mgr.AddColourParameter("Val", "V", "Luminance Color", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = u |> ignore; new Guid("8e29a696-bf5e-4604-bfec-d0e504ee541d")

  override u.Icon = u |> ignore; Icons.Blend

  override u.SolveInstance(DA: IGH_DataAccess) =
    u |> ignore
    DA.SetData(0, Utils.createColor(128, 128, 128)) |> ignore

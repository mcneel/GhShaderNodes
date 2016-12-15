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

type ColorToBwNode() =
  inherit CyclesNode(
    "Color2Bw",
    "color -> bw",
    "Convert input color to gray scale value", "Shader", "Converter", typeof<ccl.ShaderNodes.RgbToBwNode>)

  override u.ComponentGuid = u |> ignore; new Guid("fb96081c-3353-4215-b182-ec548d96d0fb")
  override u.Icon = u |> ignore; Icons.Blend

type SeparateRgbNode() =
  inherit CyclesNode(
    "Separate RGB",
    "Separate RGB",
    "Separate color into its constituent RGB values", "Shader", "Converter", typeof<ccl.ShaderNodes.SeparateRgbNode>)

  override u.ComponentGuid = u |> ignore; new Guid("ac4dadca-1474-4261-9cfb-927a47bd1a4e")
  override u.Icon = u |> ignore; Icons.Blend

type SeparateXyzNode() =
  inherit CyclesNode(
    "Separate XYZ",
    "Separate XYZ",
    "Separate color into its constituent XYZ values", "Shader", "Converter", typeof<ccl.ShaderNodes.SeparateXyzNode>)

  override u.ComponentGuid = u |> ignore; new Guid("0b3116da-5dce-47a9-a457-5ac6891b5322")
  override u.Icon = u |> ignore; Icons.Blend

type SeparateHsvNode() =
  inherit CyclesNode(
    "Separate HSV",
    "Separate HSV",
    "Separate color into its constituent HSV values", "Shader", "Converter", typeof<ccl.ShaderNodes.SeparateHsvNode>)

  override u.ComponentGuid = u |> ignore; new Guid("b3eafc03-89d4-4797-bf0d-132ec2d43567")
  override u.Icon = u |> ignore; Icons.Blend

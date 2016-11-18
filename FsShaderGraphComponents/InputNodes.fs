namespace InputNodes

open System

open FsShaderGraphComponents

open ShaderGraphResources

type TextureCoordinate() =
  inherit CyclesNode(
    "Texture Coordinate", "texcoord",
    "Texture Coordinate for point being sampled",
    "Shader", "Input",
    typeof<ccl.ShaderNodes.TextureCoordinateNode>)
  override u.ComponentGuid = u |> ignore; new Guid("d78aa03c-713b-43b8-a478-7edfe75cf148")
  override u.Icon = u |> ignore; Icons.TC

type Geometry() =
  inherit CyclesNode(
    "Geometry", "geom",
    "Geometry info for the point being sampled",
    "Shader", "Input",
    typeof<ccl.ShaderNodes.GeometryInfoNode>)
  override u.ComponentGuid = u |> ignore; new Guid("1268d35e-8912-45c1-9642-0b29ec4f1ff9")
  override u.Icon = u |> ignore; Icons.TC

type LayerWeightNode() =
  inherit CyclesNode(
   "Layer Weight", "layer weight",
   "Layer weight",
   "Shader", "Input",
   typeof<ccl.ShaderNodes.LayerWeightNode>)
  override u.ComponentGuid = u |> ignore; new Guid("5576ff9f-99f7-4611-aa42-dcc4b6c621ac")
  override u.Icon = u |> ignore; Icons.Emission

type LightPathNode() =
  inherit CyclesNode(
    "Light Path", "light path",
    "Light Path",
    "Shader", "Input",
    typeof<ccl.ShaderNodes.LightPathNode>)
  override u.ComponentGuid = u |> ignore; new Guid("9ba94ea6-d977-47ba-807c-b4b68fa9dea8")
  override u.Icon = u |> ignore; Icons.Emission

type LightFalloffNode() =
  inherit CyclesNode(
    "Light Falloff", "light falloff",
    "Light Falloff",
    "Shader", "Input",
    typeof<ccl.ShaderNodes.LightFalloffNode>)
  override u.ComponentGuid = u |> ignore; new Guid("5232b3d2-80ac-44f2-bc9e-b6c066d2c6ac")
  override u.Icon = u |> ignore; Icons.Emission

type FresnelNode() =
  inherit CyclesNode(
    "Fresnel", "fresnel",
    "Fresnel",
    "Shader", "Input",
    typeof<ccl.ShaderNodes.FresnelNode>)
  override u.ComponentGuid = u |> ignore; new Guid("b9cca29d-2c77-42cd-a99d-70eb880c02ac")
  override u.Icon = u |> ignore; Icons.Emission
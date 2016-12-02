namespace TextureNodes

open FsShaderGraphComponents

open System
open System.Windows.Forms

open Grasshopper.Kernel

open ShaderGraphResources

type BumpNode() =
  inherit CyclesNode(
    "Bump", "bump",
    "Bump node",
    "Shader", "Vector",
    typeof<ccl.ShaderNodes.BumpNode>)
  override u.ComponentGuid = u |> ignore; new Guid("02f39fc2-757a-4ec0-91f6-1822ca48663d")

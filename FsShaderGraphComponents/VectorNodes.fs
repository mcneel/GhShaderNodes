namespace TextureNodes

open FsShaderGraphComponents

open System
open System.Windows.Forms

open ccl.ShaderNodes

open Grasshopper.Kernel

open ShaderGraphResources

type BumpNode() =
  inherit CyclesNode(
    "Bump", "bump",
    "Bump node",
    "Shader", "Vector",
    typeof<ccl.ShaderNodes.BumpNode>)
  override u.ComponentGuid = u |> ignore; new Guid("02f39fc2-757a-4ec0-91f6-1822ca48663d")

[<AbstractClass>]
type VectorMathBaseNode(openum, category, subcategory, outputdescription, op) =
  inherit CyclesNode(
    openum.ToString().Replace('_', ' '),
    openum.ToString().Replace('_', ' '),
    openum.ToString().Replace('_', ' '),
    category, subcategory,
    typeof<VectorMathNode>)

  let opEnum = openum
  let mutable mUseClamp = false

  member u.OutputDesc = u |> ignore; outputdescription
  member u.Op = u |> ignore; op

  member u.UseClamp
    with get() = u |> ignore; mUseClamp
    and set(value) = mUseClamp <- value

  override u.SolveInstance(DA : IGH_DataAccess) =
    let mn = u.ShaderNode :?> VectorMathNode
    mn.Operation <- opEnum
    base.SolveInstance(DA)
    let v1 = Utils.readVector(u, DA, 0, "Couldn't read Value1")
    let v2 = Utils.readVector(u, DA, 1, "Couldn't read Value2")

    let r = u.Op v1 v2

    DA.SetData(0, r) |> ignore
    DA.SetData(1, r) |> ignore

type VectorMathAdd() =
  inherit VectorMathBaseNode(VectorMathNode.Operations.Add, "Shader", "Vector", "Vector1+Vector2", (+))
  override u.ComponentGuid = u |> ignore; new Guid("297632e9-77ec-47b3-9726-78f855b17796")
  override u.Icon = u |> ignore; Icons.Add

type VectorMathSubtract() =
  inherit VectorMathBaseNode(VectorMathNode.Operations.Subtract, "Shader", "Vector", "Vector1-Vector2", (+))
  override u.ComponentGuid = u |> ignore; new Guid("473800e8-a584-4238-a9c4-d5474481a424")
  override u.Icon = u |> ignore; Icons.Sub

type VectorMathAverage() =
  inherit VectorMathBaseNode(VectorMathNode.Operations.Average, "Shader", "Vector", "NormLen(Vector1+Vector2)", (+))
  override u.ComponentGuid = u |> ignore; new Guid("000d564f-85c7-409f-b4fc-ae0c1152a19e")
  override u.Icon = u |> ignore; Icons.Add

type VectorMathDot() =
  inherit VectorMathBaseNode(VectorMathNode.Operations.Dot_Product, "Shader", "Vector", "Fac = Dot(Vector1,Vector2)", (+))
  override u.ComponentGuid = u |> ignore; new Guid("f63a2c72-5f84-4e58-a80c-73a5fb72e145")
  override u.Icon = u |> ignore; Icons.Mult

type VectorMathCross() =
  inherit VectorMathBaseNode(VectorMathNode.Operations.Cross_Product, "Shader", "Vector", "NormLen(Cross(Vector1,Vector2))", (+))
  override u.ComponentGuid = u |> ignore; new Guid("35c5f912-4d8b-4a37-ad5b-1e0384a45d7e")
  override u.Icon = u |> ignore; Icons.Mult

type VectorMathNormalize() =
  inherit VectorMathBaseNode(VectorMathNode.Operations.Normalize, "Shader", "Vector", "NormLen(Vector1)", (+))
  override u.ComponentGuid = u |> ignore; new Guid("b0972558-a3d6-441f-8b8a-4f557872ad9f")
  override u.Icon = u |> ignore; Icons.Add

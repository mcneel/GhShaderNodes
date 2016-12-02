namespace MathNodes

open FsShaderGraphComponents

open System
open System.Windows.Forms

open ccl.ShaderNodes

open Grasshopper.Kernel

open ShaderGraphResources


[<AbstractClass>]
type MathBaseNode(openum, category, subcategory, outputdescription, op) =
  inherit CyclesNode(
    openum.ToString().Replace('_', ' '),
    openum.ToString().Replace('_', ' '),
    openum.ToString().Replace('_', ' '),
    category, subcategory,
    typeof<MathNode>)

  let opEnum = openum
  let mutable mUseClamp = false

  member u.OutputDesc = u |> ignore; outputdescription
  member u.Op = u |> ignore; op

  member u.UseClamp
    with get() = u |> ignore; mUseClamp
    and set(value) = mUseClamp <- value

  override u.SolveInstance(DA : IGH_DataAccess) =
    let mn = u.ShaderNode :?> MathNode
    mn.Operation <- opEnum
    mn.UseClamp <- u.UseClamp
    base.SolveInstance(DA)
    let v1 = Utils.readFloat(u, DA, 0, "Couldn't read Value1")
    let v2 = Utils.readFloat(u, DA, 1, "Couldn't read Value2")

    let r =
      match u.UseClamp with
      | true -> u.Message <- "UseClamp = true"; max (min (u.Op v1 v2) 1.0) 0.0
      | _ -> u.Message <- "UseClamp = false"; u.Op v1 v2

    DA.SetData(0, r) |> ignore

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetBoolean("UseClamp", u.UseClamp)
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("UseClamp") then
      u.UseClamp <- reader.GetBoolean("UseClamp")

    base.Read(reader)

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let handler _ _ = u.UseClamp <- not u.UseClamp; u.ExpireSolution true
    GH_DocumentObject.Menu_AppendItem(menu, "Use Clamp", handler, true, u.UseClamp) |> ignore

  (*interface ICyclesNode with
    member u.NodeName = "math"
    member u.GetXml node nickname inputs iteration =
      let x = Utils.GetInputsXml (inputs, iteration)
      "<" + Utils.GetNodeXml node nickname (x + String.Format(" type=\"{0}\"", u.Name)) + " />"*)

type MathAdd() =
  inherit MathBaseNode(MathNode.Operations.Add, "Shader", "Math", "Value1+Value2", (+))
  override u.ComponentGuid = u |> ignore; new Guid("ec3b4eb3-7cd5-43c8-8ef7-deb2200df882")
  override u.Icon = u |> ignore; Icons.Add

type MathSubtract() =
  inherit MathBaseNode(MathNode.Operations.Subtract, "Shader", "Math", "Value1-Value2", (-))
  override u.ComponentGuid = u |> ignore; new Guid("c2b99ede-3050-483d-ab90-35a1548d2d22")
  override u.Icon = u |> ignore; Icons.Sub

type MathMultiply() =
  inherit MathBaseNode(MathNode.Operations.Multiply, "Shader", "Math", "Value1×Value2", (*))
  override u.ComponentGuid = u |> ignore; new Guid("4a360292-b84b-4808-ad8e-67f2b77b0e15")
  override u.Icon = u |> ignore; Icons.Mult

type MathDivide() =
  inherit MathBaseNode(MathNode.Operations.Divide, "Shader", "Math", "Value1/Value2", (/))
  override u.ComponentGuid = u |> ignore; new Guid("623ee461-9576-4981-a85a-7aa4a30e2e98")
  override u.Icon = u |> ignore; Icons.Div

type MathPower() =
  inherit MathBaseNode(MathNode.Operations.Power, "Shader", "Math", "Value1**Value2", ( ** ))
  override u.ComponentGuid = u |> ignore; new Guid("2e74876b-33f9-4262-9791-cf53466a63e3")
  override u.Icon = u |> ignore; Icons.Pow

type MathLogarithm() =
  inherit MathBaseNode(MathNode.Operations.Logarithm, "Shader", "Math", "log(Value1)/log(Value2)", Utils.Logarithm)
  override u.ComponentGuid = u |> ignore; new Guid("72b96bf7-350e-4408-bdec-d61ea6b4d677")
  override u.Icon = u |> ignore; Icons.Log

type MathMinimum() =
  inherit MathBaseNode(MathNode.Operations.Minimum, "Shader", "Math", "min(Value1,Value2)", min)
  override u.ComponentGuid = u |> ignore; new Guid("825f2014-0e51-4d60-bb2f-8aa40aea7b91")
  override u.Icon = u |> ignore; Icons.Min

type MathMaximum() =
  inherit MathBaseNode(MathNode.Operations.Maximum, "Shader", "Math", "max(Value1,Value2)", max)
  override u.ComponentGuid = u |> ignore; new Guid("1706489e-c7a3-453d-971a-1429a51aa783")
  override u.Icon = u |> ignore; Icons.Max

type MathLessThan() =
  inherit MathBaseNode(MathNode.Operations.Less_Than, "Shader", "Math", "Value1 < Value2", Utils.LessThen)
  override u.ComponentGuid = u |> ignore; new Guid("94d49a6f-2520-430d-b2ee-6eda864b568b")
  override u.Icon = u |> ignore; Icons.LessThan

type MathGreaterThan() =
  inherit MathBaseNode(MathNode.Operations.Greater_Than, "Shader", "Math", "Value1 > Value2", Utils.GreaterThan)
  override u.ComponentGuid = u |> ignore; new Guid("7a833ba4-28f2-4676-8fa0-c746a9dd6b02")
  override u.Icon = u |> ignore; Icons.GreaterThan

type MathSine() =
  inherit MathBaseNode(MathNode.Operations.Sine, "Shader", "Math", "Sine(Value1)", Utils.Sine)
  override u.ComponentGuid = u |> ignore; new Guid("63468f25-b71e-4285-91d0-e0505118f738")
  override u.Icon = u |> ignore; Icons.Add

type MathCosine() =
  inherit MathBaseNode(MathNode.Operations.Cosine, "Shader", "Math", "Cosine(Value1)", Utils.Cosine)
  override u.ComponentGuid = u |> ignore; new Guid("a8db0fa4-0bd3-405f-a8ae-1416dd4de632")
  override u.Icon = u |> ignore; Icons.Add

type MathTangent() =
  inherit MathBaseNode(MathNode.Operations.Tangent, "Shader", "Math", "Tangent(Value1)", Utils.Tangent)
  override u.ComponentGuid = u |> ignore; new Guid("e3e72aad-acf4-4965-a5d7-c1c6c3701faf")
  override u.Icon = u |> ignore; Icons.Add

type MathArcsine() =
  inherit MathBaseNode(MathNode.Operations.Arcsine, "Shader", "Math", "Arcsine(Value1)", Utils.Arcsine)
  override u.ComponentGuid = u |> ignore; new Guid("475a3c6c-b047-461e-ba07-ce96c32a5d94")
  override u.Icon = u |> ignore; Icons.Add

type MathArccosine() =
  inherit MathBaseNode(MathNode.Operations.Arccosine, "Shader", "Math", "Arccosine(Value1)", Utils.Arccosine)
  override u.ComponentGuid = u |> ignore; new Guid("07440de9-5786-4265-a328-1acb45188157")
  override u.Icon = u |> ignore; Icons.Add

type MathArctangent() =
  inherit MathBaseNode(MathNode.Operations.Arctangent, "Shader", "Math", "Arctangent(Value1)", Utils.Arctangent)
  override u.ComponentGuid = u |> ignore; new Guid("a01cd7ab-a8aa-427a-bcaf-99e2410f620c")
  override u.Icon = u |> ignore; Icons.Add

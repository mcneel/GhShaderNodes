namespace MathNodes

open FsShaderGraphComponents

open System
open System.Windows.Forms

open Grasshopper.Kernel

open ShaderGraphResources


[<AbstractClass>]
type MathBaseNode(name, nickname, description, category, subcategory, outputdescription, op) =
  inherit GH_Component(name, nickname, description, category, subcategory)

  let mutable m_useclamp = false

  member u.OutputDesc = outputdescription
  member u.Op = op

  member u.UseClamp
    with get() = m_useclamp
    and set(value) = m_useclamp <- value

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddNumberParameter("Value1", "1", "First value", GH_ParamAccess.item, 0.0) |> ignore
    mgr.AddNumberParameter("Value2", "2", "Second value", GH_ParamAccess.item, 0.0) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddNumberParameter("Value", "R", u.OutputDesc, GH_ParamAccess.item) |> ignore

  override u.SolveInstance(DA : IGH_DataAccess) =
    let v1 = Utils.readFloat(u, DA, 0, "Couldn't read Value1")
    let v2 = Utils.readFloat(u, DA, 1, "Couldn't read Value2")

    let r = match u.UseClamp with true -> u.Message <- "UseClamp = true"; max (min (u.Op v1 v2) 1.0) 0.0 | _ -> u.Message <- "UseClamp = false"; u.Op v1 v2

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

  interface ICyclesNode with
    member u.NodeName = "math"
    member u.GetXml node nickname inputs iteration =
      let x = Utils.GetInputsXml (inputs, iteration)
      "<" + Utils.GetNodeXml node nickname (x + String.Format(" type=\"{0}\"", u.Name)) + " />"

type MathAdd() =
  inherit MathBaseNode("Add", "add", "Add two floats", "Shader", "Math", "Value1+Value2", (+))
  override u.ComponentGuid = new Guid("ec3b4eb3-7cd5-43c8-8ef7-deb2200df882")
  override u.Icon = Icons.Add

type MathSubtract() =
  inherit MathBaseNode("Subtract", "subtract", "Subtract two floats", "Shader", "Math", "Value1-Value2", (-))
  override u.ComponentGuid = new Guid("c2b99ede-3050-483d-ab90-35a1548d2d22")
  override u.Icon = Icons.Sub

type MathMultiply() =
  inherit MathBaseNode("Multiply", "multiply", "Multiply two floats", "Shader", "Math", "Value1×Value2", (*))
  override u.ComponentGuid = new Guid("4a360292-b84b-4808-ad8e-67f2b77b0e15")
  override u.Icon = Icons.Mult

type MathDivide() =
  inherit MathBaseNode("Divide", "divide", "Divide two floats", "Shader", "Math", "Value1/Value2", (/))
  override u.ComponentGuid = new Guid("623ee461-9576-4981-a85a-7aa4a30e2e98")
  override u.Icon = Icons.Div

type MathPower() =
  inherit MathBaseNode("Power", "power", "Power two floats", "Shader", "Math", "Value1**Value2", ( ** ) )
  override u.ComponentGuid = new Guid("2e74876b-33f9-4262-9791-cf53466a63e3")
  override u.Icon = Icons.Pow

type MathLogarithm() =
  inherit MathBaseNode("Logarithm", "logarithm", "Logarithm two floats", "Shader", "Math", "log(Value1)/log(Value2)", Utils.Logarithm)
  override u.ComponentGuid = new Guid("72b96bf7-350e-4408-bdec-d61ea6b4d677")
  override u.Icon = Icons.Log

type MathMinimum() =
  inherit MathBaseNode("Minimum", "min", "Minimum two floats", "Shader", "Math", "min(Value1,Value2)", min)
  override u.ComponentGuid = new Guid("825f2014-0e51-4d60-bb2f-8aa40aea7b91")
  override u.Icon = Icons.Min

type MathMaximum() =
  inherit MathBaseNode("Maximum", "max", "Maximum two floats", "Shader", "Math", "max(Value1,Value2)", max)
  override u.ComponentGuid = new Guid("1706489e-c7a3-453d-971a-1429a51aa783")
  override u.Icon = Icons.Max

type MathLessThan() =
  inherit MathBaseNode("Less Than", "less", "v1 < v2", "Shader", "Math", "Value1 < Value2", Utils.LessThen)
  override u.ComponentGuid = new Guid("94d49a6f-2520-430d-b2ee-6eda864b568b")
  override u.Icon = Icons.LessThan

type MathGreaterThan() =
  inherit MathBaseNode("Greater Than", "greater", "Value1 > Value2", "Shader", "Math", "Value1 > Value2", Utils.GreaterThan)
  override u.ComponentGuid = new Guid("7a833ba4-28f2-4676-8fa0-c746a9dd6b02")
  override u.Icon = Icons.GreaterThan

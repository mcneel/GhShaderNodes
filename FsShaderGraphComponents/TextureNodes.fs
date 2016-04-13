namespace TextureNodes

open FsShaderGraphComponents

open System
open System.Drawing
open System.Windows.Forms

open Grasshopper.Kernel
open Grasshopper.Kernel.Types

open Rhino.Geometry

open ShaderGraphResources

type NoiseTextureNode() =
  inherit GH_Component("Noise", "noise", "Noise", "Shader", "Texture")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.Zero) |> ignore
    mgr.AddNumberParameter("Scale", "S", "Scale", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Detail", "D", "Detail", GH_ParamAccess.item, 2.0) |> ignore
    mgr.AddNumberParameter("Distortion", "Dist", "Distortion", GH_ParamAccess.item, 0.0) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Fac", "F", "Fac", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("c3632808-8f29-48bd-afc2-0b85ad5763c0")

  override u.Icon = Icons.Emission

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- ""

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  interface ICyclesNode with
    member u.NodeName = "noise_texture"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      "<" + Utils.GetNodeXml node nickname x + " />"

type GradientTypes =
  | Linear
  | Quadratic
  | Easing
  | Diagonal
  | Radial
  | Quadratic_Sphere
  | Spherical
  member u.toString = Utils.toString u
  member u.toStringR = (u.toString)
  static member fromString s = Utils.fromString<GradientTypes> s

type GradientTextureNode() =
  inherit GH_Component("Gradient", "gradient", "Gradient", "Shader", "Texture")

  member val Gradient = Linear with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.Zero) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Fac", "F", "Fac", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("e9d63595-4a09-4351-93f4-acd2a0248a9b")

  override u.Icon = Icons.Emission

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Gradient", u.Gradient.toString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Gradient") then
      u.Gradient <-
        let d = GradientTypes.fromString (reader.GetString "Gradient")
        match d with None -> Linear | _ -> d.Value

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.Gradient.toStringR

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let append_menu (gt:GradientTypes) =
      GH_DocumentObject.Menu_AppendItem(menu, gt.toStringR, (fun _ _ -> u.Gradient <- gt; u.ExpireSolution true), true, u.Gradient = gt) |> ignore
    append_menu Linear
    append_menu Easing
    append_menu Quadratic
    append_menu Diagonal
    append_menu Radial
    append_menu Quadratic_Sphere
    append_menu Spherical

  interface ICyclesNode with
    member u.NodeName = "gradient_texture"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      let t = String.Format(" interpolation=\"{0}\" ", u.Gradient.toStringR)
      "<" + Utils.GetNodeXml node nickname (x+t) + " />"


namespace TextureNodes

open FsShaderGraphComponents

open System
open System.Drawing

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
      "<" + Utils.GetNodeXml node nickname x + "/>"


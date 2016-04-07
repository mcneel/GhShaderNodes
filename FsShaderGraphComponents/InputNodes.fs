namespace InputNodes

open System

open Grasshopper.Kernel

open Rhino.Geometry

open FsShaderGraphComponents

open ShaderGraphResources

type TextureCoordinate() =
  inherit GH_Component("Texture Coordinate", "texcoord", "Texture Coordinate for point being sampled", "Shader", "Input")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    ()

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddVectorParameter("Generated", "G", "Generated", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("Normal", "N", "Normal", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("UV", "UV", "UV", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("Object", "O", "Object", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("Camera", "C", "Camera", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("Window", "W", "Window", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("Reflection", "R", "Reflection", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("WcsBox", "WCS", "WcsBox", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvSpherical", "Sph", "EnvSpherical", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvEmap", "Emap", "EnvEmap", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvBox", "Ebox", "EnvBox", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvLightProbe", "LP", "EnvLightProbe", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvCubemap", "Cube", "EnvCubemap", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvCubemapVerticalCross", "CubeV", "EnvCubemapVerticalCross", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvCubemapHorizontalCross", "CubeH", "EnvCubemapHorizontalCross", GH_ParamAccess.item) |> ignore
    mgr.AddVectorParameter("EnvHemi", "Hemi", "EnvHemi", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("d78aa03c-713b-43b8-a478-7edfe75cf148")

  override u.Icon = Icons.TC

  override u.SolveInstance(DA: IGH_DataAccess) =
    DA.SetData(0, Vector3d.Zero) |> ignore
    DA.SetData(1, Vector3d.Zero) |> ignore
    DA.SetData(2, Vector3d.Zero) |> ignore
    DA.SetData(3, Vector3d.Zero) |> ignore
    DA.SetData(4, Vector3d.Zero) |> ignore
    DA.SetData(5, Vector3d.Zero) |> ignore
    DA.SetData(6, Vector3d.Zero) |> ignore
    DA.SetData(7, Vector3d.Zero) |> ignore
    DA.SetData(8, Vector3d.Zero) |> ignore
    DA.SetData(9, Vector3d.Zero) |> ignore
    DA.SetData(10, Vector3d.Zero) |> ignore
    DA.SetData(11, Vector3d.Zero) |> ignore
    DA.SetData(12, Vector3d.Zero) |> ignore
    DA.SetData(13, Vector3d.Zero) |> ignore
    DA.SetData(14, Vector3d.Zero) |> ignore
    DA.SetData(15, Vector3d.Zero) |> ignore

  interface ICyclesNode with
    member u.NodeName = "texture_coordinate"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      Utils.GetNodeXml node nickname x

type LayerWeightNode() =
  inherit GH_Component("Layer Weight", "layer weight", "Layer weight", "Shader", "Input")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddNumberParameter("Blend", "B", "Blend factor 0.0-1.0", GH_ParamAccess.item, 0.5) |> ignore
    mgr.AddVectorParameter("Normal", "N", "Roughness of emission bsdf", GH_ParamAccess.item, Vector3d.Zero) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddNumberParameter("Fresnel", "Fr", "Fresnel", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Facing", "Fa", "Fresnel", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("5576ff9f-99f7-4611-aa42-dcc4b6c621ac")

  override u.Icon = Icons.Emission

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- ""

    DA.SetData(0, 0.5) |> ignore
    DA.SetData(1, 0.5) |> ignore

  interface ICyclesNode with
    member u.NodeName = "layer_weight"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      Utils.GetNodeXml node nickname x

type LightPathNode() =
  inherit GH_Component("Light Path", "light path", "Light Path", "Shader", "Input")

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    ()

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddNumberParameter("IsCameraRay", "CR", "Is Camera Ray", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("IsShadowRay", "SR", "Is Shadow Ray", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("IsDiffuseRay", "DR", "Is Diffuse Ray", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("IsGlossyRay", "GR", "Is Glossy Ray", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("IsSingularRay", "SingR", "Is Singular Ray", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("IsReflectionRay", "RR", "Is Reflection Ray", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("IsTransmissionRay", "TR", "Is Transmission Ray", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("IsVolumeScatterRay", "VSR", "Is VolumeScatter Ray", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("RayLength", "RL", "Ray Length", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("RayDepth", "RD", "Ray Depth", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("TransparentDepth", "TD", "Transparent Depth", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("9ba94ea6-d977-47ba-807c-b4b68fa9dea8")

  override u.Icon = Icons.Emission

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- ""

    DA.SetData(0, 1.0) |> ignore
    DA.SetData(1, 1.0) |> ignore
    DA.SetData(2, 1.0) |> ignore
    DA.SetData(3, 1.0) |> ignore
    DA.SetData(4, 1.0) |> ignore
    DA.SetData(5, 1.0) |> ignore
    DA.SetData(6, 1.0) |> ignore
    DA.SetData(7, 1.0) |> ignore
    DA.SetData(8, 1.0) |> ignore
    DA.SetData(9, 1.0) |> ignore
    DA.SetData(10, 1.0) |> ignore

  interface ICyclesNode with
    member u.NodeName = "light_path"
    member u.GetXml node nickname inputs =
      let x = Utils.GetInputsXml inputs
      Utils.GetNodeXml node nickname x




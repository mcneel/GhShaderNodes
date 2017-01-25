namespace BsdfNodes

open System
open System.Windows.Forms

open Grasshopper.Kernel

open FsShaderGraphComponents

open ShaderGraphResources

type BlendNode() =
  inherit CyclesNode (
    "Blend", "blend",
    "Blend two BSDF nodes",
    "Shader", "Operation",
    typeof<ccl.ShaderNodes.MixClosureNode>)
  override u.ComponentGuid = u |> ignore; new Guid("133f2f95-926f-4ab4-bc8b-5f96e106d3e4")
  override u.Icon = u |> ignore; Icons.Blend

type BackgroundNode() =
  inherit CyclesNode(
    "Background", "background",
    "Background two BSDF nodes",
    "Shader", "Operation",
    typeof<ccl.ShaderNodes.BackgroundNode>)
  override u.ComponentGuid = u |> ignore; new Guid("dd68810b-0a0e-4c54-b08e-f46b41e79f32")
  override u.Icon = u |> ignore; Icons.Output

type AddClosureNode() =
  inherit CyclesNode(
    "Add", "add",
    "Add two BSDF nodes",
    "Shader", "Operation",
    typeof<ccl.ShaderNodes.AddClosureNode>)
  override u.ComponentGuid = u |> ignore; new Guid("f7929217-6fbb-4bd5-b74f-9763816dc38c")
  override u.Icon = u |> ignore; Icons.Add

type TransparentBsdf() =
  inherit CyclesNode(
    "Transparent BSDF", "transparent",
    "Transparent BSDF node for shader graph",
    "Shader", "BSDF",
    typeof<ccl.ShaderNodes.TransparentBsdfNode>)
  override u.ComponentGuid = u |> ignore; new Guid("15f77ebf-ae59-4c49-80b1-362a7168f85f")
  override u.Icon = u |> ignore; Icons.Diffuse

type DiffuseBsdf() =
  inherit CyclesNode(
    "Diffuse BSDF", "diffuse",
    "Diffuse BSDF node for shader graph",
    "Shader", "BSDF",
    typeof<ccl.ShaderNodes.DiffuseBsdfNode>)
  override u.ComponentGuid = u |> ignore; new Guid("e79bd4ac-1aa0-450d-aa4a-495cfeb8cb13")
  override u.Icon = u |> ignore; Icons.Diffuse

type VelvetBsdf() =
  inherit CyclesNode(
    "Velvet BSDF", "velvet",
    "Velvet BSDF node for shader graph",
    "Shader", "BSDF",
    typeof<ccl.ShaderNodes.VelvetBsdfNode>)
  override u.ComponentGuid = u |> ignore; new Guid("d85aeb1d-e42f-43b6-86d6-ddf9cae5a633")
  override u.Icon = u |> ignore; Icons.Diffuse

type AnisotropicBsdf() =
  inherit CyclesNode(
    "Anisotropic BSDF", "anisotropic",
    "Anisotropic BSDF node for shader graph",
    "Shader", "BSDF",
    typeof<ccl.ShaderNodes.AnisotropicBsdfNode>)
  member val Distribution = Multiscatter_GGX with get, set
  override u.ComponentGuid = u |> ignore; new Guid("bab1082a-4c74-4d07-9c63-d3f40a178c6a")
  override u.Icon = u |> ignore; Icons.Glossy
  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.Distribution.ToString
    base.SolveInstance(DA)
  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Distribution", u.Distribution.ToString) |> ignore
    base.Write(writer)
  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Distribution") then
      u.Distribution <-
        let d = Distribution.FromString (reader.GetString "Distribution")
        match d with | Option.None -> GGX | _ -> d.Value
    base.Read(reader)
  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendMenu (it:Distribution) =
      GH_DocumentObject.Menu_AppendItem(menu, it.ToStringR, (fun _ _ -> u.Distribution <- it; u.ExpireSolution true),
        true, u.Distribution = it) |> ignore
    appendMenu Beckmann
    appendMenu GGX
    appendMenu Multiscatter_GGX
    appendMenu Ashihkmin_Shirley

type RefractionBsdf() =
  inherit CyclesNode(
    "Refraction BSDF", "refraction",
    "Refraction BSDF node for shader graph",
    "Shader", "BSDF",
    typeof<ccl.ShaderNodes.RefractionBsdfNode>)
  member val Distribution = Sharp with get, set
  override u.ComponentGuid = u |> ignore; new Guid("e32dffe3-e31a-45a6-9d13-4fb0eefe4ff5")
  override u.Icon = u |> ignore; Icons.Glossy
  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.Distribution.ToString
    match u.Distribution with
    | Distribution.Sharp -> 
      (u.ShaderNode :?> ccl.ShaderNodes.RefractionBsdfNode).Distribution <- ccl.ShaderNodes.RefractionBsdfNode.RefractionDistribution.Sharp
    | Distribution.Beckmann -> 
      (u.ShaderNode :?> ccl.ShaderNodes.RefractionBsdfNode).Distribution <- ccl.ShaderNodes.RefractionBsdfNode.RefractionDistribution.Beckmann
    | _ -> 
      (u.ShaderNode :?> ccl.ShaderNodes.RefractionBsdfNode).Distribution <- ccl.ShaderNodes.RefractionBsdfNode.RefractionDistribution.GGX
    base.SolveInstance(DA)
  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Distribution", u.Distribution.ToString) |> ignore
    base.Write(writer)
  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Distribution") then
      u.Distribution <-
        let d = Distribution.FromString (reader.GetString "Distribution")
        match d with | Option.None -> Sharp | _ -> d.Value
    base.Read(reader)
  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendMenu (it:Distribution) =
      GH_DocumentObject.Menu_AppendItem(menu, it.ToStringR, (fun _ _ -> u.Distribution <- it; u.ExpireSolution true),
        true, u.Distribution = it) |> ignore
    appendMenu Sharp
    appendMenu Beckmann
    appendMenu GGX

type GlassBsdf() =
  inherit CyclesNode(
    "Glass BSDF", "glass",
    "Glass BSDF node for shader graph",
    "Shader", "BSDF",
    typeof<ccl.ShaderNodes.GlassBsdfNode>)
  member val Distribution = Multiscatter_GGX with get, set
  override u.ComponentGuid = u |> ignore; new Guid("4db00f7b-fa70-4130-813d-a9f7cd193795")
  override u.Icon = u |> ignore; Icons.Glossy
  override u.SolveInstance(DA: IGH_DataAccess) =
    match u.Distribution with
    | Distribution.Sharp -> 
      (u.ShaderNode :?> ccl.ShaderNodes.GlassBsdfNode).Distribution <- ccl.ShaderNodes.GlassBsdfNode.GlassDistribution.Sharp
    | Distribution.Beckmann -> 
      (u.ShaderNode :?> ccl.ShaderNodes.GlassBsdfNode).Distribution <- ccl.ShaderNodes.GlassBsdfNode.GlassDistribution.Beckmann
    | Distribution.GGX -> 
      (u.ShaderNode :?> ccl.ShaderNodes.GlassBsdfNode).Distribution <- ccl.ShaderNodes.GlassBsdfNode.GlassDistribution.GGX
    | _ ->
      (u.ShaderNode :?> ccl.ShaderNodes.GlassBsdfNode).Distribution <- ccl.ShaderNodes.GlassBsdfNode.GlassDistribution.Multiscatter_GGX
    u.Message <- u.Distribution.ToStringR
    base.SolveInstance(DA)
  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Distribution", u.Distribution.ToString) |> ignore
    base.Write(writer)
  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Distribution") then
      u.Distribution <-
        let d = Distribution.FromString (reader.GetString "Distribution")
        match d with | Option.None -> Sharp | _ -> d.Value
    base.Read(reader)
  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendMenu (it:Distribution) =
      GH_DocumentObject.Menu_AppendItem(menu, it.ToStringR, (fun _ _ -> u.Distribution <- it; u.ExpireSolution true),
        true, u.Distribution = it) |> ignore
    appendMenu Sharp
    appendMenu Beckmann
    appendMenu Multiscatter_GGX
    appendMenu GGX

type GlossyBsdf() =
  inherit CyclesNode(
    "Glossy BSDF", "glossy",
    "Glossy BSDF node for shader graph",
    "Shader", "BSDF",
    typeof<ccl.ShaderNodes.GlossyBsdfNode>)

  member val Distribution = Multiscatter_GGX with get, set
  override u.ComponentGuid = u |> ignore; new Guid("84e014b7-a76a-4b4f-8d37-25696cbebc04")
  override u.Icon = u |> ignore; Icons.Glossy
  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.Distribution.ToStringR
    match u.Distribution with
    | Distribution.Sharp -> 
      (u.ShaderNode :?> ccl.ShaderNodes.GlossyBsdfNode).Distribution <- ccl.ShaderNodes.GlossyBsdfNode.GlossyDistribution.Sharp
    | Distribution.Beckmann -> 
      (u.ShaderNode :?> ccl.ShaderNodes.GlossyBsdfNode).Distribution <- ccl.ShaderNodes.GlossyBsdfNode.GlossyDistribution.Beckmann
    | Distribution.GGX -> 
      (u.ShaderNode :?> ccl.ShaderNodes.GlossyBsdfNode).Distribution <- ccl.ShaderNodes.GlossyBsdfNode.GlossyDistribution.GGX
    | Distribution.Multiscatter_GGX ->
      (u.ShaderNode :?> ccl.ShaderNodes.GlossyBsdfNode).Distribution <- ccl.ShaderNodes.GlossyBsdfNode.GlossyDistribution.Multiscatter_GGX
    | Distribution.Ashihkmin_Shirley ->
      (u.ShaderNode :?> ccl.ShaderNodes.GlossyBsdfNode).Distribution <- ccl.ShaderNodes.GlossyBsdfNode.GlossyDistribution.Asihkmin_Shirley
    base.SolveInstance(DA)
  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Distribution", u.Distribution.ToString) |> ignore
    base.Write(writer)
  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Distribution") then
      u.Distribution <-
        let d = Distribution.FromString (reader.GetString "Distribution")
        match d with | Option.None -> Sharp | _ -> d.Value
    base.Read(reader)
  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendMenu (it:Distribution) =
      GH_DocumentObject.Menu_AppendItem(menu, it.ToStringR, (fun _ _ -> u.Distribution <- it; u.ExpireSolution true),
        true, u.Distribution = it) |> ignore
    appendMenu Sharp
    appendMenu Beckmann
    appendMenu GGX
    appendMenu Multiscatter_GGX
    appendMenu Ashihkmin_Shirley

type EmissionBsdf() =
  inherit CyclesNode(
    "Emission BSDF", "emission",
    "Emission BSDF node for shader graph",
    "Shader", "BSDF",
    typeof<ccl.ShaderNodes.EmissionNode>)
  override u.ComponentGuid = u |> ignore; new Guid("aa365407-8e36-4400-b1a7-46cde5b21de6")
  override u.Icon = u |> ignore; Icons.Emission

type SubsurfaceScatteringBsdf() =
  inherit CyclesNode(
    "Subsurface Scattering BSDF", "subsurface scattering",
    "Subsurface Scattering BSDF node for shader graph",
    "Shader", "BSDF",
    typeof<ccl.ShaderNodes.SubsurfaceScatteringNode>)
  member val Falloff = Cubic with get, set
  override u.ComponentGuid = u |> ignore; new Guid("8b3abb10-4593-4f34-b96f-cb4ed0f64ae7")
  override u.Icon = u |> ignore; Icons.Emission


type DisneyBsdf() =
  inherit CyclesNode("Disney BSDF", "disney",
    "Disney BSDF node for shader graph", "Shader", "BSDF", typeof<ccl.ShaderNodes.UberBsdfNode>)
  member val Distribution = Multiscatter_GGX with get, set
  override u.ComponentGuid = u |> ignore; new Guid("1480e1aa-7ad4-42e3-b626-62af011917a9")
  override u.Icon = u |> ignore; Icons.Emission

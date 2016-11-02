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
    member u.GetXml node nickname inputs iteration =
      let x = Utils.GetInputsXml (inputs, iteration)
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
        match d with Option.None -> Linear | _ -> d.Value

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.Gradient.toStringR

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendMenu (gt:GradientTypes) =
      GH_DocumentObject.Menu_AppendItem(
        menu,
        gt.toStringR,
        (fun _ _ -> u.Gradient <- gt; u.ExpireSolution true),
        true,
        u.Gradient = gt) |> ignore
    appendMenu Linear
    appendMenu Easing
    appendMenu Quadratic
    appendMenu Diagonal
    appendMenu Radial
    appendMenu Quadratic_Sphere
    appendMenu Spherical

  interface ICyclesNode with
    member u.NodeName = "gradient_texture"
    member u.GetXml node nickname inputs iteration =
      let x = Utils.GetInputsXml (inputs, iteration)
      let t = String.Format(" interpolation=\"{0}\" ", u.Gradient.toStringR)
      "<" + Utils.GetNodeXml node nickname (x+t) + " />"

type MusgraveTypes =
  | Multifractal
  | FBM
  | Hybrid_Multifractal
  | Ridged_Multifractal
  | Hetero_Terrain
  member u.toString = Utils.toString u
  member u.toStringR = (u.toString).Replace("FB", "fB").Replace("_", " ")
  static member fromString s = Utils.fromString<MusgraveTypes> s

type MusgraveTextureNode() =
  inherit GH_Component("Musgrave", "musgrave", "Musgrave", "Shader", "Texture")

  member val Musgrave = FBM with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.Zero) |> ignore
    mgr.AddNumberParameter("Scale", "S", "Scale", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Detail", "D", "Detail", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Dimension", "Dim", "Dimension", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Lacunarity", "L", "Lacunarity", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Offset", "O", "Offset", GH_ParamAccess.item, 5.0) |> ignore
    mgr.AddNumberParameter("Gain", "G", "Gain", GH_ParamAccess.item, 5.0) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Fac", "F", "Fac", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("3ed2f77b-373c-4eb8-b6fb-253dff125065")

  override u.Icon = Icons.Emission

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Musgrave", u.Musgrave.toString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Musgrave") then
      u.Musgrave <-
        let d = MusgraveTypes.fromString (reader.GetString "Musgrave")
        match d with Option.None -> MusgraveTypes.FBM | _ -> d.Value

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.Musgrave.toStringR

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendMenu (gt:MusgraveTypes) =
      GH_DocumentObject.Menu_AppendItem(
        menu,
        gt.toStringR,
        (fun _ _ -> u.Musgrave <- gt; u.ExpireSolution true),
        true,
        u.Musgrave = gt) |> ignore
    appendMenu Multifractal
    appendMenu FBM
    appendMenu Hybrid_Multifractal
    appendMenu Ridged_Multifractal
    appendMenu Hetero_Terrain

  interface ICyclesNode with
    member u.NodeName = "musgrave_texture"
    member u.GetXml node nickname inputs iteration =
      let x = Utils.GetInputsXml (inputs, iteration)
      let t = String.Format(" musgrave_type=\"{0}\" ", u.Musgrave.toStringR)
      "<" + Utils.GetNodeXml node nickname (x+t) + " />"

type ImageTextureNode() =
  inherit GH_Component("Image", "image", "Image", "Shader", "Texture")

  member val ImageFile = "" with get, set
  member val Interpolation = Interpolation.Linear with get, set
  member val Projection = TextureProjection.Flat with get, set
  member val TextureExtension = Repeat with get, set
  member val ColorSpace = ColorSpace.None with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.Zero) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Alpha", "A", "Alpha", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("078f4865-e362-4ed1-818d-94fd9432ac77")

  override u.Icon = Icons.Emission

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Image", u.ImageFile) |> ignore
    writer.SetString("ImTexInterpolation", u.Interpolation.ToString) |> ignore
    writer.SetString("ImTexColorSpace", u.ColorSpace.ToString) |> ignore
    writer.SetString("ImTexTextureExtension", u.TextureExtension.ToString) |> ignore
    writer.SetString("ImTexProjection", u.Projection.ToString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Image") then
      u.ImageFile <- reader.GetString "Image"
    if reader.ItemExists("ImTexInterpolation") then
      let interp = Interpolation.FromString (reader.GetString "ImTexInterpolation")
      u.Interpolation <-
        match interp with | Option.None -> Interpolation.None | _ -> interp.Value
    if reader.ItemExists("ImTexColorSpace") then
      let colorspace = ColorSpace.FromString (reader.GetString "ImTexColorSpace")
      u.ColorSpace <-
        match colorspace with | Option.None -> ColorSpace.None | _ -> colorspace.Value
    if reader.ItemExists("ImTexTextureExtension") then
      let extension = TextureExtension.FromString (reader.GetString "ImTexTextureExtension")
      u.TextureExtension <-
        match extension with | Option.None -> TextureExtension.Repeat | _ -> extension.Value
    if reader.ItemExists("ImTexProjection") then
      let projection = TextureProjection.FromString (reader.GetString "ImTexProjection")
      u.Projection <-
        match projection with | Option.None -> TextureProjection.Flat | _ -> projection.Value

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- System.IO.Path.GetFileName(u.ImageFile)

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendInterpolationMenu (gt:Interpolation) =
      GH_DocumentObject.Menu_AppendItem(
        menu,
        gt.ToStringR,
        (fun _ _ -> u.Interpolation <- gt; u.ExpireSolution true),
        true,
        u.Interpolation = gt) |> ignore
    let appendExtensionMenu (gt:TextureExtension) =
      GH_DocumentObject.Menu_AppendItem(
        menu, gt.ToStringR,
        (fun _ _ -> u.TextureExtension <- gt; u.ExpireSolution true),
        true,
        u.TextureExtension = gt) |> ignore
    let appendColorspaceMenu (gt:ColorSpace) =
      GH_DocumentObject.Menu_AppendItem(
       menu,
       gt.ToStringR,
       (fun _ _ -> u.ColorSpace <- gt; u.ExpireSolution true),
       true, u.ColorSpace = gt) |> ignore
    let appendProjectionMenu (gt:TextureProjection) =
      GH_DocumentObject.Menu_AppendItem(menu, gt.ToStringR, (fun _ _ -> u.Projection <- gt; u.ExpireSolution true),
        true, u.Projection = gt) |> ignore
    let fd = lazy (
      let mutable fdi = new System.Windows.Forms.OpenFileDialog()
      fdi.Filter <- "Image Files(*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|All files (*.*)|*.*"
      let res = fdi.ShowDialog()
      match res with
      | DialogResult.OK -> fdi.FileName
      | _ -> ""
      )
    GH_DocumentObject.Menu_AppendItem(menu, "Select Image File...",
      (fun _ _ -> u.ImageFile <- fd.Force(); u.ExpireSolution(true))) |> ignore
    GH_DocumentObject.Menu_AppendSeparator(menu) |> ignore
    appendInterpolationMenu Interpolation.None
    appendInterpolationMenu Interpolation.Linear
    appendInterpolationMenu Interpolation.Closest
    appendInterpolationMenu Interpolation.Cubic
    appendInterpolationMenu Interpolation.Smart
    GH_DocumentObject.Menu_AppendSeparator(menu) |> ignore
    appendExtensionMenu TextureExtension.Repeat
    appendExtensionMenu TextureExtension.Extend
    appendExtensionMenu TextureExtension.Clip
    GH_DocumentObject.Menu_AppendSeparator(menu) |> ignore
    appendColorspaceMenu ColorSpace.None
    appendColorspaceMenu ColorSpace.Color
    GH_DocumentObject.Menu_AppendSeparator(menu) |> ignore
    appendProjectionMenu TextureProjection.Flat
    appendProjectionMenu TextureProjection.Box
    appendProjectionMenu TextureProjection.Sphere
    appendProjectionMenu TextureProjection.Tube

  interface ICyclesNode with
    member u.NodeName = "image_texture"
    member u.GetXml node nickname inputs iteration =
      let x = Utils.GetInputsXml (inputs, iteration)
      let t = String.Format(" src=\"{0}\" ", u.ImageFile)
      let interp = String.Format(" interpolation=\"{0}\" ", u.Interpolation.ToString)
      let extension = String.Format(" extension=\"{0}\" ", u.TextureExtension.ToString)
      let projection = String.Format(" projection=\"{0}\" ", u.Projection.ToString)
      let cs = String.Format(" color_space=\"{0}\" ", u.ColorSpace.ToString)
      "<" + Utils.GetNodeXml node nickname (x+t+interp+extension+projection+cs) + " />"

type EnvironmentTextureNode() =
  inherit GH_Component("Environment", "environment", "Environment", "Shader", "Texture")

  member val EnvironmentFile = "" with get, set
  member val Projection = EnvironmentProjection.Equirectangular with get, set
  member val Interpolation = Interpolation.Linear with get, set
  member val ColorSpace = ColorSpace.None with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.Zero) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Alpha", "A", "Alpha", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("07e09721-e982-4ea4-8358-1dfc4d26cda4")

  override u.Icon = Icons.Emission

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Environment", u.EnvironmentFile) |> ignore
    writer.SetString("Projection", u.Projection.ToStringR) |> ignore
    writer.SetString("Interpolation", u.Interpolation.ToStringR) |> ignore
    writer.SetString("ColorSpace", u.ColorSpace.ToStringR) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Environment") then
      u.EnvironmentFile <- reader.GetString "Environment"
    if reader.ItemExists("Projection") then
      let interp = EnvironmentProjection.FromString (reader.GetString "Projection")
      u.Projection <-
        match interp with | Option.None -> EnvironmentProjection.Equirectangular | _ -> interp.Value
    if reader.ItemExists("Interpolation") then
      let interp = Interpolation.FromString (reader.GetString "Interpolation")
      u.Interpolation <-
        match interp with | Option.None -> Interpolation.Linear | _ -> interp.Value
    if reader.ItemExists("ColorSpace") then
      let interp = ColorSpace.FromString (reader.GetString "Interpolation")
      u.ColorSpace <-
        match interp with | Option.None -> ColorSpace.None | _ -> interp.Value

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- System.IO.Path.GetFileName(u.EnvironmentFile)

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendInterpolationMenu (gt:Interpolation) =
      GH_DocumentObject.Menu_AppendItem(menu, gt.ToStringR, (fun _ _ -> u.Interpolation <- gt; u.ExpireSolution true),
        true, u.Interpolation = gt) |> ignore
    let appendColorspaceMenu (gt:ColorSpace) =
      GH_DocumentObject.Menu_AppendItem(menu, gt.ToStringR, (fun _ _ -> u.ColorSpace <- gt; u.ExpireSolution true),
        true, u.ColorSpace = gt) |> ignore
    let appendProjectionMenu (gt:EnvironmentProjection) =
      GH_DocumentObject.Menu_AppendItem(menu, gt.ToStringR, (fun _ _ -> u.Projection <- gt; u.ExpireSolution true),
        true, u.Projection = gt) |> ignore
    let fd = lazy (
      let mutable fdi = new System.Windows.Forms.OpenFileDialog()
      fdi.Filter <- "Image Files(*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|All files (*.*)|*.*"
      let res = fdi.ShowDialog()
      match res with
      | DialogResult.OK -> fdi.FileName
      | _ -> ""
      )
    GH_DocumentObject.Menu_AppendItem(menu, "Select Environment File...", (fun _ _ -> u.EnvironmentFile <- fd.Force(); u.ExpireSolution(true))) |> ignore
    GH_DocumentObject.Menu_AppendSeparator(menu) |> ignore
    appendProjectionMenu EnvironmentProjection.Equirectangular
    appendProjectionMenu EnvironmentProjection.Mirror_Ball
    appendProjectionMenu EnvironmentProjection.Wallpaper
    GH_DocumentObject.Menu_AppendSeparator(menu) |> ignore
    appendInterpolationMenu Interpolation.None
    appendInterpolationMenu Interpolation.Linear
    appendInterpolationMenu Interpolation.Closest
    appendInterpolationMenu Interpolation.Cubic
    appendInterpolationMenu Interpolation.Smart
    GH_DocumentObject.Menu_AppendSeparator(menu) |> ignore
    appendColorspaceMenu ColorSpace.None
    appendColorspaceMenu ColorSpace.Color

  interface ICyclesNode with
    member u.NodeName = "environment_texture"
    member u.GetXml node nickname inputs iteration =
      let x = Utils.GetInputsXml (inputs, iteration)
      let t = String.Format(" src=\"{0}\" ", u.EnvironmentFile)
      "<" + Utils.GetNodeXml node nickname (x+t) + " />"

type WaveTypes =
  | Bands
  | Rings
  member u.toString = Utils.toString u
  member u.toStringR = u.toString
  static member fromString s = Utils.fromString<WaveTypes> s

type WaveProfiles =
  | Sine
  | Saw
  member u.toString = Utils.toString u
  member u.toStringR = u.toString
  static member fromString s = Utils.fromString<WaveProfiles> s

type WaveTextureNode() =
  inherit GH_Component("Wave", "wave", "Wave", "Shader", "Texture")

  member val Wave = Bands with get, set
  member val Profile = Sine with get, set

  override u.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.Zero) |> ignore
    mgr.AddNumberParameter("Scale", "S", "Scale", GH_ParamAccess.item, 1.0) |> ignore
    mgr.AddNumberParameter("Detail", "D", "Detail", GH_ParamAccess.item, 2.0) |> ignore
    mgr.AddNumberParameter("Detail Scale", "DS", "Detail Scale", GH_ParamAccess.item, 1.0) |> ignore
    mgr.AddNumberParameter("Distortion", "Dx", "Distortion", GH_ParamAccess.item, 0.0) |> ignore

  override u.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    mgr.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item) |> ignore
    mgr.AddNumberParameter("Fac", "F", "Fac", GH_ParamAccess.item) |> ignore

  override u.ComponentGuid = new Guid("89660e7d-cf92-4fed-b61c-0231edd76504")

  override u.Icon = Icons.Emission

  override u.Write(writer:GH_IO.Serialization.GH_IWriter) =
    writer.SetString("Wave", u.Wave.toString) |> ignore
    writer.SetString("Profile", u.Profile.toString) |> ignore
    base.Write(writer)

  override u.Read(reader:GH_IO.Serialization.GH_IReader) =
    if reader.ItemExists("Wave") then
      u.Wave <-
        let d = WaveTypes.fromString (reader.GetString "Wave")
        match d with Option.None -> Bands | _ -> d.Value

    if reader.ItemExists("Profile") then
      u.Profile <-
        let d = WaveProfiles.fromString (reader.GetString "Profile")
        match d with Option.None -> Sine | _ -> d.Value

    base.Read(reader)

  override u.SolveInstance(DA: IGH_DataAccess) =
    u.Message <- u.Wave.toStringR

    DA.SetData(0, new GH_Colour(Color.Beige)) |> ignore
    DA.SetData(1, 0.5) |> ignore

  override u.AppendAdditionalComponentMenuItems(menu:ToolStripDropDown) =
    let appendMenu (gt:WaveTypes) =
      GH_DocumentObject.Menu_AppendItem(menu, gt.toStringR, (fun _ _ -> u.Wave <- gt; u.ExpireSolution true),
        true, u.Wave = gt) |> ignore
    appendMenu Bands
    appendMenu Rings
    GH_DocumentObject.Menu_AppendSeparator(menu) |> ignore
    let appendProfileMenu (gt:WaveProfiles) =
      GH_DocumentObject.Menu_AppendItem(menu, gt.toStringR, (fun _ _ -> u.Profile <- gt; u.ExpireSolution true),
        true, u.Profile = gt) |> ignore
    appendProfileMenu Sine
    appendProfileMenu Saw

  interface ICyclesNode with
    member u.NodeName = "wave_texture"
    member u.GetXml node nickname inputs iteration =
      let x = Utils.GetInputsXml (inputs, iteration)
      let t = String.Format(" wave_type=\"{0}\" ", u.Wave.toStringR)
      "<" + Utils.GetNodeXml node nickname (x+t) + " />"

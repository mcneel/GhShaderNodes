namespace CyclesRenderer

open System
open System.Linq
open System.Text
open System.IO
open System.Collections.Generic
open System.Drawing

open System.Windows.Forms

open Grasshopper
open Grasshopper.Kernel
open Grasshopper.Kernel.Types
open Grasshopper.Kernel.Attributes

open Grasshopper.GUI
open Grasshopper.GUI.Canvas

open ShaderGraphResources

open ccl

type Priority() =
  inherit GH_AssemblyPriority()
  override I.PriorityLoad() =
    let ass = System.Reflection.Assembly.GetAssembly(typeof<CSycles>)
    let l = ass.Location
    let path = Path.GetDirectoryName(l)
    let user_path = Path.Combine(path, "RhinoCycles")
    let app_path = Path.Combine(path, "RhinoCycles")
    let mutable inited = false

    Grasshopper.Instances.DocumentServer.add_DocumentAdded(
      fun serv _ ->
        if serv.DocumentCount > 0 && not inited then
          CSycles.path_init(app_path, user_path)
          CSycles.initialise()
          inited <- true
    )

    Grasshopper.Instances.DocumentServer.add_DocumentRemoved(
      fun serv _ ->
        if serv.DocumentCount < 1 then
          CSycles.shutdown()
          inited <- false
    )
    GH_LoadingInstruction.Proceed

type Info() =
  inherit GH_AssemblyInfo()

  override I.Name = "Cycles Render Engine"
  override I.Description = "Cycles renderer integration for Grasshopper"
  override I.Id = new Guid("55098e5d-a603-49e6-8977-6178ddbbfda5")
  override I.Icon = Icons.Cycles
  override I.AuthorName = "Nathan 'jesterKing' Letwory"
  override I.AuthorContact = "nathan@mcneel.com"

type CyclesRenderer()  =
  inherit GH_Component("Cycles", "cycles", "Cycles renderer", "Render", "Cycles")
  let m_client = new Client()
  let m_bitmaplock = new obj()
  let Samples = 50
  let mutable (m_session:Session) = null
  let mutable m_inited = false
  let mutable (m_render:Bitmap) = null
  let mutable m_hasdata = false
  let shaders = new List<Shader>()
  let meshes = new List<Mesh>()
  let objects = new List<ccl.Object>()

  let rnd = new Random()

  member I.PauseRendering() =
    match m_session with
    | null -> ()
    | _ ->
      m_session.SetPause true

  member I.RenderSize
    with get() =
      match I.m_attributes with
      | null -> new Size(50, 50)
      | _ ->
        let attr = (I.m_attributes :?> CyclesRendererAttributes)
        let mutable size = (attr.RenderDimension:Rectangle).Size
        if size.Width < 50 then size.Width <- 50
        if size.Height < 50 then size.Height  <- 50
        size

  member I.ContinueRendering() =
    match m_session with
    | null -> ()
    | _ ->
      let size = I.RenderSize
      m_session.Reset((uint32)size.Width, (uint32)size.Height, (uint32)Samples)
      m_session.Scene.Reset()
      m_session.SetPause false

  member I.IsInited
    with get() = m_inited

  member I.BitmapLock
    with get() = m_bitmaplock

  member I.Bitmap
    with get() = m_render

  override I.CreateAttributes() =
    I.m_attributes <- new CyclesRendererAttributes(I)

  override I.ComponentGuid = new Guid("4cfa4098-e7ec-4c43-a00d-e14f965f80a0")

  override I.Icon = Icons.Cycles

  override I.RegisterInputParams(mgr : GH_Component.GH_InputParamManager) =
    mgr.AddMeshParameter("Meshes", "M", "Collection of meshes", GH_ParamAccess.list) |> ignore
    //mgr.AddPointParameter("Locations", "L", "Collection of locations", GH_ParamAccess.list) |> ignore
    mgr.AddTextParameter("Shader XML", "S", "Shader XML definition list", GH_ParamAccess.list) |> ignore
    mgr.AddPointParameter("Camera Location", "CL", "Camera location", GH_ParamAccess.item) |> ignore
    mgr.AddPointParameter("Camera LookAt", "LA", "Camera look-at location", GH_ParamAccess.item) |> ignore
    //mgr.AddVectorParameter("Camera Up", "CU", "Camera up vector", GH_ParamAccess.item) |> ignore

  override I.RegisterOutputParams(mgr : GH_Component.GH_OutputParamManager) =
    ()

  override I.SolveInstance(DA : IGH_DataAccess) =
    I.PauseRendering()
    m_session.Scene.Lock()
    let mutable meshlist = new List<GH_Mesh>()
    //let mutable locs = new List<GH_Point>()
    let mutable xmlshaders = new List<GH_String>()
    let mutable cl = new GH_Point()
    let mutable la = new GH_Point()
    //let mutable up = new GH_Vector()
    let mutable md = DA.GetDataList(0, meshlist)
    match md with
    | false -> m_hasdata <- false
    | true -> ()
(*    md <- DA.GetDataList(1, locs)
    match md with
    | false -> m_hasdata <- false
    | true -> () *)
    md <- DA.GetDataList(1, xmlshaders)
    match md with
    | false -> m_hasdata <- false
    | true -> ()
    md <- DA.GetData(2, &cl) 
    match md with
    | false -> m_hasdata <- false
    | true -> ()
    md <- DA.GetData(3, &la)
    match md with
    | false -> m_hasdata <- false
    | true -> ()
(*    md <- DA.GetData(5, &up) 
    match md with
    | false -> m_hasdata <- false
    | true -> () *)

    let tfm = (Transform.LookAt(
                new float4((float32)cl.Value.X, (float32)cl.Value.Y, (float32)cl.Value.Z),
                new float4((float32)la.Value.X, (float32)la.Value.Y, (float32)la.Value.Z),
                new float4(0.0f, 0.0f, -1.0f)
    ) * Transform.Scale(1.0f, -1.0f, 1.0f))

    let mutable shidx = 0
    for ghstring in xmlshaders do
      let shaderXml = ghstring.Value;
      let mutable sh = shaders.ElementAtOrDefault(shidx);
      let mutable addnew = false;

      if sh = null then
        sh <- new ccl.Shader(m_client, Shader.ShaderType.Material)
        sh.Name <- Guid.NewGuid.ToString()
        addnew <- true;

      sh.Recreate();
      let xmlmem = Encoding.UTF8.GetBytes(shaderXml);
      ( use xmlstream = new MemoryStream(xmlmem)
        let settings = new System.Xml.XmlReaderSettings(ConformanceLevel = System.Xml.ConformanceLevel.Fragment, IgnoreComments = true, IgnoreProcessingInstructions = true, IgnoreWhitespace = true)
        let reader = System.Xml.XmlReader.Create(xmlstream, settings);

        ccl.Utilities.Instance.ReadNodeGraph(&sh, reader);
      )

      match addnew with
      | true ->
          shaders.Add(sh)
          m_session.Scene.AddShader(sh) |> ignore
      | _ -> shaders.[shidx] <- sh

      sh.Tag();

      shidx <- shidx+1

    let mutable midx = 0
    let mutable cursh:Shader = null;
    let p = new GH_Point()
    for mi in meshlist do
      let meshdata = mi.Value
      let mutable newmesh = false
      let mutable newob = false
      let mutable me = meshes.ElementAtOrDefault(midx)
      cursh <- shaders.ElementAtOrDefault(midx)
      
      if cursh=null then cursh <- shaders.First()

      match me with
      | null ->
        me <- new Mesh(m_client, cursh)
        newmesh <- true
      | _ ->
        me.ClearData()


      // Get face indices flattened to an
      // integer array.
      let findices = meshdata.Faces.ToIntArray(true)

      let verts = meshdata.Vertices.ToFloatArray()
      me.SetVerts(ref verts)
      me.SetVertTris(ref findices, false)
      if not newmesh then me.ReplaceShader(cursh)

      me.TagRebuild();

      let mutable o = objects.ElementAtOrDefault(midx);
      match o with
      | null ->
        newob <- true;
        o <- new ccl.Object(m_client)
      | _ -> ()

      o.Mesh <- me
      let otfm = Transform.Translate(
                  (float32)p.Value.X,
                  (float32)p.Value.Y,
                  (float32)p.Value.Z
        )
      o.Transform <- otfm
      o.Visibility <- PathRay.AllVisibility
      o.TagUpdate()

      match newmesh with
      | true -> meshes.Add(me)
      | _ -> meshes.[midx] <- me

      match newob with
      | true -> objects.Add(o)
      | _ -> objects.[midx] <- o;
      midx <- midx+1

    m_hasdata <- true
    m_session.Scene.Camera.Matrix <- tfm;
    m_session.Scene.Unlock()
    I.ContinueRendering()

  override I.RemovedFromDocument(document:GH_Document) =
    match m_session with
    | null -> ()
    | _ ->
      m_session.Cancel("done with it")
      m_session.Destroy()

    base.RemovedFromDocument(document);
  override I.AddedToDocument(document:GH_Document) =
    let dev = Device.FirstCuda
    let attr = I.m_attributes :?> CyclesRendererAttributes
    match I.m_attributes with
    | null -> ()
    | _ -> attr.RenderDimensionChanged |> Observable.add (fun x -> 
      if m_inited then
        I.PauseRendering()
        let resize() = 
          m_render <- new Bitmap((x:Size).Width, (x:Size).Height)
        lock m_bitmaplock resize
        I.ContinueRendering()
      )
    I.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, String.Format("Using device {0} {1}", dev.Name, dev.Description))

    let scene_params = new SceneParameters(m_client, ShadingSystem.SVM, BvhType.Static, false, false, false)
    let scene = new Scene(m_client, scene_params, dev)

    let xml = new CSyclesXmlReader(m_client, @"D:\Dev\CCSycles\tests\default.xml")
    xml.Parse(true)
    let size = I.RenderSize;
    scene.Camera.Size <- size;
    lock m_bitmaplock (fun _ ->
      m_render <- new Bitmap(size.Width, size.Height)
    )

    let session_params = new SessionParameters(m_client, dev)
    session_params.Experimental <- false
    session_params.Samples <- Samples
    session_params.TileSize <- match dev.IsCpu with true -> new Size(32, 32) | _ -> new Size(256, 256)
    session_params.Threads <- (uint32 (match dev.IsCpu with true -> 2 | _ -> 0))
    session_params.ShadingSystem <- ShadingSystem.SVM
    session_params.StartResolution <- 128
    session_params.SkipLinearToSrgbConversion <- true
    session_params.Background <- false
    session_params.Progressive <- true
    session_params.ProgressiveRefine <- true

    m_session <- new Session(m_client, session_params, scene)
    m_session.Reset((uint32)size.Width, (uint32)size.Height, (uint32)Samples);

    (*m_log = Log;
    m_cb = null;
    m_up = null;
    m_display = DisplayUpdate;
    Session.UpdateCallback = m_up;
    Session.UpdateTileCallback = null;
    Session.WriteTileCallback = m_cb;
    Session.DisplayUpdateCallback = m_display;
    CSycles.set_logger(m_client.Id, m_log);*)

    CSycles.set_logger(m_client.Id, 
      fun s -> System.Diagnostics.Debug.WriteLine(s)
    )

    m_session.DisplayUpdateCallback <-
     (fun sid sample ->
        let rd = I.RenderSize
        let mutable bufsize = (uint32)0
        let mutable bufstride = (uint32)0

        lock m_bitmaplock (fun _ -> 
          CSycles.session_get_buffer_info(m_client.Id, m_session.Id, &bufsize, &bufstride)
          if m_render<>null && bufsize > (uint32)0 && bufsize = (uint32 (rd.Width * rd.Height * 4)) && m_render.Size = rd then
            m_session.DrawNogl(rd.Width, rd.Height)
            let pixels = CSycles.session_copy_buffer(m_client.Id, m_session.Id, bufsize)
            for x in [0..rd.Width-1] do
              for y in [0..rd.Height-1] do
                let i = y * (int)rd.Width * 4 + x * 4;
                let r = (int)(max (min (pixels.[i] * 255.0f) 255.0f) 0.0f);
                let g = (int)(max (min (pixels.[i+1] * 255.0f) 255.0f) 0.0f);
                let b = (int)(max (min (pixels.[i+2] * 255.0f) 255.0f) 0.0f);
                let a = 255;
                match m_hasdata with
                | true -> m_render.SetPixel(x, y, Color.FromArgb(a, r, g, b))
                | _ -> m_render.SetPixel(x, y, Color.FromArgb(255, rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255)))
        ) |> ignore
        match sample with
        | sample when (sample % 10) = 0 -> Grasshopper.Instances.InvalidateCanvas()
        | sample when Samples = sample -> Grasshopper.Instances.InvalidateCanvas()
        | sample when (Samples - 1) = sample -> Grasshopper.Instances.InvalidateCanvas()
        | sample when (Samples - 2) = sample -> Grasshopper.Instances.InvalidateCanvas()
        | _ -> ()
     )

    m_inited <- true
    m_session.Start()
    base.AddedToDocument(document)

// dependency cycle, so using 'and' keyword here to
// allow these two types to know about each other
and CyclesRendererAttributes(owner:CyclesRenderer) as i =
  inherit GH_ResizableAttributes<CyclesRenderer>(owner)
  let minimumSize = new Size(180, 100)
  let padding = new Padding(0,0,8,8)
  let mutable m_cyclesRect = new RectangleF();

  // dimension changed event
  let dimchangedEvt = new Event<_>()

  do
    i.Bounds <- RectangleF.op_Implicit (new Rectangle(Point.Empty, minimumSize))

  member I.RenderDimension with get() = GH_Convert.ToRectangle(m_cyclesRect)

  member I.RenderDimensionChanged = dimchangedEvt.Publish
  override I.MinimumSize with get() = minimumSize
  override I.SizingBorders with get() = padding

  override I.AppendToAttributeTree(attr:List<IGH_Attributes>) =
    attr.Add(I)
    for param in I.Owner.Params do
      param.Attributes.AppendToAttributeTree(attr)

  override I.Layout() =
    let old = m_cyclesRect
    I.Bounds <- new System.Drawing.RectangleF(I.Pivot.X, I.Pivot.Y, I.Bounds.Width, I.Bounds.Height)
    let mutable fauxBox = I.Bounds
    fauxBox.Inflate(-5.0f, -5.0f)
    //Layout all input parameters for the first time, so we know how big they all are.
    GH_ComponentAttributes.LayoutInputParams(I.Owner, fauxBox)
    // Now adjust the fauxbox so we can layout properly
    fauxBox.X <- fauxBox.X + (I.Owner.Params.Input.[0].Attributes.Bounds.Width + 4.0f)
    //Layout all input parameters for the second and last time, making sure they all end up at the correct place.
    GH_ComponentAttributes.LayoutInputParams(I.Owner, fauxBox);

    let x0 = I.Owner.Params.Input.[0].Attributes.Bounds.Right + 2.0f;
    let x1 = I.Bounds.Right - 8.0f;
    let y0 = I.Bounds.Top + 3.0f;
    let y1 = I.Bounds.Bottom - 3.0f;

    m_cyclesRect <- RectangleF.FromLTRB(x0, y0, x1, y1);
    //m_cyclesRect = GH_Convert.ToRectangle(m_cyclesRect);

    if old <> m_cyclesRect then
      dimchangedEvt.Trigger(I.RenderDimension.Size)

  override I.Render(canvas:GH_Canvas, graphics:Graphics, channel:GH_CanvasChannel) =
    match channel with
    | GH_CanvasChannel.Wires -> for p in I.Owner.Params.Input do p.Attributes.RenderToCanvas(canvas, channel)
    | GH_CanvasChannel.Objects ->
      let palette =
        match I.Owner.RuntimeMessageLevel with
        | GH_RuntimeMessageLevel.Warning -> GH_Palette.Warning
        | GH_RuntimeMessageLevel.Error -> GH_Palette.Error
        | _ -> GH_Palette.Normal

      // Create a new Capsule without text or icon.
      let capsule = GH_Capsule.CreateCapsule(I.Bounds, palette)
      for inp in I.Owner.Params.Input do capsule.AddInputGrip(inp.Attributes.InputGrip.Y)
      capsule.Render(graphics, I.Selected, I.Owner.Locked, I.Owner.Hidden)
      // Always dispose of a GH_Capsule when you're done with it.
      capsule.Dispose();

      let render_rect = GH_Convert.ToRectangle(m_cyclesRect);
      let p = new Pen(Color.Black);

      p.DashPattern <- [| 2.0f; 2.0f |]
      p.Width <- 1.0f
      GH_GraphicsUtil.ShadowRectangle(graphics, render_rect, 10)
      graphics.DrawRectangle(p, render_rect)
      p.Dispose()

      match I.Owner.IsInited with
      | false -> ()
      | true ->
        lock I.Owner.BitmapLock ( fun _ -> graphics.DrawImage(I.Owner.Bitmap, render_rect.Location)) |> ignore

      let style = GH_CapsuleRenderEngine.GetImpliedStyle(palette, I.Selected, I.Owner.Locked, I.Owner.Hidden)
      GH_ComponentAttributes.RenderComponentParameters(canvas, graphics, I.Owner, style) |> ignore
    | _ -> ()

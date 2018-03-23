using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ccl;

using RhinoCyclesCore.Materials;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Attributes;

using ShaderGraphResources;

namespace ShaderGraphComponents
{
	public class OutputNode : CyclesNode
	{
		public OutputNode()
			: base(
					"Output", "output",
					"Output node for shader graph",
					"Shader", "Output",
					typeof(ccl.ShaderNodes.OutputNode))
		{
		}
		private List<Guid> matId = new List<Guid>();

		protected override void RegisterOutputParams(GH_OutputParamManager mgr)
		{
			mgr.AddTextParameter("Xml", "X", "tree as xml", GH_ParamAccess.item);
		}

		public override Guid ComponentGuid => new Guid("14df22af-d119-4f69-a536-34a30ddb175e");

		protected override Bitmap Icon => Icons.Output;

		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			var rms = Rhino.RhinoDoc.ActiveDoc.RenderMaterials.Where(x => x is XmlMaterial).Select(i => (i.Name, i.Id)).Distinct();
			foreach(var rm in rms)
			{
				GH_DocumentObject.Menu_AppendItem(
					menu,
					rm.Item1,
					(_, __) => { if (matId.Contains(rm.Item2)) matId.Remove(rm.Item2); else matId.Add(rm.Item2); },
					true,
					matId.Contains(rm.Item2));
			}
		}
		/*
			member u.IsBackground =
				match u.Params.Input.[0].SourceCount>0 with
				| false -> false
				| true ->
					let rec hasBgNode (n:GH_Component) (acc:bool) =
						[for inp in n.Params.Input ->
							match inp.SourceCount>0 with
							| false -> acc
							| true ->
								let s = inp.Sources.[0]
								match s with
								| :? GH_NumberSlider -> acc
								| :? GH_ColourPickerObject -> acc
								| _ -> 
									let attrp = Utils.castAs<GH_ComponentAttributes>(s.Attributes.Parent)
									match attrp with
									| null -> acc
									| _ ->
										match attrp.Owner.ComponentGuid = new Guid("dd68810b-0a0e-4c54-b08e-f46b41e79f32") with
										| true -> acc
										| false -> hasBgNode (Utils.castAs<GH_Component>(attrp.Owner)) acc
						].Any(fun x -> x)

					hasBgNode u false
					*/

		protected override void SolveInstance(IGH_DataAccess da)
		{
			base.SolveInstance(da);
			Message = "";

			// create our code shader
			var theshader = new ccl.CodeShader(ccl.Shader.ShaderType.Material);

			// determine all nodes used for this shader
			var usednodes = UsedNodes(this, da.Iteration);

			// add all nodes to the shader
			foreach (var n in usednodes)
			{
				if (n is CyclesNode cn) {
					theshader.AddNode(cn.ShaderNode);
				}
			}

			// finalize the shader
			theshader.FinalizeGraph();
			var xmlcode = theshader.Xml + ShaderNode.CreateConnectXml() + "<!-- " + theshader.Code + ShaderNode.CreateConnectCode() + " -->";
			da.SetData(0, xmlcode);
		}

		private IGH_Param GetSource(int i, IGH_Param p) {
			return i < p.SourceCount ? p.Sources[i] : p.Sources.LastOrDefault();
		}

		/*
    let cleancontent (l:string) = l.Trim().Replace("\n", "")
    let linebreaks (l:string) = l.Replace(";", ";\n")
    let xmllinebreaks (l:string) = l.Replace(">", ">\n")
		*/

		

		private void ColConTags(List<object> acc, object _n, int iteration)
		{
			var n = _n as GH_Component;
			if (n is null) return;

			var dd = (from inp in n.Params.Input let s = GetSource(iteration, inp) where !(s is null) select s);

			var numsls = (from inp in dd where (inp is GH_NumberSlider) select (object)inp);
			var cols = (from inp in dd where (inp is GH_ColourPickerObject) select (object)inp);
			var others = (from inp in dd let attrp = inp.Attributes.Parent as GH_ComponentAttributes where !(attrp is null) select (object)attrp.Owner);

			var filtered = numsls.Concat(cols).Concat(others);

			foreach(var comp in filtered)
			{
				ColConTags(acc, comp, iteration);
			}

			acc.AddRange(filtered);
		}

		private List<object> UsedNodes(GH_Component n, int iteration) {
			List<object> ns = new List<object>();

			ColConTags(ns, n, iteration);

			ns = ns.Distinct().ToList();

			return ns;

		}
/*      let rec colcontags (acc: obj list) (_n:obj) =
        let n = Utils.castAs<GH_Component>(_n)
        match n with
        | null -> acc
        | _ ->
          let diveinto (inp:IGH_Param) =
            let s = getSource iteration inp
            let tst =
              match isNull s with
              | true -> null
              | false ->
                match s with
                | :? GH_NumberSlider -> Utils.castAs<obj>(s)
                | :? GH_ColourPickerObject -> Utils.castAs<obj>(s)
                | s when isNull s -> null
                | _ -> 
                  let attrp = Utils.castAs<GH_ComponentAttributes>(s.Attributes.Parent)
                  match attrp with
                  | null -> null
                  | _ -> Utils.castAs<obj>(attrp.Owner)
            tst
          let dd = n.Params.Input |> Seq.map diveinto
          
          let filteredCompAttrs =
            dd
            |> Seq.filter (fun x -> (isNull >> not) x)

          let deeperCompAttrs =
            filteredCompAttrs
            |> Seq.map (fun x -> colcontags [] x)

          let resCompAttrs =
            deeperCompAttrs
            |> Seq.concat
            |> List.ofSeq

          filteredCompAttrs |> List.ofSeq |> List.append resCompAttrs |> List.append acc

      colcontags [] n
			*/
		
/*
    let usednodes = usedNodes u da.Iteration |> Seq.distinct |> List.ofSeq

    let addtoshader (x:obj) =
      match x with
      | :? CyclesNode as cn -> theshader.AddNode(cn.ShaderNode)
      | _ -> ()

    let rr = usednodes |> List.iter addtoshader
    rr |> ignore
    theshader.FinalizeGraph() |> ignore

    let isBackgroundShader =
      let isBackgroundNode (o:obj) = 
        match o with
        | :? CyclesNode as n ->
          n.ComponentGuid.Equals(new Guid("dd68810b-0a0e-4c54-b08e-f46b41e79f32"))
        | _ -> false
      match List.tryFind isBackgroundNode usednodes with
      | Option.None -> false
      | _ -> true

    let newxmlcode =
      theshader.Xml + u.ShaderNode.CreateConnectXml()
      |> cleancontent
      |> xmllinebreaks
    let csharpcode =
      theshader.Code + u.ShaderNode.CreateConnectCode()
      |> cleancontent
      |> linebreaks

    let xmlcode = newxmlcode + "<!--\n" + csharpcode  + "\n-->"

    match isBackgroundShader with
    | true ->
      let env = Utils.castAs<XmlEnvironment>(Rhino.RhinoDoc.ActiveDoc.CurrentEnvironment.ForBackground)
      match env with
      | null -> u.Message <- "NO BACKGROUND"
      | _ ->
        env.BeginChange(Rhino.Render.RenderContent.ChangeContexts.Program)
        env.SetParameter("xmlcode", xmlcode) |> ignore
        env.EndChange()
        //Rhino.RhinoDoc.ActiveDoc.CurrentEnvironment.ForBackground <- env
      ()
    | false ->
      let m = 
        match matId.Count with
        | 0 -> null
        | _ ->
          let midx =
            match da.Iteration < matId.Count with
            | true -> da.Iteration
            | false -> matId.Count - 1
          Rhino.RhinoDoc.ActiveDoc.RenderMaterials.Where(fun m -> m.Id = matId.[midx]).FirstOrDefault()
      match m with
      | null ->
        u.Message <- "NO MATERIAL"
      | _ ->
        let m' = m :?> XmlMaterial
        m'.BeginChange(Rhino.Render.RenderContent.ChangeContexts.Program)
        m'.SetParameter("xmlcode", xmlcode) |> ignore
        m'.EndChange()
        match matId.Count > 1 with
        | true -> u.Message <- "multiple materials set"
        | _ -> u.Message <- m.Name
        for mm in Rhino.RhinoDoc.ActiveDoc.Materials.Where(fun x -> x.RenderMaterialInstanceId = m.Id) do
          mm.DiffuseColor <- Utils.randomColor
          mm.CommitChanges() |> ignore

    da.SetData(0, xmlcode ) |> ignore
		*/
	}
}

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
				Menu_AppendItem(
					menu,
					rm.Item1,
					(_, __) => { if (matId.Contains(rm.Item2)) matId.Remove(rm.Item2); else matId.Add(rm.Item2); },
					true,
					matId.Contains(rm.Item2));
			}
		}

		protected override void SolveInstance(IGH_DataAccess da)
		{
			base.SolveInstance(da);
			Message = "";

			// create our code shader
			var theshader = new ccl.CodeShader(ccl.Shader.ShaderType.Material);

			// determine all nodes used for this shader
			var usednodes = UsedNodes(this, da.Iteration);

			bool isBg = false;

			// add all nodes to the shader
			foreach (var n in usednodes)
			{
				if (n is CyclesNode cn) {
					isBg |= (cn is BSDF.BackgroundNode);
					theshader.AddNode(cn.ShaderNode);
				}
			}

			// finalize the shader
			theshader.FinalizeGraph();
			var xmlgraph = theshader.Xml.Trim().Replace("\n","").Replace(">",">\n") + ShaderNode.CreateConnectXml().Trim().Replace("\n", "").Replace(">",">\n");
			var codegraph = theshader.Code.Trim().Replace("\n","").Replace(";",";\n") + ShaderNode.CreateConnectCode().Trim().Replace("\n", "").Replace(";",";\n");
			var xmlcode = xmlgraph + "<!--\n" + codegraph + "\n-->";

			// Update XmlMaterial with shader.
			if (matId.Count() > 0 && xmlgraph.Length > 0)
			{
				var midx = da.Iteration < matId.Count ? da.Iteration : matId.Count - 1;
				if (Rhino.RhinoDoc.ActiveDoc.RenderMaterials.Where(i => i.Id.Equals(matId[midx])).FirstOrDefault() is XmlMaterial m)
				{
					m.BeginChange(Rhino.Render.RenderContent.ChangeContexts.Program);
					m.SetParameter("xmlcode", xmlgraph);
					m.EndChange();
					if (matId.Count() > 1)
					{
						Message = "multiple materials set";
					}
					else
					{

						Message = m.Name;
					}
				}
				else
				{
					Message = "NO MATERIAL";
				}
			}

			da.SetData(0, xmlcode);
		}

		private IGH_Param GetSource(int i, IGH_Param p) {
			return i < p.SourceCount ? p.Sources[i] : p.Sources.LastOrDefault();
		}

		private void ColConTags(List<object> acc, object _n, int iteration)
		{
			var n = _n as GH_Component;
			if (n is null) return;

			var dd = (from inp in n.Params.Input let s = GetSource(iteration, inp) where !(s is null) select s);

			var numsls = (from inp in dd where (inp is GH_NumberSlider) select (object)inp);
			var cols = (from inp in dd where (inp is GH_ColourPickerObject) select (object)inp);
			var others = (from inp in dd let attrp = inp.Attributes.Parent as GH_ComponentAttributes where !(attrp is null) select (object)attrp.Owner);
			var clus = (from inp in dd let clattr =inp.Attributes.Parent as GH_ClusterAttributes where !(clattr is null) select clattr.Owner as GH_Cluster);
			foreach(var cls in clus)
			{
				var pl = cls.ProtectionLevel;
				if(pl == GH_ClusterProtection.Unprotected)
				{
					var cldoc = cls.Document("");
					var clouts = (from outp in cls.Params.Output select outp);
					var clauth = cldoc.AttributeCount;
				}
				var plstr = pl.ToString();
			}

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
	}
}

using ShaderGraphResources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderGraphComponents.BSDF
{
	public class DiffuseBsdf : CyclesNode
	{
		public DiffuseBsdf()
			: base("Diffuse BSDF", "diffuse",
    "Diffuse BSDF node for shader graph",
    "Shader", "BSDF",
    typeof(ccl.ShaderNodes.DiffuseBsdfNode))
			{
			}
		public override Guid ComponentGuid => new Guid("e79bd4ac-1aa0-450d-aa4a-495cfeb8cb13");
		protected override Bitmap Icon => Icons.Diffuse;
	}
}

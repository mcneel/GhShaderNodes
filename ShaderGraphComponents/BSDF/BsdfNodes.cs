using GH_IO.Serialization;
using Grasshopper.Kernel;
using ShaderGraphResources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShaderGraphComponents.BSDF
{

	// base class for BSDFs with Distribution in component menu, and saving and reading thereof.
	public class WithDistribution : CyclesNode
	{
		public WithDistribution(string name, string nickname, string description, string category, string subcategory, Type nodetype) : base(name, nickname, description, category, subcategory, nodetype) { }
		public Distribution Distribution { get; set; }
		public void appendMenu(Distribution it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				Utils.DistributionToStringR(it),
				((_, __) => { u.Distribution = it; u.ExpireSolution(true); }),
				true, u.Distribution == it);
		}
		public override bool Write(GH_IWriter writer)
		{
			writer.SetString("Distribution", Distribution.ToString());
			return base.Write(writer);
		}


		public override bool Read(GH_IO.Serialization.GH_IReader reader)
		{
			if (reader.ItemExists("Distribution"))
			{
				var dist = reader.GetString("Distribution");
				Distribution = Utils.DistributionFromString(dist);
			}
			return base.Read(reader);
		}
	}
	public class BlendNode : CyclesNode
	{
		public BlendNode() : base(
			"Blend", "blend",
			"Blend two BSDF nodes",
			"Shader", "Operation",
			typeof(ccl.ShaderNodes.MixClosureNode))
		{
		}

		public override Guid ComponentGuid => new Guid("133f2f95-926f-4ab4-bc8b-5f96e106d3e4");

		protected override Bitmap Icon => Icons.Blend;
	}
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

	public class BackgroundNode : CyclesNode
	{
		public BackgroundNode() : base(
		"Background", "background",
			"Background two BSDF nodes",
			"Shader", "Operation",
			typeof(ccl.ShaderNodes.BackgroundNode))
		{ }

		public override Guid ComponentGuid => new Guid("dd68810b-0a0e-4c54-b08e-f46b41e79f32");

		protected override Bitmap Icon => Icons.Output;
	}

	public class AddClosureNode : CyclesNode
	{
		public AddClosureNode() : base(
		"Add", "add",
			"Add two BSDF nodes",
			"Shader", "Operation",
			typeof(ccl.ShaderNodes.AddClosureNode))
		{ }

		public override Guid ComponentGuid => new Guid("f7929217-6fbb-4bd5-b74f-9763816dc38c");

		protected override Bitmap Icon => Icons.Add;
	}

	public class TransparentBsdf : CyclesNode
	{
		public TransparentBsdf() : base(
		"Transparent BSDF", "transparent",
			"Transparent BSDF node for shader graph",
			"Shader", "BSDF",
			typeof(ccl.ShaderNodes.TransparentBsdfNode))
		{ }

		public override Guid ComponentGuid => new Guid("15f77ebf-ae59-4c49-80b1-362a7168f85f");

		protected override Bitmap Icon => Icons.Diffuse;
	}

	public class VelvetBsdf : CyclesNode
	{
		public VelvetBsdf() : base(
		"Velvet BSDF", "velvet",
			"Velvet BSDF node for shader graph",
			"Shader", "BSDF",
			typeof(ccl.ShaderNodes.VelvetBsdfNode))
		{ }

		public override Guid ComponentGuid => new Guid("d85aeb1d-e42f-43b6-86d6-ddf9cae5a633");

		protected override Bitmap Icon => Icons.Diffuse;
	}

	public class AnisotropicBsdf : WithDistribution
	{
		public AnisotropicBsdf() : base(
		"Anisotropic BSDF", "anisotropic",
			"Anisotropic BSDF node for shader graph",
			"Shader", "BSDF",
			typeof(ccl.ShaderNodes.AnisotropicBsdfNode))
		{
			Distribution = Distribution.Multiscatter_GGX;
		}

		public override Guid ComponentGuid => new Guid("bab1082a-4c74-4d07-9c63-d3f40a178c6a");

		protected override Bitmap Icon => Icons.Glossy;
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			base.SolveInstance(DA);
			Message = Utils.DistributionToStringR(Distribution);
		}

		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			appendMenu(Distribution.Beckmann, menu);
			appendMenu(Distribution.GGX, menu);
			appendMenu(Distribution.Multiscatter_GGX, menu);
			appendMenu(Distribution.Asihkmin_Shirley, menu);
		}
	}

	public class RefractionBsdf : WithDistribution
	{
		public RefractionBsdf() : base(
		"Refraction BSDF", "refraction",
			"Refraction BSDF node for shader graph",
			"Shader", "BSDF",
			typeof(ccl.ShaderNodes.RefractionBsdfNode))
		{
			Distribution = Distribution.GGX;
		}

		public override Guid ComponentGuid => new Guid("e32dffe3-e31a-45a6-9d13-4fb0eefe4ff5");
		protected override Bitmap Icon => Icons.Glossy;

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			var sn = ShaderNode as ccl.ShaderNodes.RefractionBsdfNode;

			switch (Distribution)
			{
				case Distribution.Sharp:
					sn.Distribution = ccl.ShaderNodes.RefractionBsdfNode.RefractionDistribution.Sharp;
					break;
				case Distribution.Beckmann:
					sn.Distribution = ccl.ShaderNodes.RefractionBsdfNode.RefractionDistribution.Beckmann;
					break;
				default:
					sn.Distribution = ccl.ShaderNodes.RefractionBsdfNode.RefractionDistribution.GGX;
					break;
			}

			base.SolveInstance(DA);
			Message = Distribution.ToString();
		}
		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			appendMenu(Distribution.Sharp, menu);
			appendMenu(Distribution.Beckmann, menu);
			appendMenu(Distribution.GGX, menu);
		}
	}

	public class GlassBsdf : WithDistribution
	{
		public GlassBsdf() : base(
		"Glass BSDF", "glass",
			"Glass BSDF node for shader graph",
			"Shader", "BSDF",
			typeof(ccl.ShaderNodes.GlassBsdfNode))
		{
			Distribution = Distribution.Multiscatter_GGX;
		}

		public override Guid ComponentGuid => new Guid("4db00f7b-fa70-4130-813d-a9f7cd193795");

		protected override Bitmap Icon => Icons.Glossy;
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			if (ShaderNode is ccl.ShaderNodes.GlassBsdfNode sn)
			{
				switch (Distribution)
				{
					case Distribution.Sharp:
						sn.Distribution = ccl.ShaderNodes.GlassBsdfNode.GlassDistribution.Sharp;
						break;
					case Distribution.Beckmann:
						sn.Distribution = ccl.ShaderNodes.GlassBsdfNode.GlassDistribution.Beckmann;
						break;
					case Distribution.GGX:
						sn.Distribution = ccl.ShaderNodes.GlassBsdfNode.GlassDistribution.GGX;
						break;
					default:
						sn.Distribution = ccl.ShaderNodes.GlassBsdfNode.GlassDistribution.Multiscatter_GGX;
						break;
				}
			}

			base.SolveInstance(DA);
			Message = Utils.DistributionToStringR(Distribution);
		}
		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			appendMenu(Distribution.Sharp, menu);
			appendMenu(Distribution.Beckmann, menu);
			appendMenu(Distribution.Multiscatter_GGX, menu);
			appendMenu(Distribution.GGX, menu);
		}
	}

	public class GlossyBsdf : WithDistribution
	{
		public GlossyBsdf() : base(
		"Glossy BSDF", "glossy",
			"Glossy BSDF node for shader graph",
			"Shader", "BSDF",
			typeof(ccl.ShaderNodes.GlossyBsdfNode))
		{
			Distribution = Distribution.Multiscatter_GGX;
		}

		public override Guid ComponentGuid => new Guid("84e014b7-a76a-4b4f-8d37-25696cbebc04");

		protected override Bitmap Icon => Icons.Glossy;
		protected override void SolveInstance(IGH_DataAccess DA)
		{

			if (ShaderNode is ccl.ShaderNodes.GlossyBsdfNode sn)
			{
				switch (Distribution)
				{
					case Distribution.Sharp:
						sn.Distribution = ccl.ShaderNodes.GlossyBsdfNode.GlossyDistribution.Sharp;
						break;
					case Distribution.Beckmann:
						sn.Distribution = ccl.ShaderNodes.GlossyBsdfNode.GlossyDistribution.Beckmann;
						break;
					case Distribution.GGX:
						sn.Distribution = ccl.ShaderNodes.GlossyBsdfNode.GlossyDistribution.GGX;
						break;
					case Distribution.Multiscatter_GGX:
						sn.Distribution = ccl.ShaderNodes.GlossyBsdfNode.GlossyDistribution.Multiscatter_GGX;
						break;
					case Distribution.Asihkmin_Shirley:
						sn.Distribution = ccl.ShaderNodes.GlossyBsdfNode.GlossyDistribution.Asihkmin_Shirley;
						break;
				}
			}
			base.SolveInstance(DA);
			Message = Utils.DistributionToStringR(Distribution);
		}

		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			appendMenu(Distribution.Sharp, menu);
			appendMenu(Distribution.Beckmann, menu);
			appendMenu(Distribution.GGX, menu);
			appendMenu(Distribution.Multiscatter_GGX, menu);
			appendMenu(Distribution.Asihkmin_Shirley, menu);
		}
	}

	public class EmissionBsdf : CyclesNode
	{
		public EmissionBsdf() : base(
		"Emission BSDF", "emission",
		"Emission BSDF node for shader graph",
		"Shader", "BSDF",
		typeof(ccl.ShaderNodes.EmissionNode))
		{ }
		public override Guid ComponentGuid => new Guid("aa365407-8e36-4400-b1a7-46cde5b21de6");
		protected override Bitmap Icon => Icons.Emission;
	}

	public class SubsurfaceScatteringBsdf : CyclesNode
	{
		public SubsurfaceScatteringBsdf() : base(
		"Subsurface Scattering BSDF", "subsurface scattering",
				"Subsurface Scattering BSDF node for shader graph",
				"Shader", "BSDF",
				typeof(ccl.ShaderNodes.SubsurfaceScatteringNode))
		{ }

		//member val Falloff = Cubic with get, set
		public override Guid ComponentGuid => new Guid("8b3abb10-4593-4f34-b96f-cb4ed0f64ae7");

		protected override Bitmap Icon => Icons.Emission;
	}


	public class PrincipledBsdf : WithDistribution
	{
		public PrincipledBsdf() : base("Principled BSDF", "principled",
				"Principled BSDF node for shader graph", "Shader", "BSDF", typeof(ccl.ShaderNodes.PrincipledBsdfNode))
		{ Distribution = Distribution.GGX; }


		public override Guid ComponentGuid => new Guid("1480e1aa-7ad4-42e3-b626-62af011917a9");

		protected override Bitmap Icon => Icons.Emission;
		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			appendMenu(Distribution.GGX, menu);
			appendMenu(Distribution.Multiscatter_GGX, menu);
		}
	}
}

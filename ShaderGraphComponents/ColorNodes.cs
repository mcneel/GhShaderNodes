using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using ShaderGraphResources;

namespace ShaderGraphComponents
{
	public class WireframeNode : CyclesNode
	{
		public WireframeNode() : base(
	"Wireframe",
	"Wireframe",
	"Wireframe shader",
	"Shader", "Input",
	typeof(ccl.ShaderNodes.WireframeNode))
		{ }

		bool UsePixelSize { get; set; } = false;

		public override Guid ComponentGuid => new Guid("2aeb616a-649f-4fc9-afcd-c9f2087a5955");
		protected override Bitmap Icon => Icons.Blend;

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			base.SolveInstance(DA);
			Message = "UsePixelSize = " + (UsePixelSize ? "true" : "false");
		}

		public override bool Write(GH_IWriter writer)
		{
			writer.SetBoolean("UsePixelSize", UsePixelSize);
			return base.Write(writer);
		}

		public override bool Read(GH_IReader reader)
		{
			if (reader.ItemExists("UsePixelSize"))
			{
				UsePixelSize = reader.GetBoolean("UsePixelSize");
			}
			return base.Read(reader);
		}

		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			Menu_AppendItem(menu, "UsePixelSize", (_, __) => { UsePixelSize = !UsePixelSize; ExpireSolution(true); }, true, UsePixelSize);
		}
	}
	public class MixRgbNode : CyclesNode
	{
		public MixRgbNode() : base(
		"Mix", "mix",
			"Mix two color nodes, using specified blend type",
			"Shader", "Color",
			typeof(ccl.ShaderNodes.MixNode))
		{ }

		ccl.ShaderNodes.MixNode.BlendTypes Blend { get; set; } = ccl.ShaderNodes.MixNode.BlendTypes.Blend;

		public override Guid ComponentGuid => new Guid("c3a397a6-f760-4ea1-9700-5722eee58489");
		protected override Bitmap Icon => Icons.Blend;

		public void appendMenu(ccl.ShaderNodes.MixNode.BlendTypes it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				it.ToString().Replace("_", "-"),
				((_, __) =>
				{
					u.Blend = it; u.ExpireSolution(true);
				}),
				true, u.Blend == it);
		}


		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Blend, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Add, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Multiply, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Screen, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Overlay, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Subtract, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Divide, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Difference, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Darken, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Lighten, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Dodge, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Burn, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Hue, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Saturation, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Value, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Color, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Soft_Light, menu);
			appendMenu(ccl.ShaderNodes.MixNode.BlendTypes.Linear_Light, menu);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			base.SolveInstance(DA);
			if (ShaderNode is ccl.ShaderNodes.MixNode mn) mn.BlendType = Blend;
			Message = Blend.ToString().Replace("_", " ");
		}


		public override bool Write(GH_IWriter writer)
		{
			writer.SetString("Blend", Blend.ToString());
			return base.Write(writer);
		}

		public override bool Read(GH_IReader reader)
		{
			if (reader.ItemExists("Blend"))
			{
				if (ShaderNode is ccl.ShaderNodes.MixNode mn)
				{
					if (Enum.TryParse(reader.GetString("Blend"), out ccl.ShaderNodes.MixNode.BlendTypes bt))
					{
						mn.BlendType = bt;
					}
					else
					{
						mn.BlendType = ccl.ShaderNodes.MixNode.BlendTypes.Blend;
					}

				}
			}
			return base.Read(reader);
		}
	}

	public class ColorRampNode : CyclesNode
	{
		public ColorRampNode() : base(
		"Color Ramp", "color ramp",
			"Convert a float to a color according a gradient specification (RGB only)",
			"Shader", "Color", typeof(ccl.ShaderNodes.ColorRampNode))
		{ }


		int cpidx = 1;
		int spidx = 2;

		ccl.ShaderNodes.ColorBand.Interpolations Interpolation { get; set; } = ccl.ShaderNodes.ColorBand.Interpolations.Ease;

		public override Guid ComponentGuid => new Guid("dc8abb5a-5a92-4148-8118-b397929d7bb3");

		protected override Bitmap Icon => Icons.Blend;

		protected override void RegisterInputParams(GH_InputParamManager mgr)
		{
			base.RegisterInputParams(mgr);
			cpidx = mgr.AddColourParameter("Stop Colours", "SC", "List of colours", GH_ParamAccess.list);
			spidx = mgr.AddNumberParameter("Stop Positions", "SP", "List of stop positions", GH_ParamAccess.list);
		}

		public override bool Write(GH_IWriter writer)
		{
			writer.SetString("Interpolation", Interpolation.ToString());
			return base.Write(writer);
		}

		public override bool Read(GH_IReader reader)
		{
			if (reader.ItemExists("Interpolation"))
			{
				if (Enum.TryParse(reader.GetString("Interpolation"), out ccl.ShaderNodes.ColorBand.Interpolations ip))
				{
					Interpolation = ip;
				}
				else
				{
					Interpolation = ccl.ShaderNodes.ColorBand.Interpolations.Linear;
				}
			}
			return base.Read(reader);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			base.SolveInstance(DA);
			if (ShaderNode is ccl.ShaderNodes.ColorRampNode crn)
			{

				crn.ColorBand.Interpolation = Interpolation;
				crn.ins.Fac.Value = Utils.readFloat(this, DA, 0, "Couldn't read Fac");
				crn.ColorBand.Stops.Clear();
				if (Params.Input[cpidx].VolatileDataCount == Params.Input[spidx].VolatileDataCount)
				{
					var cp = Params.Input[cpidx].VolatileData.get_Branch(0);
					var sp = Params.Input[spidx].VolatileData.get_Branch(0);
					for (int i = 0; i < cp.Count; i++)
					{
						var c = cp[i] as GH_Colour;
						var s = sp[i] as GH_Number;
						var ncs = new ccl.ShaderNodes.ColorStop();
						ncs.Color = Utils.float4FromColor(c.Value);
						ncs.Position = (float)s.Value;
						crn.ColorBand.Stops.Add(ncs);
					}
				}
			}
		}
		public void appendMenu(ccl.ShaderNodes.ColorBand.Interpolations it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				it.ToString().Replace("_", "-"),
				((_, __) =>
				{
					u.Interpolation = it; u.ExpireSolution(true);
				}),
				true, u.Interpolation == it);
		}

		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			appendMenu(ccl.ShaderNodes.ColorBand.Interpolations.Linear, menu);
			appendMenu(ccl.ShaderNodes.ColorBand.Interpolations.Ease, menu);
			appendMenu(ccl.ShaderNodes.ColorBand.Interpolations.Constant, menu);
		}
	}
}

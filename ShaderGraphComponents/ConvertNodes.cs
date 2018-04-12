using ShaderGraphResources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderGraphComponents
{
	public class CombineXyzNode : CyclesNode
	{
		public CombineXyzNode() : base(
			"Combine XYZ", "combine xyz",
			"Combine three float values into a vector",
			"Shader", "Converter",
			typeof(ccl.ShaderNodes.CombineXyzNode))
		{ }
		public override Guid ComponentGuid => new Guid("a2db4f23-2152-4584-b584-bdb3c16b0f33");
		protected override Bitmap Icon => Icons.CombineXYZ;
	}

	public class CombineRgbNode : CyclesNode
	{
		public CombineRgbNode() : base(
			"Combine RGB", "combine RGB",
			"Combine three float values into an RGB color",
			"Shader", "Converter",
			typeof(ccl.ShaderNodes.CombineRgbNode))
		{ }
		public override Guid ComponentGuid => new Guid("6bc90415-6f69-47f8-8e2e-b89f6390c273");
		protected override Bitmap Icon => Icons.CombineRGB;
	}

	public class CombineHsvNode : CyclesNode
	{
		public CombineHsvNode() : base(
			"Combine HSV", "combine HSV",
			"Combine three float values into an HSV color",
			"Shader", "Converter",
			typeof(ccl.ShaderNodes.CombineHsvNode))
		{ }
		public override Guid ComponentGuid => new Guid("e229219e-9a14-48c4-a36d-75152b81471a");
		protected override Bitmap Icon => Icons.CombineHSV;
	}

	public class ColorToLuminanceNode : CyclesNode
	{
		public ColorToLuminanceNode() : base(
			"Color2Luminance",
			"color -> luminance",
			"Convert input color to luminance value", "Shader", "Converter", typeof(ccl.ShaderNodes.RgbToLuminanceNode))
		{ }

		public override Guid ComponentGuid => new Guid("677f0004-2f9a-48da-897e-1deae4552b4f");
		protected override Bitmap Icon => Icons.ColorToLuminance;
	}

	public class ColorToBwNode : CyclesNode
	{
		public ColorToBwNode() : base(
			"Color2Bw",
			"color -> bw",
			"Convert input color to gray scale value", "Shader", "Converter", typeof(ccl.ShaderNodes.RgbToBwNode))
		{ }

		public override Guid ComponentGuid => new Guid("fb96081c-3353-4215-b182-ec548d96d0fb");
		protected override Bitmap Icon => Icons.ColorToBw;
	}

	public class SeparateRgbNode : CyclesNode
	{
		public SeparateRgbNode() : base(
			"Separate RGB",
			"Separate RGB",
			"Separate color into its constituent RGB values", "Shader", "Converter", typeof(ccl.ShaderNodes.SeparateRgbNode))
		{ }

		public override Guid ComponentGuid => new Guid("ac4dadca-1474-4261-9cfb-927a47bd1a4e");
		protected override Bitmap Icon => Icons.SeparateRGB;
	}

	public class SeparateXyzNode : CyclesNode
	{
		public SeparateXyzNode() : base(
			"Separate XYZ",
			"Separate XYZ",
			"Separate color into its constituent XYZ values", "Shader", "Converter", typeof(ccl.ShaderNodes.SeparateXyzNode))
		{ }

		public override Guid ComponentGuid => new Guid("0b3116da-5dce-47a9-a457-5ac6891b5322");
		protected override Bitmap Icon => Icons.SeparateXYZ;
	}

	public class SeparateHsvNode : CyclesNode
	{
		public SeparateHsvNode() : base(
			"Separate HSV",
			"Separate HSV",
			"Separate color into its constituent HSV values", "Shader", "Converter", typeof(ccl.ShaderNodes.SeparateHsvNode))
		{ }

		public override Guid ComponentGuid => new Guid("b3eafc03-89d4-4797-bf0d-132ec2d43567");
		protected override Bitmap Icon => Icons.SeparateHSV;
	}
}

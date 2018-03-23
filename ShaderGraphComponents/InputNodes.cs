using Grasshopper.Kernel;
using ShaderGraphResources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderGraphComponents
{
	public class TextureCoordinate : CyclesNode
	{
		public TextureCoordinate() : base(
			"Texture Coordinate", "texcoord",
			"Texture Coordinate for point being sampled",
			"Shader", "Input",
			typeof(ccl.ShaderNodes.TextureCoordinateNode))
		{ }
		public override Guid ComponentGuid => new Guid("d78aa03c-713b-43b8-a478-7edfe75cf148");
		protected override Bitmap Icon => Icons.TC;

	}

	public class Geometry : CyclesNode
	{
		public Geometry() : base(
			"Geometry", "geom",
			"Geometry info for the point being sampled",
			"Shader", "Input",
			typeof(ccl.ShaderNodes.GeometryInfoNode))
		{ }
		public override Guid ComponentGuid => new Guid("1268d35e-8912-45c1-9642-0b29ec4f1ff9");
		protected override Bitmap Icon => Icons.TC;

	}

	public class LayerWeightNode : CyclesNode
	{
		public LayerWeightNode() : base(
		 "Layer Weight", "layer weight",
		 "Layer weight",
		 "Shader", "Input",
		 typeof(ccl.ShaderNodes.LayerWeightNode))
		{ }
		public override Guid ComponentGuid => new Guid("5576ff9f-99f7-4611-aa42-dcc4b6c621ac");
		protected override Bitmap Icon => Icons.Emission;

	}

	public class LightPathNode : CyclesNode
	{
		public LightPathNode() : base(
			"Light Path", "light path",
			"Light Path",
			"Shader", "Input",
			typeof(ccl.ShaderNodes.LightPathNode))
		{ }
		public override Guid ComponentGuid => new Guid("9ba94ea6-d977-47ba-807c-b4b68fa9dea8");
		protected override Bitmap Icon => Icons.Emission;

	}

	public class LightFalloffNode : CyclesNode
	{
		public LightFalloffNode() : base(
			"Light Falloff", "light falloff",
			"Light Falloff",
			"Shader", "Input",
			typeof(ccl.ShaderNodes.LightFalloffNode))
		{ }
		public override Guid ComponentGuid => new Guid("5232b3d2-80ac-44f2-bc9e-b6c066d2c6ac");
		protected override Bitmap Icon => Icons.Emission;

	}

	public class FresnelNode : CyclesNode
	{
		public FresnelNode() : base(
			"Fresnel", "fresnel",
			"Fresnel",
			"Shader", "Input",
			typeof(ccl.ShaderNodes.FresnelNode))
		{ }
		public override Guid ComponentGuid => new Guid("b9cca29d-2c77-42cd-a99d-70eb880c02ac");
		protected override Bitmap Icon => Icons.Emission;

	}

	public class CameraDataNode : CyclesNode
	{
		public CameraDataNode() : base(
			"Camera Data", "camera info",
			"Camera Data",
			"Shader", "Input",
			typeof(ccl.ShaderNodes.CameraDataNode))
		{ }
		public override Guid ComponentGuid => new Guid("8b3ea49e-6d18-4f8d-ab41-65bd0cfd94b9");
		protected override Bitmap Icon => Icons.Emission;

	}

	public class AttributeNode : CyclesNode
	{
		public AttributeNode() : base(
			"VertexColor Attribute", "vertexcolor",
			"VertexColor Attribute",
			"Shader", "Input",
			typeof(ccl.ShaderNodes.AttributeNode))
		{ }
		public override Guid ComponentGuid => new Guid("1afc9157-59fe-48e0-8506-c5bcf2f1a375");
		protected override Bitmap Icon => Icons.TC;

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			if (ShaderNode is ccl.ShaderNodes.AttributeNode mn)
			{
				mn.Attribute = "vertexcolor";
			}
			base.SolveInstance(DA);
		}
	}
}

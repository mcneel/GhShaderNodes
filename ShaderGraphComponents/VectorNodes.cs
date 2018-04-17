using GH_IO.Serialization;
using Grasshopper.Kernel;
using ShaderGraphComponents;
using ShaderGraphResources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShaderGraphComponents
{
	public class VectorMathBaseNode : CyclesNode
	{
		public VectorMathBaseNode(Type optype, ccl.ShaderNodes.VectorMathNode.Operations openum, string category, string subcategory, string outputdescription): base(
		openum.ToString().Replace('_', ' '),
		openum.ToString().Replace('_', ' '),
		openum.ToString().Replace('_', ' '),
		category, subcategory,
		optype)
		{
			opEnum = openum;
			OutputDesc = outputdescription;
		}

		private ccl.ShaderNodes.VectorMathNode.Operations opEnum { get; set; }

		public string OutputDesc { get; private set; }

		public bool UseClamp { get; set; }

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			base.SolveInstance(DA);

			var v1 = Utils.readVector(this, DA, 0, "Couldn't read Value1");
			var v2 = Utils.readVector(this, DA, 1, "Couldn't read Value2");

			if (ShaderNode is ccl.ShaderNodes.VectorMathNode mn)
			{
				mn.Operation = opEnum;
			}

			DA.SetData(0, v1);
			DA.SetData(1, v2);
		}
	}

	public class BumpNode : CyclesNode
	{
		public BumpNode() : base(
		"Bump", "bump",
			"Bump node",
			"Shader", "Vector",
			typeof(ccl.ShaderNodes.BumpNode))
		{ }
		public override Guid ComponentGuid => new Guid("02f39fc2-757a-4ec0-91f6-1822ca48663d");
		protected override Bitmap Icon => Icons.Bump;
	}
	public class VectorMathAdd : VectorMathBaseNode {
		public VectorMathAdd() : base(typeof(ccl.ShaderNodes.VectorAdd), ccl.ShaderNodes.VectorMathNode.Operations.Add, "Shader", "Vector", "Vector1+Vector2") { }
		public override Guid ComponentGuid => new Guid("297632e9-77ec-47b3-9726-78f855b17796");
		protected override Bitmap Icon => Icons.VectorAdd;
	}

	public class VectorMathSubtract : VectorMathBaseNode {
		public VectorMathSubtract() : base(typeof(ccl.ShaderNodes.VectorSubtract), ccl.ShaderNodes.VectorMathNode.Operations.Subtract, "Shader", "Vector", "Vector1-Vector2") { }
		public override Guid ComponentGuid => new Guid("473800e8-a584-4238-a9c4-d5474481a424");
		protected override Bitmap Icon => Icons.VectorSubtract;
	}

	public class VectorMathAverage : VectorMathBaseNode {
		public VectorMathAverage() : base(typeof(ccl.ShaderNodes.VectorAverage), ccl.ShaderNodes.VectorMathNode.Operations.Average, "Shader", "Vector", "NormLen(Vector1+Vector2)") { }
		public override Guid ComponentGuid => new Guid("000d564f-85c7-409f-b4fc-ae0c1152a19e");
		protected override Bitmap Icon => Icons.VectorAverage;
	}

	public class VectorMathDot : VectorMathBaseNode {
		public VectorMathDot() : base(typeof(ccl.ShaderNodes.VectorDot_Product), ccl.ShaderNodes.VectorMathNode.Operations.Dot_Product, "Shader", "Vector", "Fac = Dot(Vector1,Vector2)") { }
		public override Guid ComponentGuid => new Guid("f63a2c72-5f84-4e58-a80c-73a5fb72e145");
		protected override Bitmap Icon => Icons.VectorDot;
	}

	public class VectorMathCross : VectorMathBaseNode {
		public VectorMathCross() : base(typeof(ccl.ShaderNodes.VectorCross_Product), ccl.ShaderNodes.VectorMathNode.Operations.Cross_Product, "Shader", "Vector", "NormLen(Cross(Vector1,Vector2))") { }
		public override Guid ComponentGuid => new Guid("35c5f912-4d8b-4a37-ad5b-1e0384a45d7e");
		protected override Bitmap Icon => Icons.VectorCross;
	}

	public class VectorMathNormalize : VectorMathBaseNode {
		public VectorMathNormalize() : base(typeof(ccl.ShaderNodes.VectorNormalize), ccl.ShaderNodes.VectorMathNode.Operations.Normalize, "Shader", "Vector", "NormLen(Vector1)") { }
		public override Guid ComponentGuid => new Guid("b0972558-a3d6-441f-8b8a-4f557872ad9f");
		protected override Bitmap Icon => Icons.VectorNormalize;
	}

	public class MappingNode : CyclesNode
	{
		public MappingNode() : base("Mapping node", "mapping",
			"Mapping node node for shader graph", "Shader", "Vector", typeof(ccl.ShaderNodes.MappingNode))
		{ }
		MappingType Mapping { get; set; } = MappingType.Texture;

		public override Guid ComponentGuid => new Guid("12411bcd-1de4-43a9-8431-89fc1980b58b");
		protected override Bitmap Icon => Icons.Mapping;

		public void appendMenu(MappingType it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				it.ToString(),
				((_, __) => { Mapping = it; u.ExpireSolution(true); }),
				true, Mapping == it);
		}
		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			appendMenu(MappingType.Point, menu);
			appendMenu(MappingType.Texture, menu);
			appendMenu(MappingType.Vector, menu);
			appendMenu(MappingType.Normal, menu);
		}
		public override bool Write(GH_IWriter writer)
		{
			writer.SetString("Mapping", Mapping.ToString());
			return base.Write(writer);
		}


		public override bool Read(GH_IReader reader)
		{
			if (reader.ItemExists("Mapping"))
			{
				var dist = reader.GetString("Mapping");
				Mapping = Utils.MappingFromString(dist);
			}
			return base.Read(reader);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			base.SolveInstance(DA);
			if(ShaderNode is ccl.ShaderNodes.MappingNode mn)
			{
				switch (Mapping)
				{
					case MappingType.Normal:
						mn.Mapping = ccl.ShaderNodes.TextureNode.MappingType.Normal;
						break;
					case MappingType.Point:
						mn.Mapping = ccl.ShaderNodes.TextureNode.MappingType.Point;
						break;
					case MappingType.Texture:
						mn.Mapping = ccl.ShaderNodes.TextureNode.MappingType.Texture;
						break;
					case MappingType.Vector:
						mn.Mapping = ccl.ShaderNodes.TextureNode.MappingType.Vector;
						break;
				}
			}
		}
	}
}

using ccl.ShaderNodes;
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

namespace ShaderGraphComponents
{

	public class MathBaseNode : CyclesNode {
		public MathBaseNode(Type mathop, MathNode.Operations openum, string category, string subcategory, string outputdescription, Func<float, float, float> op) : base(
			openum.ToString().Replace('_', ' '),
			openum.ToString().Replace('_', ' '),
			openum.ToString().Replace('_', ' '),
			category, subcategory,
			mathop)
		{ opEnum = openum;
			Op = op;
		}

		MathNode.Operations opEnum { get; set; }
		bool UseClamp { get; set; } = false;

		Func<float, float, float> Op;

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			if (ShaderNode is MathNode mn)
			{
				mn.Operation = opEnum;
				mn.UseClamp = UseClamp;
				base.SolveInstance(DA);

				var v1 = Utils.readFloat(this, DA, 0, "Couldn't read Value1");
				var v2 = Utils.readFloat(this, DA, 1, "Couldn't read Value2");

				var r = (UseClamp ? Math.Max(Math.Min(Op(v1, v2), 1.0), 0.0) : Op(v1, v2));
				DA.SetData(0, r);
			}
		}

		public override bool Write(GH_IWriter writer)
		{
			writer.SetBoolean("UseClamp", UseClamp);
			return base.Write(writer);
		}

		public override bool Read(GH_IReader reader)
		{
			if (reader.ItemExists("UseClamp"))
			{
				UseClamp = reader.GetBoolean("UseClamp");
			}
			return base.Read(reader);
		}

		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			Menu_AppendItem(menu, "Use Clamp", (_, __) => UseClamp = !UseClamp, true, UseClamp);
		}

	}

	public class MathAdd : MathBaseNode {
		public MathAdd() : base (typeof(ccl.ShaderNodes.MathAdd), MathNode.Operations.Add, "Shader", "Math", "Value1+Value2", (a,b)=>a+b) {}
  public override Guid ComponentGuid => new Guid("ec3b4eb3-7cd5-43c8-8ef7-deb2200df882");
	
	protected override Bitmap Icon => Icons.Add;
	}

	public class MathSubtract : MathBaseNode {
		public MathSubtract() : base(typeof(ccl.ShaderNodes.MathSubtract), MathNode.Operations.Subtract, "Shader", "Math", "Value1-Value2", (a, b) => a - b) { }
		public override Guid ComponentGuid => new Guid("c2b99ede-3050-483d-ab90-35a1548d2d22");
		protected override Bitmap Icon => Icons.Sub;
	}

	public class MathMultiply : MathBaseNode {
		public MathMultiply() : base(typeof(ccl.ShaderNodes.MathMultiply), MathNode.Operations.Multiply, "Shader", "Math", "Value1×Value2", (a, b) => a * b) { }
		public override Guid ComponentGuid => new Guid("4a360292-b84b-4808-ad8e-67f2b77b0e15");
		protected override Bitmap Icon => Icons.Mult;
	}

	public class MathDivide : MathBaseNode {
		public MathDivide() : base(typeof(ccl.ShaderNodes.MathDivide), MathNode.Operations.Divide, "Shader", "Math", "Value1/Value2", (a, b) => a / b) { }
		public override Guid ComponentGuid => new Guid("623ee461-9576-4981-a85a-7aa4a30e2e98");
		protected override Bitmap Icon => Icons.Div;
	}

	public class MathPower : MathBaseNode {
		public MathPower() : base(typeof(ccl.ShaderNodes.MathPower), MathNode.Operations.Power, "Shader", "Math", "Value1**Value2", (a, b) => (float)Math.Pow(a, b)) { }
		public override Guid ComponentGuid => new Guid("2e74876b-33f9-4262-9791-cf53466a63e3");
		protected override Bitmap Icon => Icons.Pow;
	}

	public class MathLogarithm : MathBaseNode {
		public MathLogarithm() : base(typeof(ccl.ShaderNodes.MathLogarithm), MathNode.Operations.Logarithm, "Shader", "Math", "log(Value1)/log(Value2)", (a, b) => (float)(Math.Log(a) / Math.Log(b))) { }
		public override Guid ComponentGuid => new Guid("72b96bf7-350e-4408-bdec-d61ea6b4d677");
		protected override Bitmap Icon => Icons.Log;
	}

	public class MathMinimum : MathBaseNode {
		public MathMinimum() : base(typeof(ccl.ShaderNodes.MathMinimum), MathNode.Operations.Minimum, "Shader", "Math", "min(Value1,Value2)", (a, b) => Math.Min(a, b)) { }
		public override Guid ComponentGuid => new Guid("825f2014-0e51-4d60-bb2f-8aa40aea7b91");
		protected override Bitmap Icon => Icons.Min;
	}

	public class MathMaximum : MathBaseNode {
		public MathMaximum() : base(typeof(ccl.ShaderNodes.MathMaximum), MathNode.Operations.Maximum, "Shader", "Math", "max(Value1,Value2)", (a, b) => Math.Max(a, b)) { }
		public override Guid ComponentGuid => new Guid("1706489e-c7a3-453d-971a-1429a51aa783");
		protected override Bitmap Icon => Icons.Max;
	}

	public class MathLessThan : MathBaseNode {
		public MathLessThan() : base(typeof(ccl.ShaderNodes.MathLess_Than), MathNode.Operations.Less_Than, "Shader", "Math", "Value1 < Value2", (a, b) => (a < b ? 1.0f : 0.0f)) { }
		public override Guid ComponentGuid => new Guid("94d49a6f-2520-430d-b2ee-6eda864b568b");
		protected override Bitmap Icon => Icons.LessThan;
	}

	public class MathGreaterThan : MathBaseNode {
		public MathGreaterThan() : base(typeof(ccl.ShaderNodes.MathGreater_Than), MathNode.Operations.Greater_Than, "Shader", "Math", "Value1 ) Value2", (a, b) => (a > b ? 1.0f : 0.0f)) { }
		public override Guid ComponentGuid => new Guid("7a833ba4-28f2-4676-8fa0-c746a9dd6b02");
		protected override Bitmap Icon => Icons.GreaterThan;
	}

	public class MathSine : MathBaseNode {
		public MathSine() : base(typeof(ccl.ShaderNodes.MathSine), MathNode.Operations.Sine, "Shader", "Math", "Sine(Value1)", (a, b) => (float)Math.Sin(a)) { }
		public override Guid ComponentGuid => new Guid("63468f25-b71e-4285-91d0-e0505118f738");
		protected override Bitmap Icon => Icons.Sin;
	}

	public class MathCosine : MathBaseNode {
		public MathCosine() : base(typeof(ccl.ShaderNodes.MathCosine), MathNode.Operations.Cosine, "Shader", "Math", "Cosine(Value1)", (a, b) => (float)Math.Cos(a)) { }
		public override Guid ComponentGuid => new Guid("a8db0fa4-0bd3-405f-a8ae-1416dd4de632");
		protected override Bitmap Icon => Icons.Cos;
	}

	public class MathTangent : MathBaseNode {
		public MathTangent() : base(typeof(ccl.ShaderNodes.MathTangent), MathNode.Operations.Tangent, "Shader", "Math", "Tangent(Value1)", (a, b) => (float)Math.Tan(a)) { }
		public override Guid ComponentGuid => new Guid("e3e72aad-acf4-4965-a5d7-c1c6c3701faf");
		protected override Bitmap Icon => Icons.Tan;
	}

	public class MathArcsine : MathBaseNode {
		public MathArcsine() : base(typeof(ccl.ShaderNodes.MathArcsine), MathNode.Operations.Arcsine, "Shader", "Math", "Arcsine(Value1)", (a, b) => (float)Math.Asin(a)) { }
		public override Guid ComponentGuid => new Guid("475a3c6c-b047-461e-ba07-ce96c32a5d94");
		protected override Bitmap Icon => Icons.Arcsin;
	}

	public class MathArccosine : MathBaseNode {
		public MathArccosine() : base(typeof(ccl.ShaderNodes.MathArccosine), MathNode.Operations.Arccosine, "Shader", "Math", "Arccosine(Value1)", (a, b) => (float)Math.Acos(a)) { }
		public override Guid ComponentGuid => new Guid("07440de9-5786-4265-a328-1acb45188157");
		protected override Bitmap Icon => Icons.Arccos;
	}

	public class MathArctangent : MathBaseNode {
		public MathArctangent() : base(typeof(ccl.ShaderNodes.MathArctangent), MathNode.Operations.Arctangent, "Shader", "Math", "Arctangent(Value1)", (a, b) => (float)Math.Atan(a)) { }
		public override Guid ComponentGuid => new Guid("a01cd7ab-a8aa-427a-bcaf-99e2410f620c");
		protected override Bitmap Icon => Icons.Arctan;
	}

	public class MathRound : MathBaseNode {
		public MathRound() : base(typeof(ccl.ShaderNodes.MathRound), MathNode.Operations.Round, "Shader", "Math", "Round(Value1)", (a, b) => (float)Math.Round(a)) { }
		public override Guid ComponentGuid => new Guid("08e29380-e762-4411-88e0-3d9a8b3523cb");
		protected override Bitmap Icon => Icons.Round;
	}

	public class MathModulo : MathBaseNode {
		public MathModulo() : base(typeof(ccl.ShaderNodes.MathModulo), MathNode.Operations.Modulo, "Shader", "Math", "Modulo(Value1, Value2)", (a, b) => a % b) { }
		public override Guid ComponentGuid => new Guid("589244da-f040-41d8-85d6-13f478a6a327");
		protected override Bitmap Icon => Icons.Modulo;
	}

	public class MathAbsolute : MathBaseNode
	{
		public MathAbsolute() : base(typeof(ccl.ShaderNodes.MathAbsolute), MathNode.Operations.Absolute, "Shader", "Math", "Absolute(Value1)", (a,b)=>Math.Abs(a)) { }
		public override Guid ComponentGuid => new Guid("0789f738-a1d9-4b5c-8b2b-e9b38256dfec");
		protected override Bitmap Icon => Icons.Abs;
	}
}

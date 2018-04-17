using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;
using ShaderGraphResources;

namespace ShaderGraphComponents
{
	public class Info : GH_AssemblyInfo
	{
		public Info()
		{
		}

		public override string Name => "Shader Nodes";
		public override string Description => "Create shader graphs for Cycles for Rhino";
		public override Guid Id => new Guid("6a051e83-3727-465e-b5ef-74d027a6f73b");
		public override string AuthorName => "Nathan 'jesterKing' Letwory";
		public override string AuthorContact => "nathan@mcneel.com";
		public override Bitmap Icon => Icons.ShaderGraph;
		public override string AssemblyVersion => "0.1.9";
	}
}

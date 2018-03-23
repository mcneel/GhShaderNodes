using Grasshopper;
using Grasshopper.Kernel;

namespace ShaderGraphComponents
{
	public class Priority : GH_AssemblyPriority
	{
		public override GH_LoadingInstruction PriorityLoad()
		{
			return GH_LoadingInstruction.Proceed;
		}
	}
}

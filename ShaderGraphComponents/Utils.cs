using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ccl;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ShaderGraphComponents
{
	static public class Utils
	{
		static public string cleanName(string nn)
		{
			return nn.Replace(" ", "_").Replace("-", "_").Replace(">", "").ToLowerInvariant();
		}

		static public float RGBChanToFloat(byte b) => b / 255.0f;
		static public float4 float4FromColor(Color ic) => new float4(RGBChanToFloat(ic.R), RGBChanToFloat(ic.G), RGBChanToFloat(ic.B), 1.0f);
		static public float4 float4FromVector(Vector3d vec) => new ccl.float4((float)vec.X, (float)vec.Y, (float)vec.Z, 1.0f);

		static public Color readColor(GH_Component u, IGH_DataAccess da, int idx, string msg)
		{
			var c = new GH_Colour();
			var r = da.GetData(idx, ref c);
			u.Message = !r ? msg : "";
			return c.Value;
		}
		static public Vector3d readVector(GH_Component u, IGH_DataAccess da, int idx, string msg)
		{
			var c = new GH_Vector();
			var r = da.GetData(idx, ref c);
			u.Message = !r ? msg : "";
			return c.Value;
		}
		static public float readFloat(GH_Component u, IGH_DataAccess da, int idx, string msg)
		{
			var c = new GH_Number();
			var r = da.GetData(idx, ref c);
			u.Message = !r ? msg : "";
			return (float)c.Value;
		}
		static public int readInt(GH_Component u, IGH_DataAccess da, int idx, string msg)
		{
			var c = new GH_Number();
			var r = da.GetData(idx, ref c);
			u.Message = !r ? msg : "";
			return (int)c.Value;
		}
		static public string readString(GH_Component u, IGH_DataAccess da, int idx, string msg)
		{
			var c = new GH_String();
			var r = da.GetData(idx, ref c);
			u.Message = !r ? msg : "";
			return c.Value;
		}
		public static string DistributionToStringR(Distribution d) => d.ToString().Replace("_", "-");
		// default GGX
		public static Distribution DistributionFromString(string d)
		{
			if (Enum.TryParse(d, out Distribution r)) { return r; }
			return Distribution.GGX;
		}

		public static MappingType MappingFromString(string m)
		{
			if (Enum.TryParse(m, out MappingType r)) { return r; }
			return MappingType.Texture;
		}
	}

	public enum Distribution
	{
		Sharp,
		Beckmann,
		GGX,
		Asihkmin_Shirley,
		Multiscatter_GGX
	};

	public enum MappingType
	{
		Point,
		Texture,
		Vector,
		Normal
	};
}

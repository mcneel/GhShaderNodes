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
		public static string GradientToStringR(GradientTypes u) => u.ToString().Replace("_", " ");
		public static GradientTypes GradientFromString(string g)
		{
			if (Enum.TryParse(g, out GradientTypes r)) { return r; }
			return GradientTypes.Easing;

		}
		public static string MusgraveToStringR(MusgraveTypes u) => u.ToString().Replace("FB", "fB").Replace("_", " ");
		public static MusgraveTypes MusgraveFromString(string g)
		{
			if (Enum.TryParse(g, out MusgraveTypes r)) { return r; }
			return MusgraveTypes.FBM;

		}
		public static Interpolation InterpolationFromString(string g)
		{
			if (Enum.TryParse(g, out Interpolation r)) { return r; }
			return Interpolation.None;

		}
		public static string EnvironmentProjectionToStringR(EnvironmentProjection d) => d.ToString().Replace("_", " ");
		// default GGX
		public static EnvironmentProjection EnvironmentProjectionFromString(string d)
		{
			if (Enum.TryParse(d, out EnvironmentProjection r)) { return r; }
			return EnvironmentProjection.Equirectangular;
		}
		public static string TextureProjectionToStringR(TextureProjection d) => d.ToString().Replace("_", " ");
		// default GGX
		public static TextureProjection TextureProjectionFromString(string d)
		{
			if (Enum.TryParse(d, out TextureProjection r)) { return r; }
			return TextureProjection.Flat;
		}
		public static string TextureExtensionToStringR(TextureExtension d) => d.ToString().Replace("_", " ");
		// default GGX
		public static TextureExtension TextureExtensionFromString(string d)
		{
			if (Enum.TryParse(d, out TextureExtension r)) { return r; }
			return TextureExtension.Repeat;
		}
		public static string ColorSpaceToStringR(ColorSpace d) => d.ToString().Replace("_", " ");
		// default GGX
		public static ColorSpace ColorSpaceFromString(string d)
		{
			if (Enum.TryParse(d, out ColorSpace r)) { return r; }
			return ColorSpace.None;
		}
		public static string WaveTypesToStringR(WaveTypes d) => d.ToString().Replace("_", " ");
		// default GGX
		public static WaveTypes WaveTypesFromString(string d)
		{
			if (Enum.TryParse(d, out WaveTypes r)) { return r; }
			return WaveTypes.Bands;
		}
		public static string WaveProfilesToStringR(WaveProfiles d) => d.ToString().Replace("_", " ");
		// default GGX
		public static WaveProfiles WaveProfilesFromString(string d)
		{
			if (Enum.TryParse(d, out WaveProfiles r)) { return r; }
			return WaveProfiles.Sine;
		}
	}

	public enum Interpolation {
		None,
		Linear,
		Closest,
		Cubic,
		Smart
	};

	public enum EnvironmentProjection
	{
		Equirectangular,
		Mirror_Ball,
		Wallpaper,
		Use_TexCo
	};

	public enum TextureProjection
	{
		Flat,
		Box,
		Sphere,
		Tube
	};

	public enum TextureExtension
	{
		Repeat,
		Extend,
		Clip
	};

	public enum ColorSpace
	{
		None,
		Color
	};

	public enum MusgraveTypes {
  Multifractal,
  FBM,
  Hybrid_Multifractal,
  Ridged_Multifractal,
  Hetero_Terrain
	};


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
	public enum GradientTypes {
  Linear,
  Quadratic,
  Easing,
  Diagonal,
  Radial,
  Quadratic_Sphere,
  Spherical
	};
	public enum WaveTypes {
  Bands,
	Rings
	};
	public enum WaveProfiles {
		Sine,
		Saw
	};

}

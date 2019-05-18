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
			IGH_QuickCast c = null;
			var r = da.GetData(idx, ref c);
			u.Message = !r ? msg : "";
			return c?.QC_Col() ?? Color.Gray;
		}
		static public Vector3d readVector(GH_Component u, IGH_DataAccess da, int idx, string msg)
		{
			IGH_QuickCast c = null;
			var r = da.GetData(idx, ref c);
			u.Message = !r ? msg : "";
			return c?.QC_Vec() ?? Vector3d.Zero;
		}
		static public float readFloat(GH_Component u, IGH_DataAccess da, int idx, string msg)
		{
			IGH_QuickCast c = null;
			var r = da.GetData(idx, ref c);
			u.Message = !r ? msg : "";
			return (float)(c?.QC_Num() ?? 0.0);
		}
		static public int readInt(GH_Component u, IGH_DataAccess da, int idx, string msg)
		{
			IGH_QuickCast c = null;
			var r = da.GetData(idx, ref c);
			u.Message = !r ? msg : "";
			return c?.QC_Int() ?? 0;
		}
		static public string readString(GH_Component u, IGH_DataAccess da, int idx, string msg)
		{
			IGH_QuickCast c = null;
			var r = da.GetData(idx, ref c);
			u.Message = !r ? msg : "";
			return c?.QC_Text() ?? "";
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
		public static string GradientToStringR(ccl.ShaderNodes.GradientTextureNode.GradientType u) => u.ToString().Replace("_", " ");
		public static ccl.ShaderNodes.GradientTextureNode.GradientType GradientFromString(string g)
		{
			if (Enum.TryParse(g, out ccl.ShaderNodes.GradientTextureNode.GradientType r)) { return r; }
			return ccl.ShaderNodes.GradientTextureNode.GradientType.Easing;

		}
		public static string MusgraveToStringR(ccl.ShaderNodes.MusgraveTexture.MusgraveTypes u) => u.ToString().Replace("FB", "fB").Replace("_", " ");
		public static ccl.ShaderNodes.MusgraveTexture.MusgraveTypes MusgraveFromString(string g)
		{
			if (Enum.TryParse(g, out ccl.ShaderNodes.MusgraveTexture.MusgraveTypes r)) { return r; }
			return ccl.ShaderNodes.MusgraveTexture.MusgraveTypes.fBM;

		}
		public static ccl.ShaderNodes.VoronoiTexture.ColoringTypes VoronoiColoringFromString(string g)
		{
			if(Enum.TryParse(g, out ccl.ShaderNodes.VoronoiTexture.ColoringTypes r)) { return r; }
			return ccl.ShaderNodes.VoronoiTexture.ColoringTypes.Intensity;
		}
		public static ccl.ShaderNodes.VoronoiTexture.Metrics VoronoiMetricFromString(string g)
		{
			if(Enum.TryParse(g, out ccl.ShaderNodes.VoronoiTexture.Metrics r)) { return r; }
			return ccl.ShaderNodes.VoronoiTexture.Metrics.Distance;
		}
		public static ccl.ShaderNodes.VoronoiTexture.Features VoronoiFeatureFromString(string g)
		{
			if(Enum.TryParse(g, out ccl.ShaderNodes.VoronoiTexture.Features r)) { return r; }
			return ccl.ShaderNodes.VoronoiTexture.Features.F1;
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
		public static string WaveTypesToStringR(ccl.ShaderNodes.WaveTexture.WaveTypes d) => d.ToString().Replace("_", " ");
		// default GGX
		public static ccl.ShaderNodes.WaveTexture.WaveTypes WaveTypesFromString(string d)
		{
			if (Enum.TryParse(d, out ccl.ShaderNodes.WaveTexture.WaveTypes r)) { return r; }
			return ccl.ShaderNodes.WaveTexture.WaveTypes.Bands;
		}
		public static string WaveProfilesToStringR(ccl.ShaderNodes.WaveTexture.WaveProfiles d) => d.ToString().Replace("_", " ");
		// default GGX
		public static ccl.ShaderNodes.WaveTexture.WaveProfiles WaveProfilesFromString(string d)
		{
			if (Enum.TryParse(d, out ccl.ShaderNodes.WaveTexture.WaveProfiles r)) { return r; }
			return ccl.ShaderNodes.WaveTexture.WaveProfiles.Sine;
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

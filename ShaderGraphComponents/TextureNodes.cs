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
	public class NoiseTextureNode : CyclesNode
	{
		public NoiseTextureNode() : base("Noise", "noise", "Noise", "Shader", "Texture", typeof(ccl.ShaderNodes.NoiseTexture)) { }
		public override Guid ComponentGuid => new Guid("c3632808-8f29-48bd-afc2-0b85ad5763c0");
		protected override Bitmap Icon => Icons.Emission;
	}

	public class GradientTextureNode : CyclesNode
	{
		public GradientTextureNode() : base("Gradient", "gradient", "Gradient", "Shader", "Texture", typeof(ccl.ShaderNodes.GradientTextureNode)) { }

		public GradientTypes Gradient { get; set; } = GradientTypes.Easing;
		public override Guid ComponentGuid => new Guid("e9d63595-4a09-4351-93f4-acd2a0248a9b");
		protected override Bitmap Icon => Icons.Emission;
		public void appendMenu(GradientTypes it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				Utils.GradientToStringR(it),
				((_, __) => { u.Gradient = it; u.ExpireSolution(true); }),
				true, u.Gradient == it);
		}
		public override bool Write(GH_IWriter writer)
		{
			writer.SetString("Gradient", Gradient.ToString());
			return base.Write(writer);
		}


		public override bool Read(GH_IReader reader)
		{
			if (reader.ItemExists("Gradient"))
			{
				var dist = reader.GetString("Gradient");
				Gradient = Utils.GradientFromString(dist);
			}
			return base.Read(reader);
		}


		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			appendMenu(GradientTypes.Linear, menu);
			appendMenu(GradientTypes.Easing, menu);
			appendMenu(GradientTypes.Quadratic, menu);
			appendMenu(GradientTypes.Diagonal, menu);
			appendMenu(GradientTypes.Radial, menu);
			appendMenu(GradientTypes.Quadratic_Sphere, menu);
			appendMenu(GradientTypes.Spherical, menu);
		}
	}

	public class MusgraveTextureNode : CyclesNode
	{
		public MusgraveTextureNode() : base("Musgrave", "musgrave", "Musgrave", "Shader", "Texture", typeof(ccl.ShaderNodes.MusgraveTexture)) { }
		MusgraveTypes Musgrave { get; set; } = MusgraveTypes.FBM;
		public override Guid ComponentGuid => new Guid("3ed2f77b-373c-4eb8-b6fb-253dff125065");
		protected override Bitmap Icon => Icons.Emission;
		public void appendMenu(MusgraveTypes it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				Utils.MusgraveToStringR(it),
				((_, __) =>
				{
					u.Musgrave = it; u.ExpireSolution(true);
				}),
				true, u.Musgrave == it);
		}
		public override bool Write(GH_IWriter writer)
		{
			writer.SetString("Musgrave", Musgrave.ToString());
			return base.Write(writer);
		}

		public override bool Read(GH_IReader reader)
		{
			if (reader.ItemExists("Musgrave"))
			{
				var dist = reader.GetString("Musgrave");
				Musgrave = Utils.MusgraveFromString(dist);
			}
			return base.Read(reader);
		}

		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			appendMenu(MusgraveTypes.Multifractal, menu);
			appendMenu(MusgraveTypes.FBM, menu);
			appendMenu(MusgraveTypes.Hybrid_Multifractal, menu);
			appendMenu(MusgraveTypes.Ridged_Multifractal, menu);
			appendMenu(MusgraveTypes.Hetero_Terrain, menu);
		}
	}

	public class ImageTextureNode : CyclesNode
	{
		public ImageTextureNode() : base("Image", "image", "Image", "Shader", "Texture", typeof(ccl.ShaderNodes.ImageTextureNode)) { }

		public string ImageFile { get; set; } = "";
		public Interpolation Interpolation { get; set; } = Interpolation.Linear;
		public TextureProjection Projection { get; set; } = TextureProjection.Flat;
		public TextureExtension TextureExtension { get; set; } = TextureExtension.Repeat;
		public ColorSpace ColorSpace { get; set; } = ColorSpace.None;

		public override Guid ComponentGuid => new Guid("078f4865-e362-4ed1-818d-94fd9432ac77");

		protected override Bitmap Icon => Icons.Emission;

		public override bool Write(GH_IWriter writer)
		{
			writer.SetString("Image", ImageFile);
			writer.SetString("ImTexInterpolation", Interpolation.ToString());
			writer.SetString("ImTexColorSpace", ColorSpace.ToString());
			writer.SetString("ImTexTextureExtension", TextureExtension.ToString());
			writer.SetString("ImTexProjection", Projection.ToString());

			return base.Write(writer);
		}


		public override bool Read(GH_IReader reader)
		{
			if (reader.ItemExists("Image"))
			{
				ImageFile = reader.GetString("Image");
			}
			if (reader.ItemExists("ImTexInterpolation"))
			{
				var interp = Utils.InterpolationFromString(reader.GetString("ImTexInterpolation"));
				Interpolation = interp;
			}

			if (reader.ItemExists("ImTexColorSpace"))
			{
				var colorspace = Utils.ColorSpaceFromString(reader.GetString("ImTexColorSpace"));

				ColorSpace = colorspace;
			}

			if (reader.ItemExists("ImTexTextureExtension"))
			{
				var extension = Utils.TextureExtensionFromString(reader.GetString("ImTexTextureExtension"));
				TextureExtension = extension;
			}
			if (reader.ItemExists("ImTexProjection"))
			{
				var projection = Utils.TextureProjectionFromString(reader.GetString("ImTexProjection"));
				Projection = projection;
			}
			return base.Read(reader);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			base.SolveInstance(DA);

			Message = System.IO.Path.GetFileName(ImageFile);
			if (ShaderNode is ccl.ShaderNodes.ImageTextureNode im)
			{
				im.Filename = ImageFile;

				switch (TextureExtension)
				{
					case TextureExtension.Clip:
						im.Extension = ccl.ShaderNodes.TextureNode.TextureExtension.Clip;
						break;
					case TextureExtension.Repeat:
						im.Extension = ccl.ShaderNodes.TextureNode.TextureExtension.Repeat;
						break;
					case TextureExtension.Extend:
						im.Extension = ccl.ShaderNodes.TextureNode.TextureExtension.Extend;
						break;
				}

				switch (ColorSpace)
				{
					case ColorSpace.Color:
						im.ColorSpace = ccl.ShaderNodes.TextureNode.TextureColorSpace.Color;
						break;
					case ColorSpace.None:
						im.ColorSpace = ccl.ShaderNodes.TextureNode.TextureColorSpace.None;
						break;
				}

				switch (Interpolation)
				{
					case Interpolation.Closest:
						im.Interpolation = ccl.InterpolationType.Closest;
						break;
					case Interpolation.Cubic:
						im.Interpolation = ccl.InterpolationType.Cubic;
						break;
					case Interpolation.Linear:
						im.Interpolation = ccl.InterpolationType.Linear;
						break;
					case Interpolation.None:
						im.Interpolation = ccl.InterpolationType.None;
						break;
					case Interpolation.Smart:
						im.Interpolation = ccl.InterpolationType.Smart;
						break;
				}

				switch (Projection)
				{
					case TextureProjection.Box:
						im.Projection = ccl.ShaderNodes.TextureNode.TextureProjection.Box;
						break;
					case TextureProjection.Flat:
						im.Projection = ccl.ShaderNodes.TextureNode.TextureProjection.Flat;
						break;
					case TextureProjection.Tube:
						im.Projection = ccl.ShaderNodes.TextureNode.TextureProjection.Tube;
						break;
					case TextureProjection.Sphere:
						im.Projection = ccl.ShaderNodes.TextureNode.TextureProjection.Sphere;
						break;
				}
			}
		}
		public void appendInterpolationMenu(Interpolation it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				it.ToString(),
				((_, __) =>
				{
					u.Interpolation = it; u.ExpireSolution(true);
				}),
				true, u.Interpolation == it);
		}
		public void appendTextureExtensionMenu(TextureExtension it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				Utils.TextureExtensionToStringR(it),
				((_, __) =>
				{
					u.TextureExtension = it; u.ExpireSolution(true);
				}),
				true, u.TextureExtension == it);
		}
		public void appendTextureProjectionMenu(TextureProjection it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				Utils.TextureProjectionToStringR(it),
				((_, __) =>
				{
					u.Projection = it; u.ExpireSolution(true);
				}),
				true, u.Projection == it);
		}
		public void appendColorSpaceMenu(ColorSpace it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				Utils.ColorSpaceToStringR(it),
				((_, __) =>
				{
					u.ColorSpace = it; u.ExpireSolution(true);
				}),
				true, u.ColorSpace == it);
		}
		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			Menu_AppendItem(menu, "Select Image File...", (_, __) =>
			{
				var fdi = new OpenFileDialog
				{
					Filter = "Image Files(*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|All files (*.*)|*.*"
				};
				var res = fdi.ShowDialog();
				if (res == DialogResult.OK)
				{
					ImageFile = fdi.FileName;
					ExpireSolution(true);
				}
			});

			Menu_AppendSeparator(menu);
			appendInterpolationMenu(Interpolation.None, menu);
			appendInterpolationMenu(Interpolation.Linear, menu);
			appendInterpolationMenu(Interpolation.Closest, menu);
			appendInterpolationMenu(Interpolation.Cubic, menu);
			appendInterpolationMenu(Interpolation.Smart, menu);
			Menu_AppendSeparator(menu);
			appendTextureExtensionMenu(TextureExtension.Repeat, menu);
			appendTextureExtensionMenu(TextureExtension.Extend, menu);
			appendTextureExtensionMenu(TextureExtension.Clip, menu);
			Menu_AppendSeparator(menu);
			appendColorSpaceMenu(ColorSpace.None, menu);
			appendColorSpaceMenu(ColorSpace.Color, menu);
			Menu_AppendSeparator(menu);
			appendTextureProjectionMenu(TextureProjection.Flat, menu);
			appendTextureProjectionMenu(TextureProjection.Box, menu);
			appendTextureProjectionMenu(TextureProjection.Sphere, menu);
			appendTextureProjectionMenu(TextureProjection.Tube, menu);
		}
	}

	public class EnvironmentTextureNode : CyclesNode
	{
		public EnvironmentTextureNode() : base(
			"Environment", "environment",
			"Environment",
			"Shader", "Texture",
			typeof(ccl.ShaderNodes.EnvironmentTextureNode))
		{ }

		public string EnvironmentFile { get; set; } = "";
		public EnvironmentProjection Projection { get; set; } = EnvironmentProjection.Equirectangular;
		public Interpolation Interpolation { get; set; } = Interpolation.Linear;
		public ColorSpace ColorSpace { get; set; } = ColorSpace.None;

		public override Guid ComponentGuid => new Guid("07e09721-e982-4ea4-8358-1dfc4d26cda4");

		protected override Bitmap Icon => Icons.Emission;
		public override bool Write(GH_IWriter writer)
		{
			writer.SetString("Environment", EnvironmentFile);
			writer.SetString("Projection", Projection.ToString());
			writer.SetString("Interpolation", Interpolation.ToString());
			writer.SetString("ColorSpace", ColorSpace.ToString());

			return base.Write(writer);
		}


		public override bool Read(GH_IReader reader)
		{
			if (reader.ItemExists("Environment"))
			{
				EnvironmentFile = reader.GetString("Environment");
			}
			if (reader.ItemExists("Projection"))
			{
				var projection = Utils.EnvironmentProjectionFromString(reader.GetString("Projection"));
				Projection = projection;
			}
			if (reader.ItemExists("Interpolation"))
			{
				var interp = Utils.InterpolationFromString(reader.GetString("Interpolation"));
				Interpolation = interp;
			}

			if (reader.ItemExists("ColorSpace"))
			{
				var colorspace = Utils.ColorSpaceFromString(reader.GetString("ColorSpace"));

				ColorSpace = colorspace;
			}

			return base.Read(reader);
		}
		public void appendEnvironmentProjectionMenu(EnvironmentProjection it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				Utils.EnvironmentProjectionToStringR(it),
				((_, __) =>
				{
					u.Projection = it; u.ExpireSolution(true);
				}),
				true, u.Projection == it);
		}
		public void appendColorSpaceMenu(ColorSpace it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				Utils.ColorSpaceToStringR(it),
				((_, __) =>
				{
					u.ColorSpace = it; u.ExpireSolution(true);
				}),
				true, u.ColorSpace == it);
		}
		public void appendInterpolationMenu(Interpolation it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				it.ToString(),
				((_, __) =>
				{
					u.Interpolation = it; u.ExpireSolution(true);
				}),
				true, u.Interpolation == it);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			base.SolveInstance(DA);
			Message = System.IO.Path.GetFileName(EnvironmentFile);
			if (ShaderNode is ccl.ShaderNodes.EnvironmentTextureNode mn) mn.Filename = EnvironmentFile;
		}

		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			Menu_AppendItem(menu, "Select Image File...", (_, __) =>
			{
				var fdi = new OpenFileDialog
				{
					Filter = "Image Files(*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|All files (*.*)|*.*"
				};
				var res = fdi.ShowDialog();
				if (res == DialogResult.OK)
				{
					EnvironmentFile = fdi.FileName;
					ExpireSolution(true);
				}
			});
			Menu_AppendSeparator(menu);
			appendEnvironmentProjectionMenu(EnvironmentProjection.Equirectangular, menu);
			appendEnvironmentProjectionMenu(EnvironmentProjection.Mirror_Ball, menu);
			appendEnvironmentProjectionMenu(EnvironmentProjection.Wallpaper, menu);
			appendEnvironmentProjectionMenu(EnvironmentProjection.Use_TexCo, menu);
			Menu_AppendSeparator(menu);
			appendInterpolationMenu(Interpolation.None, menu);
			appendInterpolationMenu(Interpolation.Linear, menu);
			appendInterpolationMenu(Interpolation.Closest, menu);
			appendInterpolationMenu(Interpolation.Cubic, menu);
			appendInterpolationMenu(Interpolation.Smart, menu);
			Menu_AppendSeparator(menu);
			appendColorSpaceMenu(ColorSpace.None, menu);
			appendColorSpaceMenu(ColorSpace.Color, menu);
		}

	}

	public class WaveTextureNode : CyclesNode
	{
		public WaveTextureNode() : base("Wave", "wave", "Wave", "Shader", "Texture", typeof(ccl.ShaderNodes.WaveTexture)) { }

		public WaveTypes Wave { get; set; } = WaveTypes.Bands;
		public WaveProfiles Profile { get; set; } = WaveProfiles.Sine;

		public override Guid ComponentGuid => new Guid("89660e7d-cf92-4fed-b61c-0231edd76504");

		protected override Bitmap Icon => Icons.Emission;
		public override bool Write(GH_IWriter writer)
		{
			writer.SetString("Wave", Wave.ToString());
			writer.SetString("Profile", Profile.ToString());

			return base.Write(writer);
		}
		public override bool Read(GH_IReader reader)
		{
			if (reader.ItemExists("Wave"))
			{
				Wave = Utils.WaveTypesFromString(reader.GetString("Wave"));
			}
			if (reader.ItemExists("Profile"))
			{
				Profile = Utils.WaveProfilesFromString(reader.GetString("Profile"));
			}

			return base.Read(reader);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			base.SolveInstance(DA);
			Message = Wave.ToString();
		}

		public void appendWaveTypesMenu(WaveTypes it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				Utils.WaveTypesToStringR(it),
				((_, __) =>
				{
					u.Wave = it; u.ExpireSolution(true);
				}),
				true, u.Wave == it);
		}
		public void appendWaveProfilesMenu(WaveProfiles it, ToolStripDropDown menu)
		{
			var u = this;
			Menu_AppendItem(
				menu,
				Utils.WaveProfilesToStringR(it),
				((_, __) =>
				{
					u.Profile = it; u.ExpireSolution(true);
				}),
				true, u.Profile == it);
		}

		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			appendWaveTypesMenu(WaveTypes.Bands, menu);
			appendWaveTypesMenu(WaveTypes.Rings, menu);
			Menu_AppendSeparator(menu);
			appendWaveProfilesMenu(WaveProfiles.Sine, menu);
			appendWaveProfilesMenu(WaveProfiles.Saw, menu);
		}
	}
}

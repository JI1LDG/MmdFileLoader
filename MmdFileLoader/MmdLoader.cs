using SlimDX;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MmdFileLoader {
	public class MmdLoader {
		public string Name;
		public string Comment;
		public MmdVertex[] Vertex;
		public int[] Index;
		public MmdMaterial[] Material;

		public MmdLoader(string Path) {
			if(Path.Contains(".pmd")) {
				PmdLoad(Path);
			} else {
				PmxLoad(Path);
			}
		}

		private void PmdLoad(string Path) {
			var pl = new Pmd.PmdLoader(Path);
			Name = pl.Header.ModelName;
			Comment = pl.Header.Comment;
			Vertex = pl.Vertex.Select(x => new MmdVertex() {
				Position = x.Position, Normal = x.Normal, Uv = x.Uv,
			}).ToArray();
			Index = pl.Index.SelectMany(x => x.Indicies).Select(x => (int)x).ToArray();
			Material = pl.Material.Select(x => new MmdMaterial(x, pl.Toon)).ToArray();
		}

		private void PmxLoad(string Path) {
			var pl = new Pmx.PmxLoader(Path);
			Name = pl.ModelInfo.Name;
			Comment = pl.ModelInfo.Comment;
			Vertex = pl.Vertex.Select(x => new MmdVertex() {
				Position = x.Position, Normal = x.Normal, Uv = x.Uv,
			}).ToArray();
			Index = pl.Index;
			Material = pl.Material.Select(x => new MmdMaterial(x, pl.Texture)).ToArray();
		}
	}

	public class MmdVertex {
		public Vector3 Position;
		public Vector3 Normal;
		public Vector2 Uv;
	}

	public class MmdMaterial {
		public string Name;
		public Color3 Diffuse;
		public float Alpha;
		public Color3 Specular;
		public float Specularity;
		public Color3 Ambient;
		public DrawFlagEnumes DrawFlag;
		public string NormalTexture;
		public string AddSphereTexture;
		public string MultiplySphereTexture;
		public string ToonTexture;
		public string Memo;
		public int IndiciesCount;

		public MmdMaterial(Pmx.PmxMaterial Mat, string[] Textures) {
			Name = Mat.Name;
			Diffuse = Mat.Diffuse;
			Alpha = Mat.Alpha;
			Specular = Mat.Specular;
			Specularity = Mat.Specularity;
			Ambient = Mat.Ambient;
			DrawFlag = Mat.DrawFlag;
			if(Mat.NormalTextureIndex >= 0) NormalTexture = Textures[Mat.NormalTextureIndex];
			if(Mat.SphereTextureIndex >= 0) {
				if(Mat.SphereMode == Pmx.PmxMaterial.SphereModeEnumes.Spa) AddSphereTexture = Textures[Mat.SphereTextureIndex];
				if(Mat.SphereMode == Pmx.PmxMaterial.SphereModeEnumes.Sph) MultiplySphereTexture = Textures[Mat.SphereTextureIndex];
			}
			if(Mat.IsReferredTable) {
				if(Mat.ToonTextureIndex >= 0) ToonTexture = Textures[Mat.ToonTextureIndex];
			} else {
				ToonTexture = "toon\\toon" + Mat.ToonTextureIndex.ToString("00") + ".bmp";
			}
			Memo = Mat.Memo;
			IndiciesCount = Mat.IndiciesCount;
		}

		public MmdMaterial(Pmd.PmdMaterial Mat, Pmd.PmdToon[] Ton) {
			Name = "";
			Diffuse = Mat.Diffuse;
			Alpha = Mat.Alpha;
			Specular = Mat.Specular;
			Specularity = Mat.Specularity;
			Ambient = Mat.Mirror;
			if(Mat.EdgeFlag == 1) DrawFlag = DrawFlagEnumes.DrawBoth;
			else DrawFlag = 0;
			Memo = "";
			if(Mat.TextureFileName != null) NormalTexture =Mat.TextureFileName;
			if(Mat.SphereFileName != null) {
				if(Mat.SphereFileName.Contains(".spa")) {
					AddSphereTexture = Mat.SphereFileName;
				} else if(Mat.SphereFileName.Contains(".sph")) {
					MultiplySphereTexture = Mat.SphereFileName;
				}
			}
			if(Mat.ToonIndex > 0 && Mat.ToonIndex <= 10) {
				var toon = Ton[Mat.ToonIndex].FileName;
				if(Regex.IsMatch(toon, @"toon(10|0[1-9]).bmp")) {
					ToonTexture = @"toon\" + toon;
				} else {
					ToonTexture = toon;
				}
			}
			IndiciesCount = Mat.IndiciesCount;
		}
	}
}

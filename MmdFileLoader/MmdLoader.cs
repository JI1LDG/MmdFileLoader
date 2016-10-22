using SlimDX;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace MmdFileLoader {
	public class MmdLoader {
		public string ParentDir { get; private set; }
		public string Name { get; private set; }
		public string Comment { get; private set; }
		public MmdVertex[] Vertex{ get; private set; }
		public int[] Index{ get; private set; }
		public MmdMaterial[] Material{ get; private set; }
		public MmdBone[] Bone{ get; private set; }

		public MmdLoader(string Path) {
			if(Path.Contains(".pmd")) {
				PmdLoad(Path);
			} else {
				PmxLoad(Path);
			}
			ParentDir = System.IO.Path.GetDirectoryName(Environment.CurrentDirectory + "\\" + Path) + "\\";
		}

		private void PmdLoad(string Path) {
			var pl = new Pmd.PmdLoader(Path);
			Name = pl.Header.ModelName;
			Comment = pl.Header.Comment;
			Vertex = pl.Vertex.Select(x => new MmdVertex() {
				Position = x.Position, Normal = x.Normal, Uv = x.Uv, IsEdgeDraw = x.EdgeFlag == 1 ? true : false, EdgeWidth = 1.0f,
				Index = new int[] { (int)x.Bone[0], (int)x.Bone[1], 0, 0 }, Weight = new Vector4(x.Weight / 100.0f, (100 - x.Weight) / 100.0f, 0, 0)
			}).ToArray();
			Index = pl.Index.SelectMany(x => x.Indicies).Select(x => (int)x).ToArray();
			Material = pl.Material.Select(x => new MmdMaterial(x, pl.Toon)).ToArray();
			Bone = pl.Bone.Select((x, index) => new MmdBone(x) { Id = index }).ToArray();
			
		}

		private void PmxLoad(string Path) {
			var pl = new Pmx.PmxLoader(Path);
			Name = pl.ModelInfo.Name;
			Comment = pl.ModelInfo.Comment;
			Vertex = pl.Vertex.Select(x => new MmdVertex() {
				Position = x.Position, Normal = x.Normal, Uv = x.Uv, IsEdgeDraw = true, EdgeWidth = x.EdgeMultiply,
				Index = x.BoneIndex, Weight = x.BoneWeight
			}).ToArray();
			Index = pl.Index;
			Material = pl.Material.Select(x => new MmdMaterial(x, pl.Texture)).ToArray();
			Bone = pl.Bone.Select(x => new MmdBone(x)).ToArray();
		}

		private Vector4 ToVec4(int[] src) {
			return new Vector4(src[0], src[1], src[2], src[3]);
		}
	}

	public class MmdVertex {
		public Vector3 Position;
		public Vector3 Normal;
		public Vector2 Uv;
		public bool IsEdgeDraw;
		public float EdgeWidth;
		public int[] Index;
		public Vector4 Weight;
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

	public class MmdBone {
		public int Id;
		public string Name;
		public Vector3 Position;
		public int ParentIndex;
		public int TailIndex;
		public Vector3 TailOffset; //if TailIndex == -2
		public BoneFlagEnum BoneFlag;
		public int Rank;
		public Vector3 XVector;
		public Vector3 ZVector;
		public int AddedParentIndex;
		public float Addly;

		public MmdBone(Pmd.PmdBone Bone) {
			Name = Bone.Name;
			Position = Bone.HeadPosition;
			ParentIndex = Bone.ParentIndex;
			TailIndex = Bone.TailIndex;
			BoneFlag = Bone.BoneFlag;
			Rank = 0;
		}

		public MmdBone(Pmx.PmxBone Bone) {
			Name = Bone.Name;
			Position = Bone.Position;
			ParentIndex = Bone.ParentIndex;
			TailIndex = Bone.TailIndex;
			TailOffset = Bone.Offset;
			BoneFlag = Bone.BoneFlag;
			Rank = Bone.Rank;
			XVector = Bone.XVector;
			ZVector = Bone.ZVector;
			AddedParentIndex = Bone.AddedParentIndex;
			Addly = Bone.Addly;
		}
	}
}

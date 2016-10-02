using SlimDX;
using System.IO;
using System.Linq;

namespace MmdFileLoader {
	namespace Pmx {
		public class PmxLoader {
			private PmxHeader Header;
			public PmxModelInfo ModelInfo;
			public PmxVertex[] Vertex;
			public int[] Index;
			public string[] Texture;
			public PmxMaterial[] Material;
			public PmxBone[] Bone;
			public PmxMorph[] Morph;

			public PmxLoader(string Path) {
				using(var fs = new FileStream(Path, FileMode.Open))
				using(var bs = new BinaryStream(fs)) {
					Header = new PmxHeader(bs);
					ModelInfo = new PmxModelInfo(bs);
					Vertex = Enumerable.Range(0, bs.ReadInt32()).Select(x => new PmxVertex(bs, Header.AddUvCount, Header.BoneIndexSize)).ToArray();
					Index = bs.ReadIndexes(Header.VertexIndexSize, bs.ReadInt32(), true);
					Texture = Enumerable.Range(0, bs.ReadInt32()).Select(x => bs.ReadTextBuf()).ToArray();
					Material = Enumerable.Range(0, bs.ReadInt32()).Select(x => new PmxMaterial(bs, Header.TextureIndexSize)).ToArray();
					Bone = Enumerable.Range(0, bs.ReadInt32()).Select(x => new PmxBone(bs, Header.BoneIndexSize)).ToArray();
					Morph = Enumerable.Range(0, bs.ReadInt32()).Select(x => new PmxMorph(bs, Header)).ToArray();
				}
			}
		}

		public class PmxHeader {
			public byte AddUvCount;
			public byte VertexIndexSize;
			public byte TextureIndexSize;
			public byte MaterialIndexSize;
			public byte BoneIndexSize;
			public byte MorphIndexSize;
			public byte RigidIndexSize;

			public PmxHeader(BinaryStream bs) {
				bs.ReadBytes(4); bs.ReadSingle(); bs.ReadByte();
				if(bs.ReadByte() == 0) bs.ChangeEncoding("UTF-16");
				else bs.ChangeEncoding("UTF-8");
				AddUvCount = bs.ReadByte();
				VertexIndexSize = bs.ReadByte();
				TextureIndexSize = bs.ReadByte();
				MaterialIndexSize = bs.ReadByte();
				BoneIndexSize = bs.ReadByte();
				MorphIndexSize = bs.ReadByte();
				RigidIndexSize = bs.ReadByte();
			}
		}

		public class PmxModelInfo {
			public string Name;
			public string Comment;

			public PmxModelInfo(BinaryStream bs) {
				Name = bs.ReadTextBuf();
				bs.ReadTextBuf();
				Comment = bs.ReadTextBuf();
				bs.ReadTextBuf();
			}
		}

		public class PmxVertex {
			public Vector3 Position;
			public Vector3 Normal;
			public Vector2 Uv;
			public Vector4[] AddUv;
			public int[] BoneIndex;
			public float[] BoneWeight;
			public Vector3 SdefC;
			public Vector3 SdefR0;
			public Vector3 SdefR1;
			public float EdgeMultiply;

			public PmxVertex(BinaryStream bs, int addUvCount, byte boneIdxSize) {
				Position = bs.Vector3();
				Normal = bs.Vector3();
				Uv = bs.Vector2();
				if(addUvCount > 0) {
					AddUv = new Vector4[addUvCount];
					for(int i = 0;i < addUvCount; i++) {
						AddUv[i] = bs.Vector4();
					}
				}
				switch(bs.ReadByte()) {
					case 0:
						BoneIndex = bs.ReadIndexes(boneIdxSize, 1);
						BoneWeight = new float[1] { 1.0f };
						break;
					case 1:
						BoneIndex = bs.ReadIndexes(boneIdxSize, 2);
						var weight = bs.ReadSingle();
						BoneWeight = new float[2] { weight, 1.0f - weight };
						break;
					case 2:
						BoneIndex = bs.ReadIndexes(boneIdxSize, 4);
						BoneWeight = bs.ReadSingles(4);
						break;
					default:
						BoneIndex = bs.ReadIndexes(boneIdxSize, 2);
						var weight2 = bs.ReadSingle();
						BoneWeight = new float[2] { weight2, 1.0f - weight2 };
						SdefC = bs.Vector3();
						SdefR0 = bs.Vector3();
						SdefR1 = bs.Vector3();
						break;
				}
				EdgeMultiply = bs.ReadSingle();
			}
		}

		public class PmxMaterial {
			public string Name;
			public Color3 Diffuse;
			public float Alpha;
			public Color3 Specular;
			public float Specularity;
			public Color3 Ambient;
			public DrawFlagEnumes DrawFlag;
			public Color4 EdgeColor;
			public float EdgeSize;
			public int NormalTextureIndex;
			public int SphereTextureIndex;
			public SphereModeEnumes SphereMode;
			public int ToonTextureIndex;
			public bool IsReferredTable;
			public string Memo;
			public int IndiciesCount;

			public PmxMaterial(BinaryStream bs, byte textureIdxSize) {
				Name = bs.ReadTextBuf(); bs.ReadTextBuf();
				Diffuse = bs.Color3();
				Alpha = bs.ReadSingle();
				Specular = bs.Color3();
				Specularity = bs.ReadSingle();
				Ambient = bs.Color3();
				DrawFlag = (DrawFlagEnumes)bs.ReadByte();
				EdgeColor = bs.Color4();
				EdgeSize = bs.ReadSingle();
				NormalTextureIndex = bs.ReadIndex(textureIdxSize);
				SphereTextureIndex = bs.ReadIndex(textureIdxSize);
				SphereMode = (SphereModeEnumes)bs.ReadByte();
				if(bs.ReadByte() == 0) {
					IsReferredTable = true;
					ToonTextureIndex = bs.ReadIndex(textureIdxSize);
				} else ToonTextureIndex = bs.ReadByte();
				Memo = bs.ReadTextBuf();
				IndiciesCount = bs.ReadInt32();
			}

			public enum SphereModeEnumes {
				Disable = 0, Sph, Spa, SubTexture
			}
		}

		public class PmxBone {
			public string Name;
			public Vector3 Position;
			public int ParentIndex;
			public int Rank;
			public BoneFlagEnum BoneFlag;
			public Vector3 Offset;
			public int TailIndex;
			public int AddedParentIndex;
			public float Addly;
			public Vector3 AxisVector;
			public Vector3 XVector;
			public Vector3 ZVector;
			public int KeyValue;
			public int IkTargetIndex;
			public int IkLoopCount;
			public float RimitRadian;
			public PmxIk[] Ik;

			public PmxBone(BinaryStream bs, byte boneIdxSize) {
				Name = bs.ReadTextBuf(); bs.ReadTextBuf();
				Position = bs.Vector3();
				ParentIndex = bs.ReadIndex(boneIdxSize);
				Rank = bs.ReadInt32();
				BoneFlag = (BoneFlagEnum)bs.ReadUInt16();
				if(BoneFlag.HasFlag(BoneFlagEnum.AssignIndex)) {
					TailIndex = bs.ReadIndex(boneIdxSize);
				} else {
					Offset = bs.Vector3();
				}
				if(BoneFlag.HasFlag(BoneFlagEnum.AddRotate) || BoneFlag.HasFlag(BoneFlagEnum.AddMove)) {
					AddedParentIndex = bs.ReadIndex(boneIdxSize);
					Addly = bs.ReadSingle();
				}
				if(BoneFlag.HasFlag(BoneFlagEnum.FixAxis)) {
					AxisVector = bs.Vector3();
				}
				if(BoneFlag.HasFlag(BoneFlagEnum.LocalAxis)) {
					XVector = bs.Vector3();
					ZVector = bs.Vector3();
				}
				if(BoneFlag.HasFlag(BoneFlagEnum.TransformOuterParent)) {
					KeyValue = bs.ReadInt32();
				}
				if(BoneFlag.HasFlag(BoneFlagEnum.Ik)) {
					IkTargetIndex = bs.ReadIndex(boneIdxSize);
					IkLoopCount = bs.ReadInt32();
					RimitRadian = bs.ReadSingle();
					Enumerable.Range(0, bs.ReadInt32()).Select(x => new PmxIk(bs, boneIdxSize)).ToArray();
				}
			}

			public enum BoneFlagEnum {
				AssignIndex = 0x01, CanRotate = 0x02, CanMove = 0x04,
				Draw = 0x08, CanControl = 0x10, Ik = 0x20,
				AddRotate = 0x0100, AddMove = 0x0200,
				FixAxis = 0x0400, LocalAxis = 0x0800,
				TransformAfterPhysic = 0x1000, TransformOuterParent = 0x2000
			}

			public class PmxIk {
				public int LinkBoneIndex;
				public Vector3 UnderRimit;
				public Vector3 UpperRimit;

				public PmxIk(BinaryStream bs, byte boneIdxSize) {
					LinkBoneIndex = bs.ReadIndex(boneIdxSize);
					if(bs.ReadByte() == 1) {
						UnderRimit = bs.Vector3();
						UpperRimit = bs.Vector3();
					}
				}
			}
		}

		public class PmxMorph {
			public string Name;
			public MorphKinds Kind;
			public int OffsetCount;
			public PmxVertexMorph[] Vertex;
			public PmxUvMorph[] Uv;
			public PmxBoneMorph[] Bone;
			public PmxMaterialMorph[] Material;
			public PmxGroupMorph[] Group;

			public PmxMorph(BinaryStream bs, PmxHeader header) {
				Name = bs.ReadTextBuf(); bs.ReadTextBuf(); bs.ReadByte();
				Kind = (MorphKinds)bs.ReadByte();
				OffsetCount = bs.ReadInt32();
				switch(Kind) {
					case MorphKinds.Vertex:
						Vertex = Enumerable.Range(0, OffsetCount).Select(x => new PmxVertexMorph(bs, header.VertexIndexSize)).ToArray();
						break;
					case MorphKinds.Uv:
					case MorphKinds.AddUv1:
					case MorphKinds.AddUv2:
					case MorphKinds.AddUv3:
					case MorphKinds.AddUv4:
						Uv = Enumerable.Range(0, OffsetCount).Select(x => new PmxUvMorph(bs, header.VertexIndexSize)).ToArray();
						break;
					case MorphKinds.Bone:
						Bone = Enumerable.Range(0, OffsetCount).Select(x => new PmxBoneMorph(bs, header.BoneIndexSize)).ToArray();
						break;
					case MorphKinds.Material:
						Material = Enumerable.Range(0, OffsetCount).Select(x => new PmxMaterialMorph(bs, header.MaterialIndexSize)).ToArray();
						break;
					case MorphKinds.Group:
						Group = Enumerable.Range(0, OffsetCount).Select(x => new PmxGroupMorph(bs, header.MorphIndexSize)).ToArray();
						break;
				}
			}

			public enum MorphKinds {
				Group = 0, Vertex, Bone, Uv, AddUv1, AddUv2, AddUv3, AddUv4, Material
			}

			public class PmxVertexMorph {
				public int Index;
				public Vector3 Offset;

				public PmxVertexMorph(BinaryStream bs, byte vertexIdxSize) {
					Index = bs.ReadIndex(vertexIdxSize, true);
					Offset = bs.Vector3();
				}
			}

			public class PmxUvMorph {
				public int Index;
				public Vector4 Offset;

				public PmxUvMorph(BinaryStream bs, byte vertexIdxSize) {
					Index = bs.ReadIndex(vertexIdxSize, true);
					Offset = bs.Vector4();
				}
			}

			public class PmxBoneMorph {
				public int Index;
				public Vector3 Move;
				public Quaternion Rotation;

				public PmxBoneMorph(BinaryStream bs, byte boneIdxSize) {
					Index = bs.ReadIndex(boneIdxSize);
					Move = bs.Vector3();
					Rotation = bs.Quaternion();
				}
			}

			public class PmxMaterialMorph {
				public int Index;
				public OffsetEnum Offset;
				public Color3 Diffuse;
				public float Alpha;
				public Color3 Specular;
				public float Specularity;
				public Color3 Ambient;
				public Color4 EdgeColor;
				public float EdgeSize;
				public Vector4 NormalTexturity;
				public Vector4 SphereTexturity;
				public Vector4 ToonTexturity;

				public PmxMaterialMorph(BinaryStream bs, byte materialIdxSize) {
					Index = bs.ReadIndex(materialIdxSize);
					Offset = (OffsetEnum)bs.ReadByte();
					Diffuse = bs.Color3();
					Alpha = bs.ReadSingle();
					Specular = bs.Color3();
					Specularity = bs.ReadSingle();
					Ambient = bs.Color3();
					EdgeColor = bs.Color4();
					EdgeSize = bs.ReadSingle();
					NormalTexturity = bs.Vector4();
					SphereTexturity = bs.Vector4();
					ToonTexturity = bs.Vector4();
				}

				public enum OffsetEnum {
					Multiply, Add
				}
			}

			public class PmxGroupMorph {
				public int Index;
				public float Morphly;

				public PmxGroupMorph(BinaryStream bs, byte morphIdxSize) {
					Index = bs.ReadIndex(morphIdxSize);
					Morphly = bs.ReadSingle();
				}
			}
		}
	}
}

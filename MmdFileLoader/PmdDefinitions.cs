using SlimDX;
using System;
using System.IO;
using System.Text;

namespace MmdFileLoader {
	namespace Pmd {
		public class PmdLoader { //DWORD = 32, WORD = 16
			public PmdHeader Header;
			public PmdVertex[] Vertex;
			public PmdIndex[] Index;
			public PmdMaterial[] Material;
			public PmdBone[] Bone;
			public PmdIk[] Ik;
			public PmdSkin[] Skin;
			public PmdToon[] Toon;
			public PmdRigid[] Rigid;
			public PmdJoint[] Joint;

			public PmdLoader(string Path) {
				using(var fs = new FileStream(Path, FileMode.Open))
				using(var br = new BinaryReader(fs, Encoding.GetEncoding("shift-jis"))) {
					Header = new PmdHeader(br);
					Vertex = new PmdVertex[br.ReadInt32()];
					for(int i = 0; i < Vertex.Length; i++) Vertex[i] = new PmdVertex(br);
					Index = new PmdIndex[br.ReadInt32() / 3];
					for(int i = 0; i < Index.Length; i++) Index[i] = new PmdIndex(br);
					Material = new PmdMaterial[br.ReadInt32()];
					for(int i = 0; i < Material.Length; i++) Material[i] = new PmdMaterial(br);
					Bone = new PmdBone[br.ReadInt16()];
					for(int i = 0; i < Bone.Length; i++) Bone[i] = new PmdBone(br);
					Ik = new PmdIk[br.ReadInt16()];
					for(int i = 0; i < Ik.Length; i++) Ik[i] = new PmdIk(br);
					Skin = new PmdSkin[br.ReadInt16()];
					for(int i = 0; i < Skin.Length; i++) Skin[i] = new PmdSkin(br);
					br.SkipShort(br.ReadByte());
					byte BoneDispCount = br.ReadByte();
					br.Sjis(50 * BoneDispCount);
					br.Sjis(3 * br.ReadInt32());
					br.ReadByte(); br.Sjis(20); br.Sjis(256);
					br.Sjis(20 * Bone.Length);
					br.Sjis(20 * (Skin.Length - 1));
					br.Sjis(50 * BoneDispCount);
					Toon = new PmdToon[10];
					for(int i = 0; i < 10; i++) Toon[i] = new PmdToon(br);
					Rigid = new PmdRigid[br.ReadInt32()];
					for(int i = 0; i < Rigid.Length; i++) Rigid[i] = new PmdRigid(br);
					Joint = new PmdJoint[br.ReadInt32()];
					for(int i = 0; i < Joint.Length; i++) Joint[i] = new PmdJoint(br);
				}
			}
		}

		public class PmdHeader {
			public string ModelName;
			public string Comment;

			public PmdHeader(BinaryReader br) {
				br.Sjis(3); br.ReadSingle();
				ModelName = br.Sjis(20);
				Comment = br.Sjis(256);
				// ReadCharsだと、読み込みすぎる? ReadBytesとGetString使用
			}
		}

		public class PmdVertex {
			public Vector3 Position;
			public Vector3 Normal;
			public Vector2 Uv;
			public short[] Bone;
			public byte Weight;
			public byte EdgeFlag;

			public PmdVertex(BinaryReader br) {
				Position = br.Vector3();
				Normal = br.Vector3();
				Uv = br.Vector2();
				Bone = new short[] { br.ReadInt16(), br.ReadInt16() };
				Weight = br.ReadByte();
				EdgeFlag = br.ReadByte();
			}
		}

		public class PmdIndex {
			public short[] Indicies;

			public PmdIndex(BinaryReader br) {
				Indicies = new short[] { br.ReadInt16(), br.ReadInt16(), br.ReadInt16() };
			}
		}

		public class PmdMaterial {
			public Color3 Diffuse;
			public float Alpha;
			public float Specularity;
			public Color3 Specular;
			public Color3 Mirror;
			public byte ToonIndex;
			public byte EdgeFlag;
			public int IndiciesCount;
			public string TextureFileName;
			public string SphereFileName;

			public PmdMaterial(BinaryReader br) {
				Diffuse = new Color3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
				Alpha = br.ReadSingle();
				Specularity = br.ReadSingle();
				Specular = new Color3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
				Mirror = new Color3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
				ToonIndex = br.ReadByte();
				EdgeFlag = br.ReadByte();
				IndiciesCount = br.ReadInt32();
				var tmp = br.Sjis(20);
				var sp = tmp.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
				foreach(var s in sp) {
					if(s.Contains(".sp")) SphereFileName = s;
					else TextureFileName = s;
				}
			}
		}

		public class PmdBone {
			public string Name;
			public short ParentIndex;
			public short TailIndex;
			public byte Type;
			public short IkIndex;
			public Vector3 HeadPosition;

			public PmdBone(BinaryReader br) {
				Name = br.Sjis(20);
				ParentIndex = br.ReadInt16();
				TailIndex = br.ReadInt16();
				Type = br.ReadByte();
				IkIndex = br.ReadInt16();
				HeadPosition = br.Vector3();
			}
		}

		public class PmdIk {
			public short BoneIndex;
			public short TargetBoneIndex;
			public short Iterations;
			public float ControlWeight;
			public short[] ChildBoneIndex;

			public PmdIk(BinaryReader br) {
				BoneIndex = br.ReadInt16();
				TargetBoneIndex = br.ReadInt16();
				ChildBoneIndex = new short[br.ReadByte()];
				Iterations = br.ReadInt16();
				ControlWeight = br.ReadSingle();
				for(int i = 0; i < ChildBoneIndex.Length; i++) {
					ChildBoneIndex[i] = br.ReadInt16();
				}
			}
		}

		public class PmdSkin {
			public string Name;
			public byte Type;
			public PmdSkinVertex[] SkinData;

			public PmdSkin(BinaryReader br) {
				Name = br.Sjis(20);
				var count = br.ReadInt32();
				Type = br.ReadByte();
				SkinData = new PmdSkinVertex[count];
				for(int i = 0; i < SkinData.Length; i++) SkinData[i] = new PmdSkinVertex(br);
			}
		}

		public class PmdSkinVertex {
			public int VertexIndex;
			public Vector3 Offset;

			public PmdSkinVertex(BinaryReader br) {
				VertexIndex = br.ReadInt32();
				Offset = br.Vector3();
			}
		}

		public class PmdToon {
			public string FileName;

			public PmdToon(BinaryReader br) {
				FileName = br.Sjis(100);
			}
		}

		public class PmdRigid {
			public string Name;
			public short RelationBoneIndex;
			public byte GroupIndex;
			public short GroupTarget;
			public byte ShapeType;
			public float ShapeWidth;
			public float ShapeHeighth;
			public float ShapeDepth;
			public Vector3 Position;
			public Vector3 Rotation;
			public float Weight;
			public float DimPosition;
			public float DimRotation;
			public float Recoil;
			public float Friction;
			public byte RigidType;

			public PmdRigid(BinaryReader br) {
				Name = br.Sjis(20);
				RelationBoneIndex = br.ReadInt16();
				GroupIndex = br.ReadByte();
				GroupTarget = (short)(0xffff - br.ReadInt16());
				ShapeType = br.ReadByte();
				ShapeWidth = br.ReadSingle();
				ShapeHeighth = br.ReadSingle();
				ShapeDepth = br.ReadSingle();
				Position = br.Vector3();
				Rotation = br.Vector3();
				Weight = br.ReadSingle();
				DimPosition = br.ReadSingle();
				DimRotation = br.ReadSingle();
				Recoil = br.ReadSingle();
				Friction = br.ReadSingle();
				RigidType = br.ReadByte();
			}
		}

		public class PmdJoint {
			public string Name;
			public int RigidA;
			public int RigidB;
			public Vector3 Position;
			public Vector3 Rotation;
			public Vector3 Move1;
			public Vector3 Move2;
			public Vector3 Rotate1;
			public Vector3 Rotate2;
			public Vector3 SpringPosition;
			public Vector3 SpringRotation;

			public PmdJoint(BinaryReader br) {
				Name = br.Sjis(20);
				RigidA = br.ReadInt32();
				RigidB = br.ReadInt32();
				Position = br.Vector3();
				Rotation = br.Vector3();
				Move1 = br.Vector3();
				Move2 = br.Vector3();
				Rotate1 = br.Vector3();
				Rotate2 = br.Vector3();
				SpringPosition = br.Vector3();
				SpringRotation = br.Vector3();
			}
		}
	}
}

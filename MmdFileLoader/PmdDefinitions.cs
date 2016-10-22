using SlimDX;
using System;
using System.IO;
using System.Text;

namespace MmdFileLoader {
	namespace Pmd {
		public class PmdLoader { //DWORD = 32, WORD = 16
			private BinaryStream bs;
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
				using(var fs = new FileStream(Path, FileMode.Open)) {
					bs = new BinaryStream(fs);
					Header = new PmdHeader(bs);
					Vertex = new PmdVertex[bs.ReadInt32()];
					for(int i = 0; i < Vertex.Length; i++) Vertex[i] = new PmdVertex(bs);
					Index = new PmdIndex[bs.ReadInt32() / 3];
					for(int i = 0; i < Index.Length; i++) Index[i] = new PmdIndex(bs);
					Material = new PmdMaterial[bs.ReadInt32()];
					for(int i = 0; i < Material.Length; i++) Material[i] = new PmdMaterial(bs);
					Bone = new PmdBone[bs.ReadInt16()];
					for(int i = 0; i < Bone.Length; i++) Bone[i] = new PmdBone(bs);
					Ik = new PmdIk[bs.ReadInt16()];
					for(int i = 0; i < Ik.Length; i++) Ik[i] = new PmdIk(bs);
					Skin = new PmdSkin[bs.ReadInt16()];
					for(int i = 0; i < Skin.Length; i++) Skin[i] = new PmdSkin(bs);
					bs.SkipShort(bs.ReadByte());
					byte BoneDispCount = bs.ReadByte();
					bs.Sjis(50 * BoneDispCount);
					int bondis = bs.ReadInt32();
					for(int i = 0;i < bondis; i++) {
						bs.ReadInt16(); bs.ReadByte();
					}
					if(bs.ReadByte() == (byte)1) {
						bs.Sjis(20); bs.Sjis(256);
						bs.Sjis(20 * Bone.Length);
						bs.Sjis(20 * (Skin.Length - 1));
						bs.Sjis(50 * BoneDispCount);
					}
					Toon = new PmdToon[10];
					for(int i = 0; i < 10; i++) Toon[i] = new PmdToon(bs);
					Rigid = new PmdRigid[bs.ReadInt32()];
					for(int i = 0; i < Rigid.Length; i++) Rigid[i] = new PmdRigid(bs);
					Joint = new PmdJoint[bs.ReadInt32()];
					for(int i = 0; i < Joint.Length; i++) Joint[i] = new PmdJoint(bs);
				}
			}
		}

		public class PmdHeader {
			public string ModelName;
			public string Comment;

			public PmdHeader(BinaryStream bs) {
				bs.Sjis(3); bs.ReadSingle();
				ModelName = bs.Sjis(20);
				Comment = bs.Sjis(256);
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

			public PmdVertex(BinaryStream bs) {
				Position = bs.Vector3();
				Normal = bs.Vector3();
				Uv = bs.Vector2();
				Bone = new short[] { bs.ReadInt16(), bs.ReadInt16() };
				Weight = bs.ReadByte();
				EdgeFlag = bs.ReadByte();
			}
		}

		public class PmdIndex {
			public short[] Indicies;

			public PmdIndex(BinaryStream bs) {
				Indicies = new short[] { bs.ReadInt16(), bs.ReadInt16(), bs.ReadInt16() };
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

			public PmdMaterial(BinaryStream bs) {
				Diffuse = new Color3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
				Alpha = bs.ReadSingle();
				Specularity = bs.ReadSingle();
				Specular = new Color3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
				Mirror = new Color3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
				ToonIndex = bs.ReadByte();
				EdgeFlag = bs.ReadByte();
				IndiciesCount = bs.ReadInt32();
				var tmp = bs.Sjis(20);
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
			public BoneFlagEnum BoneFlag;
			public short IkIndex;
			public Vector3 HeadPosition;

			public PmdBone(BinaryStream bs) {
				Name = bs.Sjis(20);
				ParentIndex = bs.ReadInt16();
				TailIndex = bs.ReadInt16();
				if(TailIndex == 0) TailIndex = -1;
				byte type = bs.ReadByte();

				switch(type) {
					case 0:
						BoneFlag = BoneFlagEnum.CanRotate;
						break;
					case 1:
						BoneFlag = BoneFlagEnum.CanRotate | BoneFlagEnum.CanMove;
						break;
					case 2:
						BoneFlag = BoneFlagEnum.Ik;
						break;
				}

				IkIndex = bs.ReadInt16();
				HeadPosition = bs.Vector3();
			}
		}

		public class PmdIk {
			public short BoneIndex;
			public short TargetBoneIndex;
			public short Iterations;
			public float ControlWeight;
			public short[] ChildBoneIndex;

			public PmdIk(BinaryStream bs) {
				BoneIndex = bs.ReadInt16();
				TargetBoneIndex = bs.ReadInt16();
				ChildBoneIndex = new short[bs.ReadByte()];
				Iterations = bs.ReadInt16();
				ControlWeight = bs.ReadSingle();
				for(int i = 0; i < ChildBoneIndex.Length; i++) {
					ChildBoneIndex[i] = bs.ReadInt16();
				}
			}
		}

		public class PmdSkin {
			public string Name;
			public byte Type;
			public PmdSkinVertex[] SkinData;

			public PmdSkin(BinaryStream bs) {
				Name = bs.Sjis(20);
				var count = bs.ReadInt32();
				Type = bs.ReadByte();
				SkinData = new PmdSkinVertex[count];
				for(int i = 0; i < SkinData.Length; i++) SkinData[i] = new PmdSkinVertex(bs);
			}
		}

		public class PmdSkinVertex {
			public int VertexIndex;
			public Vector3 Offset;

			public PmdSkinVertex(BinaryStream bs) {
				VertexIndex = bs.ReadInt32();
				Offset = bs.Vector3();
			}
		}

		public class PmdToon {
			public string FileName;

			public PmdToon(BinaryStream bs) {
				FileName = bs.Sjis(100);
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

			public PmdRigid(BinaryStream bs) {
				Name = bs.Sjis(20);
				RelationBoneIndex = bs.ReadInt16();
				GroupIndex = bs.ReadByte();
				GroupTarget = (short)(0xffff - bs.ReadInt16());
				ShapeType = bs.ReadByte();
				ShapeWidth = bs.ReadSingle();
				ShapeHeighth = bs.ReadSingle();
				ShapeDepth = bs.ReadSingle();
				Position = bs.Vector3();
				Rotation = bs.Vector3();
				Weight = bs.ReadSingle();
				DimPosition = bs.ReadSingle();
				DimRotation = bs.ReadSingle();
				Recoil = bs.ReadSingle();
				Friction = bs.ReadSingle();
				RigidType = bs.ReadByte();
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

			public PmdJoint(BinaryStream bs) {
				Name = bs.Sjis(20);
				RigidA = bs.ReadInt32();
				RigidB = bs.ReadInt32();
				Position = bs.Vector3();
				Rotation = bs.Vector3();
				Move1 = bs.Vector3();
				Move2 = bs.Vector3();
				Rotate1 = bs.Vector3();
				Rotate2 = bs.Vector3();
				SpringPosition = bs.Vector3();
				SpringRotation = bs.Vector3();
			}
		}
	}
}

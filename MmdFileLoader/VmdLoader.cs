using SlimDX;
using System.IO;
using System.Linq;

namespace MmdFileLoader {
	public class VmdLoader {
		public bool IsntCamera { get; }
		public VmdMotion[] Motion { get; }
		public VmdSkin[] Skin { get; }

		public VmdLoader(string Path) {
			using(var fs = new FileStream(Path, FileMode.Open))
			using(var bs = new BinaryStream(fs)) {
				bs.Sjis(30);
				if(bs.Sjis(20) != "カメラ・照明") IsntCamera = true;

				Motion = Enumerable.Range(0, bs.ReadInt32()).Select(x => new VmdMotion(bs)).ToArray();
				Skin = Enumerable.Range(0, bs.ReadInt32()).Select(x => new VmdSkin(bs)).ToArray();
			}
		}
	}

	public class VmdMotion {
		public string BoneName;
		public int FrameCount;
		public Vector3 Position;
		public Quaternion Rotation;
		public Interpolation XInterp;
		public Interpolation YInterp;
		public Interpolation ZInterp;
		public Interpolation RotInterp;

		public VmdMotion(BinaryStream bs) {
			BoneName = bs.Sjis(15);
			FrameCount = bs.ReadInt32();
			Position = bs.Vector3();
			Rotation = bs.Quaternion();
			byte[] interp = bs.ReadBytes(64);
			XInterp = new Interpolation(interp, 0);
			YInterp = new Interpolation(interp, 1);
			ZInterp = new Interpolation(interp, 2);
			RotInterp = new Interpolation(interp, 3);
		}
	}

	public class Interpolation {
		public byte AX;
		public byte AY;
		public byte BX;
		public byte BY;

		public Interpolation(byte[] interp, int offset) {
			AX = interp[offset + 4 * 0];
			AY = interp[offset + 4 * 1];
			BX = interp[offset + 4 * 2];
			BY = interp[offset + 4 * 3];
		}
	}

	public class VmdSkin {
		public string Name;
		public int FrameCount;
		public float Weight;

		public VmdSkin(BinaryStream bs) {
			Name = bs.Sjis(15);
			FrameCount = bs.ReadInt32();
			Weight = bs.ReadSingle();
		}
	}
}

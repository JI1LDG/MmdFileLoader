using SlimDX;
using System.IO;
using System.Text;

namespace MmdFileLoader {
	static class Extensions {
		public static string Sjis(this BinaryReader src, int count) {
			var tmp = Encoding.GetEncoding("shift-jis").GetString(src.ReadBytes(count));
			if(tmp.IndexOf('\0') > 0) return tmp.Substring(0, tmp.IndexOf('\0'));
			else return tmp;
		}

		public static Vector2 Vector2(this BinaryReader src) {
			return new Vector2(src.ReadSingle(), src.ReadSingle());
		}

		public static Vector3 Vector3(this BinaryReader src) {
			return new Vector3(src.ReadSingle(), src.ReadSingle(), src.ReadSingle());
		}

		public static Vector3 Vector3AsRadian(this BinaryReader src) {
			var tmp = Vector3(src);
			for(int i = 0;i < 3;i++) tmp[i] *= (float)(180 / System.Math.PI);
			return tmp;
		}

		public static void SkipShort(this BinaryReader src, int count) {
			for(int i = 0; i < count; i++) {
				src.ReadInt16();
			}
		}
	}
}
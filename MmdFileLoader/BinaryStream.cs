using System.IO;
using System.Linq;
using System.Text;
using Sx = SlimDX;

namespace MmdFileLoader {
	public class BinaryStream : BinaryReader {
		private Encoding encode;

		public BinaryStream(FileStream fileStream) : base(fileStream) {
			encode = Encoding.GetEncoding("Shift-JIS");
		}

		public void ChangeEncoding(string changeFor) {
			encode = Encoding.GetEncoding(changeFor);
		}

		public string ReadTextBuf() {
			return encode.GetString(this.ReadBytes(this.ReadInt32()));
		}
		public string Sjis(int count) {
			var tmp = Encoding.GetEncoding("shift-jis").GetString(this.ReadBytes(count));
			if(tmp.IndexOf('\0') > 0) return tmp.Substring(0, tmp.IndexOf('\0'));
			else return tmp;
		}

		public Sx.Vector2 Vector2() {
			return new Sx.Vector2(this.ReadSingle(), this.ReadSingle());
		}

		public Sx.Vector3 Vector3() {
			return new Sx.Vector3(Vector2(), this.ReadSingle());
		}

		public Sx.Color3 Color3() {
			return new Sx.Color3(this.ReadSingle(), this.ReadSingle(), this.ReadSingle());
		}

		public Sx.Vector3 Vector3AsRadian() {
			var tmp = this.Vector3();
			for(int i = 0; i < 3; i++) tmp[i] *= (float)(180 / System.Math.PI);
			return tmp;
		}

		public Sx.Vector4 Vector4() {
			return new Sx.Vector4(Vector3(), this.ReadSingle());
		}

		public Sx.Quaternion Quaternion() {
			return new Sx.Quaternion(Vector3(), this.ReadSingle());
		}

		public Sx.Color4 Color4() {
			return new Sx.Color4(this.ReadSingle(), this.ReadSingle(), this.ReadSingle(), this.ReadSingle());
		}

		public float[] ReadSingles(int count) {
			return Enumerable.Range(0, count).Select(x => this.ReadSingle()).ToArray();
		}

		public void SkipShort(int count) {
			for(int i = 0; i < count; i++) {
				this.ReadInt16();
			}
		}

		public int ReadIndex(byte size, bool isVertex = false) {
			switch(size) {
				case 1:
					if(isVertex) return this.ReadByte();
					else return this.ReadSByte();
				case 2:
					if(isVertex) return this.ReadUInt16();
					return this.ReadInt16();
				case 3:
					return this.ReadInt32();
				default: return -1;
			}
		}

		public int[] ReadIndexes(byte size, int count, bool isVertex = false) {
			return Enumerable.Range(0, count).Select(x => ReadIndex(size, isVertex)).ToArray();
		}
	}
}

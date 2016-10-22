namespace MmdFileLoader {
	class Program {
		/*static void Main(string[] args) {
			var pl = new MmdLoader(@"cirno\cirno.pmd");
		}*/
	}

	public enum DrawFlagEnumes {
		DrawBoth = 0x01, GroundShadow = 0x02, DrawForSelfShadowMap = 0x04,
		DrawSelfShadow = 0x08, DrawEdge = 0x10,
	}

	public enum BoneFlagEnum {
		AssignIndex = 0x01, CanRotate = 0x02, CanMove = 0x04,
		Draw = 0x08, CanControl = 0x10, Ik = 0x20,
		AddRotate = 0x0100, AddMove = 0x0200,
		FixAxis = 0x0400, LocalAxis = 0x0800,
		TransformAfterPhysic = 0x1000, TransformOuterParent = 0x2000
	}
}

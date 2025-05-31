using System.Drawing;

namespace Falling_Sand {
	internal class Tile {
		public static Color emptyColor = Color.Black;
		public bool isWall = false;
		public bool isFloor = false;
		public Sand? sand = null;
	}
}

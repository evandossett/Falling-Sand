using System.Drawing;

namespace Falling_Sand {
	internal class Sand {
		public static int Preview_Opacity = 200;

		/**
		 * 0 = can be stacked
		 * 1 = requires 1 piece of sand on either side
		 * 2 = requires 2 pieces of sand on either side
		 * n = requires n pieces of sand on either side
		 */
		public int SurfaceTension = 1;

		/**
		 * Determines if a piece of sand can sink into a different piece
		 */
		public float Density = 1f;

		public Color Color = Color.White;

		public Sand() { }

		public Sand(Sand sand) {
			Color = sand.Color;
			Density = sand.Density;
			SurfaceTension = sand.SurfaceTension;
		}

		public Color GetOpaqueColor(int opacity) => Color.FromArgb(opacity, Color);
		public Brush GetOpaqueBrush(int opacity) => new SolidBrush(Color.FromArgb(opacity, Color));
		public Brush GetSolidBrush() => new SolidBrush(Color);
	}
}

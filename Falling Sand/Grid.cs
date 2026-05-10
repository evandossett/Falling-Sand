using System.Drawing;

namespace Falling_Sand {
	internal class Grid {
		public Tile[][] Tiles;
		private int height = 0;
		private int width = 0;
		private static readonly Random random = new Random((int)DateTime.UtcNow.Ticks);
		private bool[][] updated;

		public Grid(int width, int height) {
			Tiles = new Tile[height][];
			updated = new bool[height][];
			for (int y = 0; y < height; y++) {
				Tiles[y] = new Tile[width];
				updated[y] = new bool[width];
				for (int x = 0; x < width; x++) {
					Tiles[y][x] = new Tile();
				}
			}
			this.height = height;
			this.width = width;
		}

		public Dictionary<Color, List<Rectangle>> GenerateGrid() {
			if (Tiles.Length <= 0)
				throw new Exception("No Tiles");

			Dictionary<Color, List<Rectangle>> grid = new Dictionary<Color, List<Rectangle>>();

			for (int x = 0; x < width; x++) {
				Rectangle r = new Rectangle();
				Color c = Color.Black;

				if (x == Window.current_preview_index)
					c = Window.current_preview_sand.GetOpaqueColor(Sand.Preview_Opacity);

				if (grid.ContainsKey(c)) {
					if (grid[c] == null)
						grid[c] = new List<Rectangle>();

					grid[c].Add(new Rectangle(new Point(x * Window.tile_scale, 0 * Window.tile_scale), new Size(1, 1) * Window.tile_scale));
				}
				else
					grid.Add(c, new List<Rectangle>() { new Rectangle(new Point(x * Window.tile_scale, 0 * Window.tile_scale), new Size(1, 1) * Window.tile_scale) });
			}

			for (int y = 1; y < height; y++) {
				for (int x = 0; x < width; x++) {
					Color c = Color.Black;

					if (Tiles[y][x].sand != null)
						c = Tiles[y][x].sand.Color;

					if (grid.ContainsKey(c)) {
						if (grid[c] == null)
							grid[c] = new List<Rectangle>();

						grid[c].Add(new Rectangle(new Point(x * Window.tile_scale, y * Window.tile_scale), new Size(1, 1) * Window.tile_scale));
					}
					else
						grid.Add(c, new List<Rectangle>() { new Rectangle(new Point(x * Window.tile_scale, y * Window.tile_scale), new Size(1, 1) * Window.tile_scale) });
				}
			}

			return grid;
		}

		public void SpawnSand(int index) {
			if (index > width || index < 0)
				throw new Exception();
			else if (Tiles[0][index].sand != null)
				return;

			Tiles[0][index].sand = new Sand();
		}

		public void SpawnSand(int index, Sand sand) {
			if (index > width || index < 0)
				throw new Exception();
			else if (Tiles[0][index].sand != null)
				return;

			Tiles[0][index].sand = sand;
		}

		public void Simulate() {
			while (SimulateFrame() == true) { SimulateFrame(); }
		}

		public bool SimulateFrame() {
			bool simulated = false;

			for (int y = 0; y < height; y++) {
				Array.Clear(updated[y], 0, width);
			}

			for (int y = height - 1; y >= 0; y--) {
				for (int x = 0; x < width; x++) {
					Sand currentSand = Tiles[y][x].sand;

					if (currentSand == null || updated[y][x])
						continue;

					if (y + 1 < height) {

						// FALLING CHECK
						if (Tiles[y + 1][x].sand == null) {
							Tiles[y + 1][x].sand = currentSand;
							Tiles[y][x].sand = null;
							updated[y + 1][x] = true;
							simulated = true;
							continue;
						}

						// DENSITY CHECK
						if (Tiles[y + 1][x].sand.Density < currentSand.Density && !updated[y + 1][x]) {
							Sand lighterSand = Tiles[y + 1][x].sand;
							Tiles[y + 1][x].sand = currentSand;
							Tiles[y][x].sand = lighterSand;

							updated[y + 1][x] = true;
							updated[y][x] = true;
							simulated = true;
							continue;
						}

						// SURFACE TENSION
						int spread = currentSand.SurfaceTension;
						if (spread > 0) {
							bool moved = false;
							int dir = random.Next(2) == 0 ? 1 : -1;

							for (int i = 0; i < 2; i++) {
								int stepX = dir;
								for (int offset = 1; offset <= spread; offset++) {
									int targetX = x + (stepX * offset);

									if (targetX < 0 || targetX >= width)
										break;

									if (Tiles[y][targetX].sand != null)
										break;

									if (Tiles[y + 1][targetX].sand == null) {
										Tiles[y + 1][targetX].sand = currentSand;
										Tiles[y][x].sand = null;
										updated[y + 1][targetX] = true;
										simulated = true;
										moved = true;
										break;
									}

									if (Tiles[y + 1][targetX].sand.Density < currentSand.Density && !updated[y + 1][targetX]) {
										Sand lighterSand = Tiles[y + 1][targetX].sand;
										Tiles[y + 1][targetX].sand = currentSand;
										Tiles[y][x].sand = lighterSand;

										updated[y + 1][targetX] = true;
										updated[y][x] = true;
										simulated = true;
										moved = true;
										break;
									}
								}

								if (moved)
									break;
								dir = -dir;
							}
							if (moved)
								continue;
						}
					}
				}
			}
			return simulated;
		}
	}
}
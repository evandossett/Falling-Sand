using System.Drawing;

namespace Falling_Sand {
	internal class Grid {
		public Tile[][] Tiles; // (y, x) AKA inverted coordinate system
		private int height = 0;
		private int width = 0;
		private static readonly Random random = new Random((int)DateTime.UtcNow.Ticks);

		public Grid(int width, int height) {
			Tiles = new Tile[height][];
			for (int y = 0 ; y < height ; y++) {
				Tiles[y] = new Tile[width];
				for (int x = 0 ; x < width ; x++) {
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

			for (int x = 0 ; x < width ; x++) {
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

			for (int y = 1 ; y < height ; y++) {
				for (int x = 0 ; x < width ; x++) {
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
			if (index >= width)
				throw new Exception();
			else if (Tiles[0][index].sand != null)
				return;

			Tiles[0][index].sand = new Sand();
		}

		public void SpawnSand(int index, Sand sand) {
			if (index >= width)
				throw new Exception();
			else if (Tiles[0][index].sand != null)
				return;

			Tiles[0][index].sand = sand;
		}

		/**
		 * Loop through each tile and find a piece of sand 
		 * Follow one of the sand through the simulations if applicable:
		 *	1. Falling - ensures sand falls until it hits sand or the floor 
		 *	2. Overflow - ensures sand is following its surface area rule, pushing it until it hits a wall if it doesnt
		 *	3. Pushing - ensures sand pushes any less dense sand up above itself
		 * Continue to loop until no pieces of sand need to be simulated
		 */
		public void Simulate() {
			while (SimulateFrame() == true) { SimulateFrame(); }
		}

		public bool SimulateFrame() {
			bool simulated = false;
			List<Sand> simsand = new List<Sand>();
			for (int y = height - 1 ; y >= 0 ; y--) {
				for (int x = 0 ; x < width ; x++) {
					if (Tiles[y][x].sand != null && !simsand.Contains(Tiles[y][x].sand ?? new Sand()) && y + 1 < height) {
						if (Tiles[y + 1][x].sand == null) { //FALLING CHECK
							simsand.Add(Tiles[y][x].sand);
							Tiles[y + 1][x].sand = Tiles[y][x].sand;
							Tiles[y][x].sand = null;
							simulated = true;
						} //FALING CHECK
						else if (Tiles[y + 1][x].sand.Density < Tiles[y][x].sand.Density) { //DENSITY CHECK
							simsand.Add(Tiles[y][x].sand);
							Sand sand = Tiles[y + 1][x].sand;
							Tiles[y + 1][x].sand = Tiles[y][x].sand;
							Tiles[y][x].sand = sand;
							simulated = true;
						} //DENSITY CHECK
						else { //SURFACE TENSION CHECK
							int offset_check = 0;
							int surface_tension = Tiles[y][x].sand.SurfaceTension;
							while (true) {
								if (offset_check > surface_tension || (x + offset_check >= width && x - offset_check < 0))
									break;

								if (offset_check + x < width && x + offset_check >= 0 && Tiles[y + 1][x + offset_check].sand == null && x - offset_check < width && x - offset_check >= 0 && Tiles[y + 1][x - offset_check].sand == null) {
									switch (random.Next(2)) {
										case 0:
											Tiles[y + 1][x + offset_check].sand = Tiles[y][x].sand;
											Tiles[y][x].sand = null;
											simulated = true;
											break;
										case 1:
											Tiles[y + 1][x - offset_check].sand = Tiles[y][x].sand;
											Tiles[y][x].sand = null;
											simulated = true;
											break;
									}

									break;
								}

								if (offset_check + x < width && x + offset_check >= 0 && Tiles[y + 1][x + offset_check].sand == null) {
									simsand.Add(Tiles[y][x].sand);
									Tiles[y + 1][x + offset_check].sand = Tiles[y][x].sand;
									Tiles[y][x].sand = null;
									simulated = true;
									break;
								}

								if (x - offset_check < width && x - offset_check >= 0 && Tiles[y + 1][x - offset_check].sand == null) {
									simsand.Add(Tiles[y][x].sand);
									Tiles[y + 1][x - offset_check].sand = Tiles[y][x].sand;
									Tiles[y][x].sand = null;
									simulated = true;
									break;
								}

								if (offset_check + x < width && x + offset_check >= 0 && Tiles[y + 1][x + offset_check].sand.Density < Tiles[y][x].sand.Density) {
									simsand.Add(Tiles[y][x].sand);
									Tiles[y + 1][x + offset_check].sand = Tiles[y][x].sand;
									Tiles[y][x].sand = null;
									simulated = true;
									break;
								}

								if (x - offset_check < width && x - offset_check >= 0 && Tiles[y + 1][x - offset_check].sand.Density < Tiles[y][x].sand.Density) {
									simsand.Add(Tiles[y][x].sand);
									Sand sand = Tiles[y + 1][x - offset_check].sand;
									Tiles[y + 1][x - offset_check].sand = Tiles[y][x].sand;
									int vertoff = 0;
									while (true) {
										if (Tiles[y + vertoff][x - offset_check].sand != null) {
											Sand sand2 = Tiles[y + vertoff][x - offset_check].sand;
											Tiles[y + vertoff][x - offset_check].sand = sand;
											sand = sand2;
											vertoff--;
										}
										else if (Tiles[y + vertoff][x - offset_check].sand == null) {
											Tiles[y + vertoff][x - offset_check].sand = sand;
											break;
										}
									}
									Tiles[y][x].sand = null;
									simulated = true;
									break;
								}

								if (x + offset_check < width && x + offset_check >= 0 && Tiles[y + 1][x + offset_check].sand.Density < Tiles[y][x].sand.Density) {
									simsand.Add(Tiles[y][x].sand);
									Sand sand = Tiles[y + 1][x + offset_check].sand;
									Tiles[y + 1][x + offset_check].sand = Tiles[y][x].sand;
									int vertoff = 0;
									while (true) {
										if (Tiles[y + vertoff][x + offset_check].sand != null) {
											Sand sand2 = Tiles[y + vertoff][x + offset_check].sand;
											Tiles[y + vertoff][x + offset_check].sand = sand;
											sand = sand2;
											vertoff--;
										}
										else if (Tiles[y + vertoff][x + offset_check].sand == null) {
											Tiles[y + vertoff][x + offset_check].sand = sand;
											break;
										}
									}
									Tiles[y][x].sand = null;
									simulated = true;
									break;
								}

								offset_check++;
							}
						}//SURFACE TENSION CHECK
					}
				}
			}

			return simulated;
		}
	}
}

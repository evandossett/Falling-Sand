using System.Drawing;
using System.Windows.Forms;

namespace Falling_Sand {

	internal class Window {
		DrawingForm form;
		public static int tile_scale = -1;
		public static Tuple<int, int> resolution = Tuple.Create(-1, -1);
		int current_sand_index = 0;
		Grid grid;
		readonly System.Windows.Forms.Timer timer_hold_lmouse = new();
		public static int current_preview_index = -1;
		public static Sand current_preview_sand = new Sand();
		Thread SimulateThread { get; set; }
		Thread RenderThread { get; set; }
		int SimulateSleep = 0;
		int RenderSleep = 0;
		int InputTimeout = 0;
		bool PauseSimulate = false;
		bool PauseRender = false;
		bool PauseInput = false;
		int WindowHeightOffset = 40;

		Sand[] sands = {
			new Sand { Color = Color.White , Density = 1, SurfaceTension = 3},
			new Sand { Color = Color.Aquamarine , Density = 2, SurfaceTension = 3},
			new Sand { Color = Color.OrangeRed , Density = 3, SurfaceTension = 2},
			new Sand { Color = Color.Olive , Density = 1, SurfaceTension = 1},
			new Sand { Color = Color.LightPink , Density = 2, SurfaceTension = 0},
		};

		public Window(Tuple<int, int> res, int t_scale, int draw_rate) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			form = new DrawingForm {
				Text = "Sandbox",
				Width = res.Item1 * t_scale,
				Height = res.Item2 * t_scale + WindowHeightOffset,
				BackColor = Color.Black
			};

			resolution = res;
			tile_scale = t_scale;

			form.Paint += OnPaint;
			form.MouseDown += OnMouseDown;
			form.MouseMove += OnMouseMove;
			form.MouseUp += OnMouseUp;
			form.MouseWheel += OnMouseScroll;

			timer_hold_lmouse.Stop();
			timer_hold_lmouse.Interval = 1;
			timer_hold_lmouse.Tick += OnMouseHold;

			current_preview_sand = sands[0];

			grid = new Grid(res.Item1, res.Item2);

			SimulateThread = new Thread(OnSimulateTick);
			SetSimulateTimer(1000 / 60);
			StartSimulateTimer();

			RenderThread = new Thread(OnDrawTick);
			SetDrawTimer(draw_rate);
			StartDrawTimer();

			InputTimeout = 1000 / 16;

			Application.Run(form);
		}

		public void SetDrawTimer(int interval) { RenderSleep = interval; }
		public void SetSimulateTimer(int interval) { SimulateSleep = interval; }

		public void StartDrawTimer() {
			RenderThread.Start();
		}

		public void StartSimulateTimer() {
			SimulateThread.Start();
		}

		public void StartDrawTimer(int interval) {
			RenderSleep = interval;
			StartDrawTimer();
		}

		public void StartSimulateTimer(int interval) {
			SimulateSleep = interval;
			StartSimulateTimer();
		}

		public void ToggleDrawTimer() { PauseRender = !PauseRender; }

		public void ToggleSimulateTimer() { PauseSimulate = !PauseSimulate; }

		public void OnDrawTick() {
			while (PauseRender) { Thread.Sleep(RenderSleep); }
			form.Invalidate();
			Thread.Sleep(RenderSleep);
			OnDrawTick();
		}

		public void OnSimulateTick() {
			while (PauseSimulate) { Thread.Sleep(SimulateSleep); }
			grid.SimulateFrame();
			Thread.Sleep(SimulateSleep);
			OnSimulateTick();
		}

		public void OnPaint(object? sender, PaintEventArgs e) {
			PauseRender = true;
			Dictionary<Color, List<Rectangle>> d_grid = grid.GenerateGrid();

			e.Graphics.Clear(Color.Black);
			foreach (Color c in d_grid.Keys) {
				e.Graphics.FillRectangles(new SolidBrush(c), d_grid[c].ToArray());
			}
			PauseRender = false;
		}

		public void OnMouseDown(object? sender, MouseEventArgs e) {
			while (PauseInput) { Thread.Sleep(InputTimeout); }

			if (e.Button != MouseButtons.Left)
				return;

			if (e.X >= 0 && e.X < form.Width && e.Y >= 0 && e.Y < form.Height) {
				grid.SpawnSand(e.X / tile_scale, new Sand(current_preview_sand));
				timer_hold_lmouse.Start();
			}
		}

		public void OnMouseUp(object? sender, MouseEventArgs e) {
			timer_hold_lmouse.Stop();
		}

		public void OnMouseScroll(object? sender, MouseEventArgs e) {
			if (e.Delta < 0) {
				if (current_sand_index >= sands.Count() - 1)
					current_sand_index = 0;
				else
					current_sand_index++;
			}
			else if (e.Delta > 0) {
				if (current_sand_index <= 0)
					current_sand_index = sands.Count() - 1;
				else
					current_sand_index--;
			}
			else
				return;

			current_preview_sand = sands[current_sand_index];
		}

		public void OnMouseHold(object? sender, EventArgs e) {
			if (current_preview_index * tile_scale >= 0 && current_preview_index * tile_scale < form.Height)
				grid.SpawnSand(current_preview_index, new Sand(sands[current_sand_index]));
		}

		public void OnMouseMove(object? sender, MouseEventArgs e) {
			if (e.X >= 0 && e.X < form.Width && e.Y >= 0 && e.Y < form.Height)
				current_preview_index = e.X / tile_scale;
		}
	}
}
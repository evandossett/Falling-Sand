using System.Drawing;
using System.Windows.Forms;

namespace Falling_Sand {
	class DrawingForm : Form {
		public DrawingForm() {
			DoubleBuffered = true;
			SetStyle(ControlStyles.OptimizedDoubleBuffer |
					    ControlStyles.AllPaintingInWmPaint |
					    ControlStyles.UserPaint, true);
			UpdateStyles();
			BackColor = Color.Black;
		}
	}
}
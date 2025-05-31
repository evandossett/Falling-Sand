using Falling_Sand;

//Settings
int timescale = 1;
bool userandom = false;
bool usecustom = true;
bool usefullycustom = false;
bool usestream = true;
int framerate = 360; //0 = no limit
Tuple<int, int> grid_resolution = Tuple.Create(200, 100); // width , height
int sand_scale = 5;

//Check Settings
if (usecustom && usefullycustom)
	throw new Exception("NO NO NO");

int rate = 0;
if (framerate == -1)
	rate = -1;
else if (framerate == 0)
	rate = 0;
else if ((1000 / framerate) * timescale < 0)
	rate = 1;
else
	rate = (int)double.Ceiling((1000 / framerate) * timescale);

if (framerate < 0) {
	throw new Exception("You fucking dumbass");
}

Random random = new Random((int)DateTime.UtcNow.Ticks);

//Begin Application
Console.Write("Creating Window...");
Window window = new Window(grid_resolution, sand_scale, rate);

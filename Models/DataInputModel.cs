

namespace Image2Cpp.Models
{
    internal class DataInputModel
    {
        public Bitmap Image { get; set; }

        public string Name { get; set; } = "IMAGE";
        public int Width { get; set; } = 480;
        public int Height { get; set; } = 320;

        public ArduinoImageConverter.ColorDepth Colors { get; set; } = ArduinoImageConverter.ColorDepth.Bpp4;

        public bool UseColor565 { get; set; } = true;

        public bool IncludeGetPixel { get; set; } = true;
        public bool Dithering { get; set; } = true;

        public bool ImageBox { get; set; } = false;
    }
}

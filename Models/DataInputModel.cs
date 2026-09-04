

namespace Image2Cpp.Models
{
    public class DataInputModel
    {
        public Bitmap Image { get; set; }

        public int Brightness { get; set; } = 0;
        public int Contrast { get; set; } = 0;

        public string Name { get; set; } = "IMAGE";
        public int Width { get; set; } = 480;
        public int Height { get; set; } = 320;

        public bool BicubicResizing { get; set; } = true;

        public ArduinoImageConverter.ColorDepth Colors { get; set; } = ArduinoImageConverter.ColorDepth.Bpp4;

        public bool UseColor565 { get; set; } = true;

        public bool IncludeRenderFunction { get; set; } = true;
        public bool Dithering { get; set; } = true;

        public bool ImageBox { get; set; } = false;
        public uint MaxArraySize { get; set; } = 0;
    }
}

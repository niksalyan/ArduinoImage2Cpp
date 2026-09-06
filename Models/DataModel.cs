

using System.ComponentModel;

namespace Image2Cpp.Models
{
    public class DataModel
    {

        [Category("Image2CPP")]
        public DataInputModel Input { get; set; } = new DataInputModel();
        [Category("Image2CPP")]
        public DataOutputModel Output { get; set; } = new DataOutputModel();
        [Category("Image2CPP")]
        public DataRenderBoxOptionsModel RenderBoxOptions { get; set; } = new DataRenderBoxOptionsModel();

    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataInputModel
    {
        public Bitmap Image { get; set; } = null;
        public Size Resize { get; set; } = new Size(240, 160);

        public bool Bicubic { get; set; } = false;
        public bool Dithering { get; set; } = false;

        public int Brightness { get; set; } = 0;
        public int Contrast { get; set; } = 0;

        public uint MaxColors { get; set; } = 32;

        public override string ToString()
        {
            return "";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataRenderBoxOptionsModel
    {
        public bool FastBox { get; set; } = false;
        public uint StartFrom { get; set; } = 0;
        public override string ToString()
        {
            return "";
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataOutputModel
    {
        public string Name { get; set; } = "IMAGE";



        [Description("Color depth for the output image")]
        public ArduinoImageConverter.ColorDepth Palette { get; set; } = ArduinoImageConverter.ColorDepth.Bpp4;

        [Description("Render type for the output image")]
        public RenderType RenderType { get; set; } = RenderType.ArduinoPixel;

        [Description("Use 565 color format")]
        public bool UseColor565 { get; set; } = true;

        public bool IncludeRenderFunction { get; set; } = true;
        

        

        

        public override string ToString()
        {
            return "";
        }
    }

    public enum RenderType
    {
        ArduinoPixel,
        ArduinoImageBox
    }
}

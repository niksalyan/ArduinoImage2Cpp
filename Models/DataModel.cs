

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text.Json.Serialization;

namespace Image2Cpp.Models
{
    public class DataModel
    {

        [Category("Input")]
        [JsonIgnore]
        public Bitmap Image { get; set; } = null;

        [Category("Input"), Description("Render type for the output image")]
        public RenderType RenderMethod { get; set; } = RenderType.ArduinoPixel;

        [Category("Options")]
        public DataInputModel Input { get; set; } = new DataInputModel();
        [Category("Options")]
        public DataOutputModel Output { get; set; } = new DataOutputModel();
        [Category("Options")]
        public DataRenderBoxOptionsModel RenderBoxOptions { get; set; } = new DataRenderBoxOptionsModel();

    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataInputModel
    {
        
        public Size Resize { get; set; } = new Size(240, 160);

        public bool Bicubic { get; set; } = false;
        public bool Dithering { get; set; } = false;

        public int Brightness { get; set; } = 0;
        public int Contrast { get; set; } = 0;

        public uint MaxColors { get; set; } = 32;

        [Description("Crop rectangle applied before resizing. Crop is enabled when Width and Height > 0.")]
        public Padding Crop { get; set; } = new Padding();


        // CropEnabled property removed: check Crop rectangle values directly when needed.

        public override string ToString()
        {
            return "";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataRenderBoxOptionsModel
    {
        public bool FastBox { get; set; } = false;
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

        

        [Description("Use 565 color format")]
        public bool UseColor565 { get; set; } = true;

        public bool IncludeRenderFunction { get; set; } = true;

        [Description("Enable transparency preview and output")]
        public bool EnableTransparency { get; set; } = false;

        [Description("Transparent color palette index. Set to -1 for no transparency")]
        public int TransparentIndex { get; set; } = -1;
        

        

        

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

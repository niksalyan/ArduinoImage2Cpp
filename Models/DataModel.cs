

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text.Json.Serialization;
using System.Drawing.Imaging.Effects;

namespace Image2Cpp.Models
{
    public class DataModel
    {

        [Category("Input")]
        [Description("Source bitmap. Not included when saving the model.")]
        [JsonIgnore]
        public Bitmap Image { get; set; } = null;

        [Category("Input"), Description("Render type for the output image")]
        public RenderType RenderMethod { get; set; } = RenderType.ArduinoPixel;

        [Category("Options"), Description("Image processing filters applied after resizing and before palette conversion.")]
        public DataFiltersModel Filters { get; set; } = new DataFiltersModel();

        public DataPaletteModel Palette { get; set; } = new DataPaletteModel();

        [Category("Options")]
        [Description("Input processing options such as resize, crop and color reduction settings.")]
        public DataInputModel Input { get; set; } = new DataInputModel();
        [Category("Options")]
        [Description("Output formatting options including palette and render settings.")]
        public DataOutputModel Output { get; set; } = new DataOutputModel();

    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataInputModel
    {
        [Description("Size to scale the (cropped) image to before palette conversion.")]
        public Size Resize { get; set; } = new Size(240, 160);

        [Description("Use bicubic interpolation when resizing (higher quality, slower).")]
        public bool Bicubic { get; set; } = false;

        [Description("Enable Floyd-Steinberg dithering when mapping to the palette.")]
        public bool Dithering { get; set; } = false;

        

        [Description("Maximum number of colors to include in the generated palette.")]
        public uint MaxColors { get; set; } = 32;

        [Description("Crop padding (pixels) applied before resizing. Values are Left/Top/Right/Bottom.")]
        public Padding Crop { get; set; } = new Padding();


        // CropEnabled property removed: check Crop rectangle values directly when needed.

        public override string ToString()
        {
            return "";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataPaletteModel
    {

        public bool UseCustomPalette { get; set; } = false;
        public List<Color> Colors { get; set; } = new List<Color>() { 
            Color.Black,
            Color.Red,
            Color.Green,
            Color.Blue
        };

        public override string ToString()
        {
            return Colors.Count + " colors";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataFiltersModel
    {
        [Description("Brightness adjustment applied after resizing (-255..255).")]
        public int Brightness { get; set; } = 0;

        [Description("Contrast adjustment applied after resizing (-100..100).")]
        public int Contrast { get; set; } = 0;

        public int CyanRed { get; set; } = 0;
        public int MagentaGreen { get; set; } = 0;
        public int YellowBlue { get; set; } = 0;

        public void Apply(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                try
                {
                    bitmap.ApplyEffect(new BrightnessContrastEffect(Brightness, Contrast));
                    if (CyanRed != 0 || MagentaGreen != 0 || YellowBlue != 0)
                    {
                        bitmap.ApplyEffect(new ColorBalanceEffect(CyanRed, MagentaGreen, YellowBlue));
                    }
                } catch { }
            }
        }



        public override string ToString()
        {
            return "";
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataOutputModel
    {
        [Description("Identifier used for generated Arduino symbols (e.g. IMAGE).")]
        public string Name { get; set; } = "IMAGE";



        [Description("Color depth for the output image")]
        public ArduinoImageConverter.ColorDepth Palette { get; set; } = ArduinoImageConverter.ColorDepth.Bpp4;

        

        [Description("Use 565 color format for generated palette values.")]
        public bool UseColor565 { get; set; } = true;

        [Description("Include helper rendering functions in the generated Arduino code.")]
        public bool IncludeRenderFunction { get; set; } = true;

        // TransparentIndex removed. Transparency is now determined by TransparentColor (optionally taken from top-left).

        [JsonIgnore]
        [Description("Transparent color used to detect transparent pixels. Leave empty to disable.")]
        public Color TransparentColor { get; set; } = Color.Empty;

        [Description("Use the top-left pixel of the resized image as the transparent color.")]
        public bool TransparentFromTopLeft { get; set; } = false;

        [Browsable(false)]
        public int TransparentColorArgb
        {
            get => TransparentColor.ToArgb();
            set => TransparentColor = Color.FromArgb(value);
        }

        [Description("Tolerance for transparent color matching (Euclidean distance, 0 = exact match).")]
        public int TransparentTolerance { get; set; } = 0;

        [Description("Optimize box rendering with the fast fillRect API when available.")]
        public bool FastBox { get; set; } = false;

        public uint DetailLevel { get; set; } = 1;



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

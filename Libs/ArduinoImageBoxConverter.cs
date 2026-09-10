using Image2Cpp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

public static class ArduinoImageBoxConverter
{
    // ============================================================
    // BOX
    // ============================================================

    public sealed class ImageBox
    {
        public byte X { get; }
        public byte Y { get; }
        public byte Width { get; }
        public byte Height { get; }
        public byte ColorIndex { get; }

        public ImageBox(
            int x,
            int y,
            int width,
            int height,
            int colorIndex)
        {
            if (x < 0 || x > 255)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y > 255)
                throw new ArgumentOutOfRangeException(nameof(y));

            if (width < 1 || width > 255)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 1 || height > 255)
                throw new ArgumentOutOfRangeException(nameof(height));

            if (colorIndex < 0 || colorIndex > 255)
                throw new ArgumentOutOfRangeException(nameof(colorIndex));

            X = (byte)x;
            Y = (byte)y;
            Width = (byte)width;
            Height = (byte)height;
            ColorIndex = (byte)colorIndex;
        }
    }


    // ============================================================
    // RESULT
    // ============================================================

    public sealed class BoxImage
    {
        public int Width { get; }
        public int Height { get; }

        public ArduinoImageConverter.ColorDepth ColorDepth { get; }

        public ImageBox[] Boxes { get; }

        public Color[] Palette { get; }

        // Transparent palette index (-1 = none)
        public int TransparentIndex { get; }

        public int BoxCount => Boxes.Length;

        public int ColorsCount => Palette.Length;

        public BoxImage(
            int width,
            int height,
            ArduinoImageConverter.ColorDepth colorDepth,
            ImageBox[] boxes,
            Color[] palette,
            int transparentIndex = -1)
        {
            Width = width;
            Height = height;
            ColorDepth = colorDepth;
            Boxes = boxes;
            Palette = palette;
            TransparentIndex = transparentIndex;
        }
        public Bitmap ToBitmap()
        {
            return ToBitmap(preview: false);
        }

        public Bitmap ToBitmap(bool preview = false)
        {
            // Use 32bpp to support alpha when transparency is enabled
            var bitmap = new Bitmap(
                Width,
                Height,
                PixelFormat.Format32bppArgb);

            using Graphics g = Graphics.FromImage(bitmap);

            if (preview)
            {
                int cell = 8;
                Color c1 = Color.FromArgb(220, 220, 220);
                Color c2 = Color.FromArgb(180, 180, 180);

                for (int y = 0; y < bitmap.Height; y += cell)
                {
                    for (int x = 0; x < bitmap.Width; x += cell)
                    {
                        bool odd = ((x / cell) + (y / cell)) % 2 == 1;
                        using (Brush b = new SolidBrush(odd ? c1 : c2))
                        {
                            g.FillRectangle(b, x, y, cell, cell);
                        }
                    }
                }
            }
            else
            {
                // Start fully transparent
                g.Clear(Color.Transparent);
            }

            foreach (ImageBox box in Boxes)
            {
                if (box.ColorIndex >= Palette.Length)
                    continue;

                if (TransparentIndex >= 0 && box.ColorIndex == TransparentIndex)
                    continue; // skip transparent boxes

                Color color = Palette[box.ColorIndex];

                using var brush = new SolidBrush(color);

                g.FillRectangle(
                        brush,
                        box.X,
                        box.Y,
                        box.Width,
                        box.Height);
            }

            return bitmap;
        }

        // ========================================================
        // ARDUINO CODE
        // ========================================================

        public string ToArduinoCode(DataModel data)
        {
            if (string.IsNullOrWhiteSpace(data.Output.Name))
                throw new ArgumentException(
                    "Name cannot be empty.",
                    nameof(data.Output.Name));

            string identifier =
                MakeIdentifier(data.Output.Name);

            string macroName =
                identifier.ToUpperInvariant();

            var sb = new StringBuilder();

            sb.AppendLine("// ============================================================");
            sb.AppendLine($"// Generated by ArduinoImageBoxConverter - {macroName}");
            sb.AppendLine("// ============================================================");
            sb.AppendLine();

            // ----------------------------------------------------
            // DEFINES
            // ----------------------------------------------------

            sb.AppendLine(
                $"#define {macroName}_WIDTH       {Width}");

            sb.AppendLine(
                $"#define {macroName}_HEIGHT      {Height}");

            sb.AppendLine(
                $"#define {macroName}_BPP         {(int)ColorDepth}");

            sb.AppendLine(
                $"#define {macroName}_COLORS      {ColorsCount}");

            sb.AppendLine(
                $"#define {macroName}_BOX_COUNT   {BoxCount}");

            sb.AppendLine();

            // ----------------------------------------------------
            // BOX STRUCT
            // ----------------------------------------------------

            sb.AppendLine(
                $"struct {identifier}Box");

            sb.AppendLine("{");

            sb.AppendLine("    uint8_t x;");
            sb.AppendLine("    uint8_t y;");
            sb.AppendLine("    uint8_t width;");
            sb.AppendLine("    uint8_t height;");
            sb.AppendLine("    uint8_t color;");

            sb.AppendLine("};");
            sb.AppendLine();

            // ----------------------------------------------------
            // BOX DATA
            // ----------------------------------------------------

            sb.AppendLine(
                $"const {identifier}Box " +
                $"{identifier}Boxes[] PROGMEM =");

            sb.AppendLine("{");

            for (int i = 0; i < Boxes.Length; i++)
            {
                ImageBox box = Boxes[i];

                sb.AppendLine(
                    $"    {{ {box.X}, {box.Y}, " +
                    $"{box.Width}, {box.Height}, " +
                    $"{box.ColorIndex} }}" +
                    (i < Boxes.Length - 1 ? "," : ""));
            }

            sb.AppendLine("};");
            sb.AppendLine();

            // ----------------------------------------------------
            // PALETTE
            // ----------------------------------------------------

            // Palette is always emitted in RGB565 uint16_t values
            sb.AppendLine(
                $"const uint16_t {identifier}Palette" +
                $"[{ColorsCount}] PROGMEM =");

            sb.AppendLine("{");

            for (int i = 0; i < Palette.Length; i++)
            {
                ushort rgb565 =
                    ToRgb565(Palette[i]);

                sb.Append(
                    $"    0x{rgb565:X4}");

                if (i < Palette.Length - 1)
                    sb.Append(",");

                sb.AppendLine();
            }

            sb.AppendLine("};");

            sb.AppendLine();

            // ----------------------------------------------------
            // TRANSPARENCY
            // ----------------------------------------------------
            // Do not emit a TRANSPARENT_INDEX macro. Palette index 0 is always treated as transparent
            bool hasTransparency = TransparentIndex >= 0 && this.TransparentIndex < Palette.Length;

            // ----------------------------------------------------
            // DRAW IMAGE
            // ----------------------------------------------------
            if (data.Output.IncludeRenderFunction)
            {
                AppendDrawImageFunction(
                sb,
                identifier,
                macroName,
                data.RenderMethod == RenderType.ArduinoImageBoxFast,
                hasTransparency);
            }
            

            return sb.ToString();
        }

    }


    // ============================================================
    // CONVERT
    // ============================================================

    public static BoxImage Convert(
        ArduinoImageConverter.IndexedImage image, DataModel data)
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image));

        if (image.Width < 1 || image.Width > 256)
            throw new ArgumentOutOfRangeException(
                nameof(image.Width),
                "Image width must be between 1 and 256.");

        if (image.Height < 1 || image.Height > 256)
            throw new ArgumentOutOfRangeException(
                nameof(image.Height),
                "Image height must be between 1 and 256.");

        byte[] indexes =
            UnpackIndexes(
                image.Data,
                image.Width,
                image.Height,
                image.ColorDepth);

        List<ImageBox> boxes =
            BuildBoxes(
                indexes,
                image.Width,
                image.Height,
                image.TransparentIndex);

        boxes = boxes.Where(b => b.Width * b.Height >= data.Output.DetailLevel).ToList();

        return new BoxImage(
            image.Width,
            image.Height,
            image.ColorDepth,
            boxes.ToArray(),
            image.Palette,
            image.TransparentIndex);
    }


    // ============================================================
    // BUILD BOXES
    //
    // LAYER RULE:
    //
    // Layer 0:
    //     Always fills the complete image.
    //
    // Layer N:
    //     Can overwrite:
    //         - itself
    //         - any later layer
    //
    //     Cannot overwrite:
    //         - any earlier layer
    //
    // Example:
    //
    //     Layer 0 = background
    //     Layer 1 = large shape
    //     Layer 2 = text
    //
    // Layer 1 may initially paint over Layer 2.
    // Layer 2 is then painted afterward.
    //
    // ============================================================

    private static List<ImageBox> BuildBoxes(
    byte[] pixels,
    int width,
    int height,
    int transparentIndex = -1)
    {
        var boxes = new List<ImageBox>();

        if (pixels == null ||
            width <= 0 ||
            height <= 0)
            return boxes;

        int pixelCount = width * height;

        if (pixels.Length < pixelCount)
            return boxes;

        // ========================================================
        // COUNT COLORS
        //
        // Transparent pixels are not drawable.
        // ========================================================

        int[] colorCounts = new int[256];

        for (int i = 0; i < pixelCount; i++)
        {
            byte color = pixels[i];

            if (transparentIndex >= 0 &&
                color == transparentIndex)
                continue;

            colorCounts[color]++;
        }

        // ========================================================
        // SORT COLORS
        //
        // Most common -> least common.
        //
        // This still gives us a useful background -> foreground
        // relationship when deciding which pixels a box may cover.
        // ========================================================

        byte[] colors =
            Enumerable
                .Range(0, 256)
                .Where(c => colorCounts[c] > 0)
                .OrderByDescending(c => colorCounts[c])
                .Select(c => (byte)c)
                .ToArray();

        if (colors.Length == 0)
            return boxes;

        // ========================================================
        // COLOR -> LAYER
        //
        // Lower layer = background
        // Higher layer = foreground
        // ========================================================

        int[] colorLayer = new int[256];

        Array.Fill(
            colorLayer,
            -1);

        for (int layer = 0;
             layer < colors.Length;
             layer++)
        {
            colorLayer[colors[layer]] = layer;
        }

        // ========================================================
        // USED
        //
        // A pixel can only be consumed once by a particular layer.
        // ========================================================

        int[] used = new int[pixelCount];

        int generation = 0;

        // ========================================================
        // GENERATE BOXES
        //
        // We process background -> foreground.
        //
        // Within each layer we always take the largest rectangle
        // available at the current position.
        //
        // Later layers can overwrite earlier layers.
        // ========================================================

        int startLayer =
            transparentIndex >= 0
                ? 0
                : 1;

        // Without transparency, the most common color becomes
        // the implicit full-screen background.
        if (transparentIndex < 0)
        {
            boxes.Add(
                new ImageBox(
                    0,
                    0,
                    width,
                    height,
                    colors[0]));

            if (colors.Length == 1)
                return boxes;
        }

        for (int layer = startLayer;
             layer < colors.Length;
             layer++)
        {
            byte color = colors[layer];

            generation++;

            // ====================================================
            // SCAN IMAGE
            //
            // Top -> bottom
            // Left -> right
            // ====================================================

            for (int y = 0;
                 y < height;
                 y++)
            {
                for (int x = 0;
                     x < width;
                     x++)
                {
                    int startIndex =
                        y * width + x;

                    // Already consumed by this layer.
                    if (used[startIndex] == generation)
                        continue;

                    // We only start a box on this layer's color.
                    if (pixels[startIndex] != color)
                        continue;

                    // ====================================================
                    // FIND MAXIMUM WIDTH
                    // ====================================================

                    int maxWidth = 0;

                    while (x + maxWidth < width)
                    {
                        int index =
                            startIndex + maxWidth;

                        // Already consumed.
                        if (used[index] == generation)
                            break;

                        byte pixelColor =
                            pixels[index];

                        // Transparency is an absolute boundary.
                        if (transparentIndex >= 0 &&
                            pixelColor == transparentIndex)
                            break;

                        // Earlier layer cannot be covered by this box.
                        if (colorLayer[pixelColor] < layer)
                            break;

                        maxWidth++;
                    }

                    if (maxWidth == 0)
                        continue;

                    // ====================================================
                    // FIND LARGEST RECTANGLE
                    //
                    // Width can only shrink as we move down.
                    // ====================================================

                    int bestWidth = 1;
                    int bestHeight = 1;

                    int currentWidth =
                        maxWidth;

                    for (int h = 1;
                         y + h <= height;
                         h++)
                    {
                        int rowStart =
                            (y + h - 1) * width + x;

                        int rowWidth = 0;

                        while (rowWidth < currentWidth)
                        {
                            int index =
                                rowStart + rowWidth;

                            if (used[index] == generation)
                                break;

                            byte pixelColor =
                                pixels[index];

                            // Transparency is a hard boundary.
                            if (transparentIndex >= 0 &&
                                pixelColor == transparentIndex)
                                break;

                            // Earlier layer is protected.
                            if (colorLayer[pixelColor] < layer)
                                break;

                            rowWidth++;
                        }

                        if (rowWidth == 0)
                            break;

                        currentWidth =
                            rowWidth;

                        int area =
                            currentWidth * h;

                        int bestArea =
                            bestWidth * bestHeight;

                        if (area > bestArea)
                        {
                            bestWidth =
                                currentWidth;

                            bestHeight =
                                h;
                        }
                    }

                    // ====================================================
                    // ADD BOX
                    // ====================================================

                    boxes.Add(
                        new ImageBox(
                            x,
                            y,
                            bestWidth,
                            bestHeight,
                            color));

                    // ====================================================
                    // MARK USED
                    // ====================================================

                    for (int yy = y;
                         yy < y + bestHeight;
                         yy++)
                    {
                        int offset =
                            yy * width + x;

                        for (int xx = 0;
                             xx < bestWidth;
                             xx++)
                        {
                            used[offset + xx] =
                                generation;
                        }
                    }
                }
            }
        }

        // ========================================================
        // IMPORTANT:
        //
        // DO NOT globally sort boxes by area here.
        //
        // The order generated above represents:
        //
        //     background -> foreground
        //
        // and later boxes are allowed to overwrite earlier ones.
        //
        // Sorting by area would break that relationship.
        // ========================================================

        return boxes;
    }


    // ============================================================
    // UNPACK INDEXES
    // ============================================================

    private static byte[] UnpackIndexes(
        byte[] data,
        int width,
        int height,
        ArduinoImageConverter.ColorDepth colorDepth)
    {
        int bits =
            (int)colorDepth;

        int pixelsPerByte =
            8 / bits;

        int mask =
            (1 << bits) - 1;

        byte[] indexes =
            new byte[width * height];

        for (int i = 0;
             i < indexes.Length;
             i++)
        {
            int byteIndex =
                i / pixelsPerByte;

            int position =
                i % pixelsPerByte;

            int shift =
                8 - bits * (position + 1);

            indexes[i] =
                (byte)(
                    (data[byteIndex] >> shift) &
                    mask);
        }

        return indexes;
    }


    // ============================================================
    // DRAW IMAGE FUNCTION
    // ============================================================

    private static void AppendDrawImageFunction(
        StringBuilder sb,
        string identifier,
        string macroName,
        bool fastApi,
        bool hasTransparency)
    {
        sb.AppendLine(
            $"static inline void {identifier}DrawImage(");

        sb.AppendLine(
            "    int16_t offsetX,");

        sb.AppendLine(
            "    int16_t offsetY,");

        sb.AppendLine(
            "    int16_t scaleX = 1,");

        sb.AppendLine(
            "    int16_t scaleY = 1)");

        sb.AppendLine("{");

        sb.AppendLine(
            $"    for (uint16_t i = 0; " +
            $"i < {macroName}_BOX_COUNT; i++)");

        sb.AppendLine("    {");

        sb.AppendLine(
            $"        {identifier}Box box;");

        sb.AppendLine();

        

        sb.AppendLine(
            $"        memcpy_P(" +
            $"&box, " +
            $"&{identifier}Boxes[i], " +
            $"sizeof(box));");

        sb.AppendLine();

        // -------------------------------------------------------
        // TRANSPARENCY: skip boxes that match the transparent index
        // -------------------------------------------------------
        if (hasTransparency)
        {
            sb.AppendLine("        if (box.color == 0)");
            sb.AppendLine("            continue;");
            sb.AppendLine();
        }

        // ========================================================
        // COLOR
        // ========================================================

        // Palette stored as RGB565 uint16_t
        sb.AppendLine(
            $"        uint16_t color = " +
            $"pgm_read_word(" +
            $"&{identifier}Palette[box.color]);");

        sb.AppendLine();

        // ========================================================
        // DRAW BOX
        // ========================================================

        sb.AppendLine(
            fastApi ?"        tft.fastFillRect(" : "        tft.fillRect(");

        sb.AppendLine(
            "            offsetX + box.x * scaleX,");

        sb.AppendLine(
            "            offsetY + box.y * scaleY,");

        sb.AppendLine(
            "            box.width * scaleX,");

        sb.AppendLine(
            "            box.height * scaleY,");

        sb.AppendLine(
            "            color);");

        sb.AppendLine("    }");

        sb.AppendLine("}");

        sb.AppendLine();
    }


    // ============================================================
    // RGB888 -> RGB565
    // ============================================================

    private static ushort ToRgb565(
        Color color)
    {
        return (ushort)(
            ((color.R & 0xF8) << 8) |
            ((color.G & 0xFC) << 3) |
            (color.B >> 3));
    }


    // ============================================================
    // IDENTIFIER
    // ============================================================

    private static string MakeIdentifier(
        string name)
    {
        var sb =
            new StringBuilder();

        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c) ||
                c == '_')
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('_');
            }
        }

        if (sb.Length == 0)
            sb.Append("image");

        if (char.IsDigit(sb[0]))
            sb.Insert(0, '_');

        return sb.ToString();
    }
}
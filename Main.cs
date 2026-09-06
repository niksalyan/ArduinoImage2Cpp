using Image2Cpp.Models;

namespace Image2Cpp
{
    public partial class Main : Form
    {
        private DataModel dataInputModel = new DataModel();
        public Main()
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = dataInputModel;
            propertyGrid1.ExpandAllGridItems();
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            try
            {
                
                if (dataInputModel.Input.Image != null)
                {
                    var indexed = ArduinoImageConverter.Convert(dataInputModel);
                    
                    switch(dataInputModel.Output.RenderType)
                    {
                        case RenderType.ArduinoPixel:
                            var r = indexed;
                            // If image contains transparency, render a checkerboard background for preview
                            if (r.TransparentIndex >= 0)
                            {
                                using Bitmap fg = r.ToBitmap();
                                var preview = new Bitmap(r.Width, r.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                                using (Graphics g = Graphics.FromImage(preview))
                                {
                                    // draw checkerboard
                                    int cell = 8;
                                    Color c1 = Color.FromArgb(220, 220, 220);
                                    Color c2 = Color.FromArgb(180, 180, 180);

                                    for (int y = 0; y < preview.Height; y += cell)
                                    {
                                        for (int x = 0; x < preview.Width; x += cell)
                                        {
                                            bool odd = ((x / cell) + (y / cell)) % 2 == 1;
                                            using (Brush b = new SolidBrush(odd ? c1 : c2))
                                            {
                                                g.FillRectangle(b, x, y, cell, cell);
                                            }
                                        }
                                    }

                                    // draw foreground with alpha
                                    g.DrawImage(fg, 0, 0, r.Width, r.Height);
                                }

                                pictureBox1.Image = preview;
                            }
                            else
                            {
                                pictureBox1.Image = r.ToBitmap();
                            }

                            textBox1.Text = r.ToArduinoCode(dataInputModel);
                            break;
                        case RenderType.ArduinoImageBox:
                            var rb = ArduinoImageBoxConverter.Convert(indexed);
                            if (rb.TransparentIndex >= 0)
                            {
                                using Bitmap fg = rb.ToBitmap();
                                var preview = new Bitmap(rb.Width, rb.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                                using (Graphics g = Graphics.FromImage(preview))
                                {
                                    int cell = 8;
                                    Color c1 = Color.FromArgb(220, 220, 220);
                                    Color c2 = Color.FromArgb(180, 180, 180);

                                    for (int y = 0; y < preview.Height; y += cell)
                                    {
                                        for (int x = 0; x < preview.Width; x += cell)
                                        {
                                            bool odd = ((x / cell) + (y / cell)) % 2 == 1;
                                            using (Brush b = new SolidBrush(odd ? c1 : c2))
                                            {
                                                g.FillRectangle(b, x, y, cell, cell);
                                            }
                                        }
                                    }

                                    g.DrawImage(fg, 0, 0, rb.Width, rb.Height);
                                }

                                pictureBox1.Image = preview;
                            }
                            else
                            {
                                pictureBox1.Image = rb.ToBitmap();
                            }

                            textBox1.Text = rb.ToArduinoCode(dataInputModel);
                            break;
                        default:
                            pictureBox1.Image = null;
                            textBox1.Text = "Render method not supported";
                            break;
                    }

                    
                    
                }
                else
                {
                    pictureBox1.Image = null;
                    textBox1.Text = "Image Not Selected";
                }
            }
            catch (Exception ex)
            {
                pictureBox1.Image = null;
                textBox1.Text = $"Error: {ex.Message}";
            }
        }
    }
}

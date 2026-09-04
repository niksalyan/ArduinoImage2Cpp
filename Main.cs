using Image2Cpp.Models;

namespace Image2Cpp
{
    public partial class Main : Form
    {
        private DataInputModel dataInputModel = new DataInputModel();
        public Main()
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = dataInputModel;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            try
            {
                pictureBox1.Image = dataInputModel.Image;
                if (dataInputModel.Image != null)
                {
                    var r = ArduinoImageConverter.Convert(dataInputModel.Image, dataInputModel.Width, dataInputModel.Height, dataInputModel.Colors, dataInputModel.Dithering);

                    if (dataInputModel.ImageBox)
                    {
                        var rb = ArduinoImageBoxConverter.Convert(r);
                        textBox1.Text = rb.ToArduinoCode(
                            dataInputModel.Name ?? "IMAGE",
                            dataInputModel.UseColor565
                            );
                    } else
                    {
                        textBox1.Text = r.ToArduinoCode(
                            dataInputModel.Name ?? "IMAGE",
                            dataInputModel.UseColor565,
                            dataInputModel.IncludeGetPixel
                            );
                    }
                    
                }
                else
                {
                    textBox1.Text = "Image Not Selected";
                }
            }
            catch (Exception ex)
            {
                textBox1.Text = $"Error: {ex.Message}";
            }
        }
    }
}

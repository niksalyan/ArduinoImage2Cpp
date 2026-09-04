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
                
                if (dataInputModel.Image != null)
                {
                    var r = ArduinoImageConverter.Convert(dataInputModel);
                    
                    if (dataInputModel.ImageBox)
                    {
                        var rb = ArduinoImageBoxConverter.Convert(r, dataInputModel.MaxArraySize);
                        pictureBox1.Image = rb.ToBitmap();
                        textBox1.Text = rb.ToArduinoCode(
                            dataInputModel.Name ?? "IMAGE",
                            dataInputModel.UseColor565,
                            dataInputModel.IncludeRenderFunction
                            );
                    } else
                    {
                        pictureBox1.Image = r.ToBitmap();
                        textBox1.Text = r.ToArduinoCode(
                            dataInputModel.Name ?? "IMAGE",
                            dataInputModel.UseColor565,
                            dataInputModel.IncludeRenderFunction
                            );
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

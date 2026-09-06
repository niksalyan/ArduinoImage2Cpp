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
                            pictureBox1.Image = r.ToBitmap();
                            textBox1.Text = r.ToArduinoCode(dataInputModel);
                            break;
                        case RenderType.ArduinoImageBox:
                            var rb = ArduinoImageBoxConverter.Convert(indexed);
                            pictureBox1.Image = rb.ToBitmap();
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

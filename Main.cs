using Image2Cpp.Models;
using System.Text.Json;
using System.IO;
using System.Linq;

namespace Image2Cpp
{
    public partial class Main : Form
    {
        private DataModel dataInputModel = new DataModel();
        private string lastModelPath = null;
        public Main()
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = dataInputModel;
            propertyGrid1.ExpandAllGridItems();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using var dlg = new SaveFileDialog();
                dlg.Filter = "JSON Files|*.json|All Files|*.*";
                dlg.DefaultExt = "json";
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                var opts = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(dlg.FileName, JsonSerializer.Serialize(dataInputModel, opts));
                lastModelPath = dlg.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using var dlg = new OpenFileDialog();
                dlg.Filter = "JSON Files|*.json|All Files|*.*";
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                string text = File.ReadAllText(dlg.FileName);
                var loaded = JsonSerializer.Deserialize<DataModel>(text);
                if (loaded == null)
                    return;

                loaded.Image = dataInputModel.Image;
                dataInputModel = loaded;
                propertyGrid1.SelectedObject = dataInputModel;
                propertyGrid1.Refresh();
                propertyGrid1.ExpandAllGridItems();
                // trigger preview update
                propertyGrid1_PropertyValueChanged(propertyGrid1, null);
                lastModelPath = dlg.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Removed DTO-based serialization in favor of direct DataModel serialization.

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            try
            {

                if (dataInputModel.Image != null)
                {
                    var indexed = ArduinoImageConverter.Convert(dataInputModel);

                    switch (dataInputModel.RenderMethod)
                    {
                        case RenderType.ArduinoPixel:
                            var r = indexed;
                            pictureBox1.Image = r.ToBitmap(true);
                            textBox1.Text = r.ToArduinoCode(dataInputModel);
                            break;
                        case RenderType.ArduinoImageBox:
                        case RenderType.ArduinoImageBoxFast:
                            var rb = ArduinoImageBoxConverter.Convert(indexed, dataInputModel);
                            pictureBox1.Image = rb.ToBitmap(true);
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

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataInputModel = new DataModel();
            propertyGrid1.SelectedObject = dataInputModel;
            pictureBox1.Image = null;
            textBox1.Text = "";
            propertyGrid1.Refresh();
            propertyGrid1.ExpandAllGridItems();
        }
    }
}

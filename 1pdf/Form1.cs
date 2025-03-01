using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Layout;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Windows.Forms;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using PdfiumViewer;
using iTextPdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfiumPdfDocument = PdfiumViewer.PdfDocument;
using System.Resources;
using System.Globalization;
using System.Threading;

namespace _1pdf
{
    public partial class Form1 : Form
    {
        List<string> fileNames = new List<string>();
        private int totalPages = 0;
        private ResourceManager rm;
        public Form1()
        {
            InitializeComponent();
            rm = new ResourceManager("_1pdf.Properties.Resources", Assembly.GetExecutingAssembly());
            //rm = Properties.Resources.ResourceManager;
            InitializeLanguageSelector();
            ApplyLocalization();
 
            listBox1.AllowDrop = true;
            listBox1.DragEnter += ListBox1_DragEnter;
            listBox1.DragDrop += ListBox1_DragDrop;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
        }


        private void InitializeLanguageSelector()
        {
            comboBoxLanguage.Items.AddRange(new string[] { "English", "Español" });

            // Set the initial selected item based on current culture
            string currentCulture = CultureInfo.CurrentUICulture.Name.ToLower();
            if (currentCulture.StartsWith("es"))
            {
                comboBoxLanguage.SelectedIndex = 1; // Spanish
            }
            else
            {
                comboBoxLanguage.SelectedIndex = 0; // English (default)
            }

            // Add the event handler for selection changes
            comboBoxLanguage.SelectedIndexChanged += (s, e) =>
            {
                string culture = comboBoxLanguage.SelectedIndex == 0 ? "en-US" : "es-ES";
                ChangeLanguage(culture);
            };
        }

        private void ApplyLocalization()
        {
            try
            {
                // Get culture based on ComboBoxLanguage selection
                CultureInfo currentCulture;
                if (comboBoxLanguage.SelectedIndex == 1) // Spanish
                {
                    currentCulture = new CultureInfo("es-ES");
                }
                else // English or default
                {
                    currentCulture = new CultureInfo("en-US");
                }

                // Set the current thread culture
                Thread.CurrentThread.CurrentUICulture = currentCulture;
                Thread.CurrentThread.CurrentCulture = currentCulture;

                // Update form labels
                btnRemove.Text = rm.GetString("ButtonRemove", currentCulture);
                btnArchivo.Text = rm.GetString("ButtonFile", currentCulture);
                btnArriba.Text = rm.GetString("ButtonUp", currentCulture);
                btnAbajo.Text = rm.GetString("ButtonDown", currentCulture);
                lblNombre.Text = rm.GetString("LabelName", currentCulture);
                btnSelectFiles.Text = rm.GetString("ButtonSelectFiles", currentCulture);
                label1.Text = rm.GetString("LabelPages", currentCulture);
                btnGenerar.Text = rm.GetString("ButtonGenerate", currentCulture);

                // Update messages
                string noFileSelectedMessage = rm.GetString("NoFileSelected", currentCulture);
                string fileCreatedMessage = rm.GetString("FileCreatedSuccess", currentCulture);
                string fileErrorMessage = rm.GetString("FileCreationError", currentCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying localization: " + ex.Message);
            }
        }


        private void ChangeLanguage(string cultureName)
        {
            try
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
                Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);

                // Update ComboBox selection to match the new culture
                if (cultureName.StartsWith("es", StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxLanguage.SelectedIndex = 1; // Spanish
                }
                else
                {
                    comboBoxLanguage.SelectedIndex = 0; // English
                }

                ApplyLocalization();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error changing language: " + ex.Message);
            }
        }


        private void ListBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }

        }

        private void AddFilesToList(string[] files)
        {
            bool containsOnlyPDF = files.All(f => Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase));
            if (containsOnlyPDF)
            {
                foreach (string file in files)
                {
                    fileNames.Add(file);
                    listBox1.Items.Add(file);
                }
            }
        }

        private void HandleDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                AddFilesToList(files);
            }
        }

        // Update existing methods to call the new methods
        private void ListBox1_DragDrop(object sender, DragEventArgs e)
        {
            HandleDragDrop(sender, e);
        }


        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                fileNames.RemoveAt(listBox1.SelectedIndex);
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (listBox1.Items.Count <= 1) return;
            if (string.IsNullOrWhiteSpace(txtnombre.Text))
            {
                MessageBox.Show(
                    rm.GetString("MissingParameterMessage", CultureInfo.CurrentUICulture),
                    rm.GetString("MissingParameterTitle", CultureInfo.CurrentUICulture),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
                return;
            }
            List<string> inputFiles = new List<string>();
            inputFiles.AddRange(listBox1.Items.OfType<string>());

            var outputFile = txtnombre.Text; 

            MergePdfDocuments(inputFiles, outputFile);
            if (File.Exists(outputFile))
            {
                MessageBox.Show(
                    string.Format(rm.GetString("FileCreatedSuccess", CultureInfo.CurrentUICulture), outputFile),
                    rm.GetString("SuccessTitle", CultureInfo.CurrentUICulture),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    string.Format(rm.GetString("FileCreationError", CultureInfo.CurrentUICulture), outputFile),
                    rm.GetString("ErrorTitle", CultureInfo.CurrentUICulture),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void MergePdfDocuments(List<string> inputFiles, string outputFile)
        {
            using (var writer = new PdfWriter(outputFile))
            {
                using (var document = new iTextPdfDocument(writer))
                {
                    var merger = new PdfMerger(document);

                    foreach (var inputFile in inputFiles)
                    {
                        using (var reader = new PdfReader(inputFile))
                        {
                            var sourceDocument = new iTextPdfDocument(reader);
                            merger.Merge(sourceDocument, 1, sourceDocument.GetNumberOfPages());
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string fileName in openFileDialog.FileNames)
                    {
                        if (!fileNames.Contains(fileName))
                        {
                            fileNames.Add(fileName);
                            listBox1.Items.Add(fileName);
                        }
                    }

                }
            }
        }

        private void btnArchivo_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo PDF (*.pdf)|*.pdf";
            saveFileDialog.Title = "Guardar archivo PDF";
            saveFileDialog.FileName = "DocumentoCompleto.pdf";
            DialogResult result = saveFileDialog.ShowDialog();

            // Si el usuario hizo clic en Aceptar en el cuadro de diálogo, guardar el archivo
            if (result == DialogResult.OK)
            {
                txtnombre.Text = saveFileDialog.FileName;
            }
        }

        private void btnArriba_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index > 0)
            {
                string temp = fileNames[index - 1];
                fileNames[index - 1] = fileNames[index];
                fileNames[index] = temp;
                listBox1.Items.Clear();
                foreach (string file in fileNames)
                {
                    listBox1.Items.Add(file);
                }
                listBox1.SelectedIndex = index - 1;
            }
        }

        private void btnAbajo_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < listBox1.Items.Count - 1 && index != -1)
            {
                string temp = fileNames[index + 1];
                fileNames[index + 1] = fileNames[index];
                fileNames[index] = temp;
                listBox1.Items.Clear();
                foreach (string file in fileNames)
                {
                    listBox1.Items.Add(file);
                }
                listBox1.SelectedIndex = index + 1;
            }
        }

        private void lblNombre_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count <= 1)
            {
                string sNotFileSelected = rm.GetString("NotSelected", CultureInfo.CurrentUICulture);
                string sTitleNFS =rm.GetString("ErrorNotSelected", CultureInfo.CurrentUICulture);
                MessageBox.Show(sNotFileSelected, sTitleNFS, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            string cPath = Path.GetDirectoryName(listBox1.Items[0].ToString());
            string cName = Path.GetFileName(listBox1.Items[0].ToString());
            txtnombre.Text = cPath + @"\_1PDF-" + cName;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedFile = listBox1.SelectedItem.ToString();
                PreviewPdf(selectedFile, 1); // Previsualizar la primera página
            }
        }

        private void numericUpDownPage_ValueChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedFile = listBox1.SelectedItem.ToString();
                int pageNumber = (int)numericUpDownPage.Value;
                PreviewPdf(selectedFile, pageNumber);
            }
        }

        private void PreviewPdf(string filePath, int pageNumber)
        {
            using (var document = PdfiumViewer.PdfDocument.Load(filePath))
            {
                totalPages = document.PageCount; // Actualiza el número total de páginas
                numericUpDownPage.Maximum = totalPages; // Ajusta el máximo del NumericUpDown
                string sInvalidPage = rm.GetString("TextInvalidPage", CultureInfo.CurrentUICulture);

                if (pageNumber > totalPages || pageNumber < 1)
                {
                    MessageBox.Show(sInvalidPage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var image = document.Render(pageNumber - 1, pictureBox1.Width, pictureBox1.Height, true);
                pictureBox1.Image = image;
            }
        }

        private class MyImageRenderListener : IEventListener
        {
            public static Image Image { get; private set; }

            public void EventOccurred(IEventData data, EventType eventType)
            {
                if (eventType == EventType.RENDER_IMAGE)
                {
                    var renderInfo = (ImageRenderInfo)data;
                    var image = renderInfo.GetImage();
                    var imgBytes = image.GetImageBytes();
                    using (var ms = new MemoryStream(imgBytes))
                    {
                        Image = Image.FromStream(ms);
                    }
                }
            }

            public ICollection<EventType> GetSupportedEvents()
            {
                return new List<EventType> { EventType.RENDER_IMAGE };
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                string sNotSelected = rm.GetString("TextSelectPreview", CultureInfo.CurrentUICulture);
                using (Font myFont = new Font("Arial", 14))
                {
                    e.Graphics.DrawString(sNotSelected, myFont, Brushes.Black, new Point(2, 2));
                }
            }
        }
    }
}

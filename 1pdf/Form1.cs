using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Layout;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Forms;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using PdfiumViewer;
using iTextPdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfiumPdfDocument = PdfiumViewer.PdfDocument;

namespace _1pdf
{
    public partial class Form1 : Form
    {
        List<string> fileNames = new List<string>();
        private int totalPages = 0;
        public Form1()
        {
            InitializeComponent();
            listBox1.AllowDrop = true;
            listBox1.DragEnter += ListBox1_DragEnter;
            listBox1.DragDrop += ListBox1_DragDrop;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
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

            if (listBox1.Items.Count == 0) return;
            if (string.IsNullOrWhiteSpace(txtnombre.Text))
            {
                MessageBox.Show("Debe escoger el nombre del archivo a generar.", "Falta un parámetro para proceder:", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            List<string> inputFiles = new List<string>();
            inputFiles.AddRange(listBox1.Items.OfType<string>());

            var outputFile = txtnombre.Text; 

            MergePdfDocuments(inputFiles, outputFile);
            if (File.Exists(outputFile))
            {
                MessageBox.Show("El archivo " + outputFile + " ha sido creado con éxito.", "Proceso finalizado de forma correcta.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("El archivo " + outputFile + " no pudo ser creado.", "Proceso con errores.", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("Debe tener al menos 1 archivo en la lista de PDFs.", "Falta un parámetro para proceder:", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            string cPath = Path.GetDirectoryName(listBox1.Items[0].ToString());
            string cName = Path.GetFileName(listBox1.Items[0].ToString());
            txtnombre.Text = cPath + @"\Unido-" + cName;
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

                if (pageNumber > totalPages || pageNumber < 1)
                {
                    MessageBox.Show("Número de página inválido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                using (Font myFont = new Font("Arial", 14))
                {
                    e.Graphics.DrawString("Seleccione un PDF para vista previa", myFont, Brushes.Black, new Point(2, 2));
                }
            }
        }
    }
}

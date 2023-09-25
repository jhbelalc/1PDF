using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Forms;

namespace _1pdf
{
    public partial class Form1 : Form
    {
        List<string> fileNames = new List<string>();
        public Form1()
        {
            InitializeComponent();
            listBox1.AllowDrop = true;
            listBox1.DragEnter += ListBox1_DragEnter;
            listBox1.DragDrop += ListBox1_DragDrop;
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

        private void ListBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
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

            var outputFile = txtnombre.Text; //@"C:\Users\john.belalcazar\Documents\Kantar\Viaje y Alojamiento\Marzo 2023\MED CO-FO-FIN- 0004 - v.3  Legalizacion gastos de Personal.pdf";

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
                using (var document = new PdfDocument(writer))
                {
                    var merger = new PdfMerger(document);

                    foreach (var inputFile in inputFiles)
                    {
                        using (var reader = new PdfReader(inputFile))
                        {
                            var sourceDocument = new PdfDocument(reader);
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
    }
}

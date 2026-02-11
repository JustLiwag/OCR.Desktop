using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;


namespace OCR.Desktop
{
    public partial class OcrApp : Form
    {

        private string _selectedPdf;
        private readonly HttpClient _http = new HttpClient();

        public OcrApp()
        {
            InitializeComponent();
        }

        private void btnSelectPdf_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "PDF files (*.pdf)|*.pdf";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedPdf = ofd.FileName;
                    lblFile.Text = Path.GetFileName(_selectedPdf);
                }
            }
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedPdf))
            {
                MessageBox.Show("Select a PDF first.");
                return;
            }

            btnUpload.Enabled = false;
            progressBar1.Style = ProgressBarStyle.Marquee;
            searchBox.AppendText("Uploading...\r\n");

            try
            {
                using (var form = new MultipartFormDataContent())
                using (var fs = File.OpenRead(_selectedPdf))
                {
                    var fileContent = new StreamContent(fs);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");

                    form.Add(fileContent, "file", Path.GetFileName(_selectedPdf));

                    var response = await _http.PostAsync(
                        "https://localhost:7080/api/ocr/manual",
                        form
                    );

                    response.EnsureSuccessStatusCode();

                    var outputPdfBytes = await response.Content.ReadAsByteArrayAsync();

                    var savePath = Path.Combine(
                        Path.GetDirectoryName(_selectedPdf),
                        Path.GetFileNameWithoutExtension(_selectedPdf) + "_OCR.pdf"
                    );

                    File.WriteAllBytes(savePath, outputPdfBytes);

                    searchBox.AppendText("OCR complete!\r\nSaved: " + savePath + "\r\n");
                    MessageBox.Show("OCR completed!");
                }
            }
            catch (Exception ex)
            {
                searchBox.AppendText("ERROR: " + ex.Message + "\r\n");
                MessageBox.Show(ex.Message);
            }
            finally
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                btnUpload.Enabled = true;
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            
        }

        //private void btnUpload_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        using (OpenFileDialog ofd = new OpenFileDialog())
        //        {
        //            ofd.Title = "Select a file to preview";
        //            ofd.Filter = "All Files|*.*|Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Text Files|*.txt";
        //            ofd.FilterIndex = 1;
        //            ofd.Multiselect = false;

        //            if (ofd.ShowDialog() == DialogResult.OK)
        //            {
        //                string filePath = ofd.FileName;
        //                lblFileName.Text = Path.GetFileName(filePath);

        //                // Preview based on file type
        //                string ext = Path.GetExtension(filePath).ToLower();
        //                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif" || ext == ".webp")
        //                {
        //                    pictureBoxPreview.Visible = true;
        //                    textBoxPreview.Visible = false;
        //                    pictureBoxPreview.Image = System.Drawing.Image.FromFile(filePath);
        //                }
        //                else if (ext == ".txt")
        //                {
        //                    pictureBoxPreview.Visible = false;
        //                    textBoxPreview.Visible = true;
        //                    textBoxPreview.Text = File.ReadAllText(filePath);
        //                }
        //                else
        //                {
        //                    pictureBoxPreview.Visible = false;
        //                    textBoxPreview.Visible = true;
        //                    textBoxPreview.Text = "Preview not supported for this file type.";
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error loading file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}
    }
}

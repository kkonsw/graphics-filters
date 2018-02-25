using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace graphics_lab1_filters
{
    public partial class mainForm : Form
    {
        Bitmap image;

        public mainForm()
        {
            InitializeComponent();
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All Files (*.*) | *.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
            }

            pictureBox.Image = image;
            pictureBox.Refresh();
        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InvertFilter filter= new InvertFilter();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker);
            if (backgroundWorker.CancellationPending != true)
                image = newImage;
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox.Image = image;
                pictureBox.Refresh();
            }
            progressBar.Value = 0;
        }

        private void button_Click(object sender, EventArgs e)
        {
            backgroundWorker.CancelAsync();
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BlurFilter filter = new BlurFilter();
            backgroundWorker.RunWorkerAsync(filter);
        }
    }
}

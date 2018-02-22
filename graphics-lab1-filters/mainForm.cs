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
            Bitmap resultImage = filter.processImage(image);
            image = resultImage;
            pictureBox.Image = resultImage;
            pictureBox.Refresh();
        }
    }
}

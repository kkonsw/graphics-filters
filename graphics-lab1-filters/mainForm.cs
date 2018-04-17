using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace graphics_lab1_filters
{
    public partial class mainForm : Form
    {
        Bitmap image;
        float[,] mask;
        static int n;
        Form Form2;
        DataGridView DataGrid1;
        bool flag = false;

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

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PNG Image|*.png|JPG Image|*.jpg|BMP Image|*.bmp";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image.Save(dialog.FileName);
            }
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).ProcessImage(image, backgroundWorker);
            if (backgroundWorker.CancellationPending != true)
                image = newImage;
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox.Image = image;
                pictureBox.Refresh();
            }
            progressBar.Value = 0;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            backgroundWorker.CancelAsync();
        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new InvertFilter();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void фильтрToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void чернобелыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScaleFilter();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void линейноеРастяжениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new AutoContrast(image);
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sepia();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void яркостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Brightness();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sharpness();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayWorld(image);
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void медианныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Median();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void расширениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (flag)
            {
                Filters filter = new Dilation(mask);
                backgroundWorker.RunWorkerAsync(filter);
            }
            else
            {
                Filters filter = new Dilation();
                backgroundWorker.RunWorkerAsync(filter);
            }
        }

        private void сужениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (flag)
            {
                Filters filter = new Erosion(mask);
                backgroundWorker.RunWorkerAsync(filter);
            }
            else
            {
                Filters filter = new Erosion();
                backgroundWorker.RunWorkerAsync(filter);
            }
        }

        private void открытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Opening();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void закрытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Closing();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void gradToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Grad();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void фильтрСобеляToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sobel();
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void motionBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MotionBlur(3);
            backgroundWorker.RunWorkerAsync(filter);
        }

        private void x3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            n = 3;
            mask = new float[n, n];
            Form2 = new Form();
            Form2.Size = new Size(500, 550);
            Form2.Text = "Структурный элемент";
            Form2.Show();
            DataGrid1 = new DataGridView();
            DataGrid1.Size = new Size(300, 300);
            DataGrid1.ColumnCount = n;
            DataGrid1.RowCount = n;
            for (int i = 0; i < n; i++)
            {
                DataGrid1.Columns[i].Width = 50;
                DataGrid1.Rows[i].Height = 25;
            }
            Form2.Controls.Add(DataGrid1);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    DataGrid1[i, j].Value = 0;
            DataGrid1.Show();
            Button ok = new Button();
            ok.Size = new Size(80, 50);
            ok.Text = "ok";
            ok.Location = new Point(200, 305);
            ok.Click += new EventHandler(button_click);
            Form2.Controls.Add(ok);
            ok.Show();
        }

        public void button_click(object sender, EventArgs e)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (DataGrid1[i, j].Value.ToString() == "0")
                        mask[i, j] = 0;
                    else
                        mask[i, j] = 1;
                }
            }
            Form2.Close();
            MessageBox.Show("Ок");
            flag = true;
        }

        private void опорныйЭлементToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int x = Convert.ToInt32(textBox1.Text);
            int y = Convert.ToInt32(textBox2.Text);
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Color color = colorDialog1.Color;
                Filters filter = new ColorBased(color, x, y, image);
                backgroundWorker.RunWorkerAsync(filter);
            }       
        }
    }
}

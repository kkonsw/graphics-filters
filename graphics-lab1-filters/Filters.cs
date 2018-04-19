using System;
using System.Drawing;
using System.ComponentModel;

namespace graphics_lab1_filters
{
    abstract class Filters
    {
        protected abstract Color CalculateNewPixelColor(Bitmap sourceImage, int x, int y);

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public virtual Bitmap ProcessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, CalculateNewPixelColor(sourceImage, i, j));
                }
            }

            return resultImage;
        }
    }

    class InvertFilter : Filters
    {
        protected override Color CalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R,
                                               255 - sourceColor.G,
                                               255 - sourceColor.B);

            return resultColor;
        }
    }

    class GrayScaleFilter : Filters
    {
        protected override Color CalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int Intensity = (int)(0.36f * sourceColor.R + 0.53f * sourceColor.G + 0.11f * sourceColor.B);

            Color resultColor = Color.FromArgb(Intensity, Intensity, Intensity);
            return resultColor;
        }
    }

    class Sepia : Filters
    {
        protected override Color CalculateNewPixelColor(Bitmap Image, int x, int y)
        {
            float k = 20f;
            Color src = Image.GetPixel(x, y);
            int Intensity = (int)(0.36 * src.R + 0.53 * src.G + 0.11 * src.B);
            return Color.FromArgb
                (Clamp((int)(Intensity + 2 * k), 0, 255), Clamp((int)(Intensity + 0.5 * k), 0, 255),
                 Clamp((int)(Intensity - k), 0, 255));
        }
    }

    class Brightness : Filters
    {
        protected override Color CalculateNewPixelColor(Bitmap Image, int x, int y)
        {
            int k = 15;
            Color src = Image.GetPixel(x, y);
            return Color.FromArgb(Clamp(src.R + k, 0, 255), Clamp(src.G + k, 0, 255),
                                  Clamp(src.B + k, 0, 255));
        }
    }

    class AutoContrast : Filters
    {
        int r_max = 0, r_min = 0, g_max = 0, g_min = 0, b_max = 0, b_min = 0;

        public AutoContrast(Bitmap src_img)
        {
            for (int i = 0; i < src_img.Width; ++i)
                for (int j = 0; j < src_img.Height; ++j)
                {
                    r_max = Math.Max(r_max, src_img.GetPixel(i, j).R);
                    g_max = Math.Max(g_max, src_img.GetPixel(i, j).G);
                    b_max = Math.Max(b_max, src_img.GetPixel(i, j).B);
                    r_min = Math.Min(r_min, src_img.GetPixel(i, j).R);
                    g_min = Math.Min(g_min, src_img.GetPixel(i, j).G);
                    b_min = Math.Min(b_min, src_img.GetPixel(i, j).B);
                }
        }

        protected override Color CalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color src = sourceImage.GetPixel(x, y);
            return Color.FromArgb(
                Clamp((src.R - r_min) * 255 / (r_max - r_min), 0, 255),
                Clamp((src.G - g_min) * 255 / (g_max - g_min), 0, 255),
                Clamp((src.B - b_min) * 255 / (b_max - b_min), 0, 255));
        }
    }

    class GrayWorld : Filters
    {
        float avg_r = 0f, avg_g = 0f, avg_b = 0f;

        public GrayWorld(Bitmap src_img)
        {
            for (int i = 0; i < src_img.Width; ++i)
                for (int j = 0; j < src_img.Height; ++j)
                {
                    avg_r += src_img.GetPixel(i, j).R;
                    avg_g += src_img.GetPixel(i, j).G;
                    avg_b += src_img.GetPixel(i, j).B;
                }
            int n = src_img.Width * src_img.Height;
            avg_r /= n;
            avg_g /= n;
            avg_b /= n;
        }
        protected override Color CalculateNewPixelColor(Bitmap src_img, int x, int y)
        {
            float avg = (avg_r + avg_g + avg_r) / 3;
            Color scr = src_img.GetPixel(x, y);
            return Color.FromArgb(
                Clamp((int)(scr.R * avg / avg_r), 0, 255),
                Clamp((int)(scr.G * avg / avg_g), 0, 255),
                Clamp((int)(scr.B * avg / avg_b), 0, 255));
        }
    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;

        protected MatrixFilter() { }
        protected MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }

        protected override Color CalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }

            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    }

    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void CreateGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;

            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }

        public GaussianFilter()
        {
            CreateGaussianKernel(3, 2);
        }
    }

    class Sharpness : MatrixFilter
    {
        public Sharpness()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            kernel[0, 0] = -1; kernel[0, 1] = -1; kernel[0, 2] = -1;
            kernel[1, 0] = -1; kernel[1, 1] = 9; kernel[1, 2] = -1;
            kernel[2, 0] = -1; kernel[2, 1] = -1; kernel[2, 2] = -1;
        }
    }

    class Sobel : MatrixFilter
    {
        protected override Color CalculateNewPixelColor(Bitmap src, int x, int y)
        {
            kernel = new float[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            Color tmp_x = base.CalculateNewPixelColor(src, x, y);
            kernel = new float[3, 3] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
            Color tmp_y = base.CalculateNewPixelColor(src, x, y);
            return Color.FromArgb(
                Clamp((int)(Math.Sqrt(tmp_x.R * tmp_x.R + tmp_y.R * tmp_y.R)), 0, 255),
                Clamp((int)(Math.Sqrt(tmp_x.G * tmp_x.G + tmp_y.G * tmp_y.G)), 0, 255),
                Clamp((int)(Math.Sqrt(tmp_x.B * tmp_x.B + tmp_y.B * tmp_y.B)), 0, 255));
        }
    }

    class MotionBlur : MatrixFilter
    {
        public MotionBlur(int radius)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = (1f / size);
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    if (i == j)
                        kernel[i + radius, j + radius] = norm;
                    else
                        kernel[i + radius, j + radius] = 0;

                }
            }
        }
    }

    class ColorBased : Filters
    {
        Color color_src;
        Color color_dst;

        public ColorBased(Color color, int x, int y, Bitmap src_img)
        {
            color_dst = color;
            color_src = src_img.GetPixel(x, y);
            src_img.SetPixel(x, y, color_dst);
        }

        protected override Color CalculateNewPixelColor(Bitmap src_img, int x, int y)
        {
            Color src = src_img.GetPixel(x, y);

            return Color.FromArgb(
                Clamp((int)(src.R * color_dst.R / color_src.R), 0, 255),
                Clamp((int)(src.G * color_dst.G / color_src.G), 0, 255),
                Clamp((int)(src.B * color_dst.B / color_src.B), 0, 255));
        }
    }

    class Median : Filters
    {
        protected override Color CalculateNewPixelColor(Bitmap src_img, int x, int y)
        {
            int range = 3;
            int size = (2 * range + 1) * (2 * range + 1);
            int[] tmp_r = new int[size], tmp_g = new int[size], tmp_b = new int[size];

            for (int i = 0, l = -range; l <= range; ++l)
                for (int k = -range; k <= range; ++k)
                {
                    int id_x = Clamp(x + k, 0, src_img.Width - 1);
                    int id_y = Clamp(y + l, 0, src_img.Height - 1);
                    Color neighbor_color = src_img.GetPixel(id_x, id_y);
                    tmp_r[i] = neighbor_color.R;
                    tmp_g[i] = neighbor_color.G;
                    tmp_b[i] = neighbor_color.B;
                    i++;
                }
            Array.Sort(tmp_r);
            Array.Sort(tmp_g);
            Array.Sort(tmp_b);
            return Color.FromArgb(tmp_r[size / 2], tmp_g[size / 2], tmp_b[size / 2]);
        }
    }

    abstract class Morfology : MatrixFilter
    {
        public Morfology()
        {
            kernel = new float[3, 3];
            kernel[0, 0] = 0.0f; kernel[0, 1] = 1.0f; kernel[0, 2] = 0.0f;
            kernel[1, 0] = 1.0f; kernel[1, 1] = 1.0f; kernel[1, 2] = 1.0f;
            kernel[2, 0] = 0.0f; kernel[2, 1] = 1.0f; kernel[2, 2] = 0.0f;
        }

        public Morfology(float[,] kernel)
        {
            this.kernel = kernel;
        }
    }

    class Dilation : Morfology
    {
        public Dilation()
        {
            kernel = new float[3, 3];
            kernel[0, 0] = 0.0f; kernel[0, 1] = 1.0f; kernel[0, 2] = 0.0f;
            kernel[1, 0] = 1.0f; kernel[1, 1] = 1.0f; kernel[1, 2] = 1.0f;
            kernel[2, 0] = 0.0f; kernel[2, 1] = 1.0f; kernel[2, 2] = 0.0f;
        }

        public Dilation(float[,] kernel)
        {
            this.kernel = kernel;
        }

        protected override Color CalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
             int radiusX = kernel.GetLength(0) / 2;
             int radiusY = kernel.GetLength(1) / 2;

             Color resultColor = Color.Black;
             byte max = 0;

             for (int l = -radiusY; l <= radiusY; l++)
                 for (int k = -radiusX; k <= radiusX; k++)
                 {
                     int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                     int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                     Color color = sourceImage.GetPixel(idX, idY);
                     int Intensity = (int)(0.36 * color.R + 0.53 * color.G + 0.11 * color.R);

                     if (kernel[k + radiusX, l + radiusY] > 0 && Intensity > max)
                     {
                         max = (byte)Intensity;
                         resultColor = color;
                     }
                 }

             return resultColor;
        }
    }

    class Erosion : Morfology
    {
        public Erosion()
        {
            kernel = new float[3, 3];
            kernel[0, 0] = 0.0f; kernel[0, 1] = 1.0f; kernel[0, 2] = 0.0f;
            kernel[1, 0] = 1.0f; kernel[1, 1] = 1.0f; kernel[1, 2] = 1.0f;
            kernel[2, 0] = 0.0f; kernel[2, 1] = 1.0f; kernel[2, 2] = 0.0f;
        }

        public Erosion(float[,] kernel)
        {
            this.kernel = kernel;
        }

        protected override Color CalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            Color resultColor = Color.Black;
            byte min = 255;

            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color color = sourceImage.GetPixel(idX, idY);
                    int Intensity = (int)(0.36 * color.R + 0.53 * color.G + 0.11 * color.R);
                   
                    if (kernel[k + radiusX, l + radiusY] > 0 && Intensity < min)
                    {
                        min = (byte)Intensity;
                        resultColor = color;
                    }
                }

            return resultColor;
        }
    }

    class Opening : Morfology
    {
        public override Bitmap ProcessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Dilation dilation = new Dilation();
            Erosion erosion = new Erosion(); 

            return dilation.ProcessImage(erosion.ProcessImage(sourceImage, worker), worker);
        }
    }

    class Closing : Morfology
    {
        public override Bitmap ProcessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Dilation dilation = new Dilation();
            Erosion erosion = new Erosion();
           
            return erosion.ProcessImage(dilation.ProcessImage(sourceImage, worker), worker);
        }
    }

    class Subtraction : Filters
    {
        Bitmap LImage = null;
        public Subtraction(Bitmap Image)
        {
            LImage = Image;
        }
        protected override Color CalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color LColor = LImage.GetPixel(x, y);
            Color RColor = sourceImage.GetPixel(x, y);
            return Color.FromArgb(Clamp(LColor.R - RColor.R, 0, 255),
                                  Clamp(LColor.G - RColor.G, 0, 255),
                                  Clamp(LColor.B - RColor.B, 0, 255));
        }
    }

    class Grad : Filters
    {
        public override Bitmap ProcessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Dilation dilation = new Dilation();
            Erosion erosion = new Erosion();
            Subtraction subtraction = new Subtraction(dilation.ProcessImage(sourceImage, worker));
            return subtraction.ProcessImage(erosion.ProcessImage(sourceImage, worker), worker);
        }

        protected override Color CalculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            throw new NotImplementedException();
        }
    }
}

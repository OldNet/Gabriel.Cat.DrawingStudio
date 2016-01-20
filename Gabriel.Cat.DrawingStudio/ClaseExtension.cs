using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.DrawingStudio;
namespace Gabriel.Cat.Extension
{
   public static class ExtensionBitmap
    {
        delegate byte[] MetodoColor(byte[][] colores);
        #region BitmapImportado
        /// <summary>
        /// Recorta una imagen en formato Bitmap
        /// </summary>
        /// <param name="localizacion">localizacion de la esquina izquierda de arriba</param>
        /// <param name="tamaño">tamaño del rectangulo</param>
        /// <param name="bitmapARecortar">bitmap para recortar</param>
        /// <returns>bitmap resultado del recorte</returns>
        public static Bitmap Recortar(this Bitmap bitmapARecortar, Point localizacion, Size tamaño)
        {

            Rectangle rect = new Rectangle(localizacion.X, localizacion.Y, tamaño.Width, tamaño.Height);
            Bitmap cropped = bitmapARecortar.Clone(rect, bitmapARecortar.PixelFormat);
            return cropped;

        }
        public static Bitmap Escala(this Bitmap imgAEscalar, decimal escala)
        {
            return Resize(imgAEscalar, new Size(Convert.ToInt32(imgAEscalar.Size.Width * escala), Convert.ToInt32(imgAEscalar.Size.Height * escala)));
        }
        public static Bitmap Resize(this Bitmap imgToResize, Size size)
        {
            Bitmap bmpResized;
            try
            {
                bmpResized = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage((System.Drawing.Image)bmpResized))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }

            }
            catch
            {
                bmpResized = imgToResize;
            }

            return bmpResized;
        }


    

        public static Color[,] GetColorMatriu(this Bitmap bmp)
        {
            Color[,] matriz = new Color[bmp.Width, bmp.Height];
            bmp.TrataBytes((arrayBytes) => {
                ulong posicion = 0;
                for (int y = 0, yFinal = bmp.Width; y < yFinal; y++)
                    for (int x = 0, xFinal = bmp.Height; x < xFinal; x++, posicion += 4)
                        matriz[x, y] = Color.FromArgb(arrayBytes[posicion], arrayBytes[posicion + 1], arrayBytes[posicion + 2], arrayBytes[posicion + 3]);

            });
            return matriz;
        }
        public static Bitmap GetBitmap(this Color[,] array)
        {
            Bitmap bmp = new Bitmap(array.GetLength(DimensionMatriz.X), array.GetLength(DimensionMatriz.Y));
            bmp.TrataBytes((arrayBytes) => {
                ulong posicion = 0;
                for (ulong y = 0, yFinal = (ulong)array.GetLongLength((int)DimensionMatriz.Y); y < yFinal; y++)
                    for (ulong x = 0, xFinal = (ulong)array.GetLongLength((int)DimensionMatriz.X); x < xFinal; x++, posicion += 4)
                    {
                        arrayBytes[posicion] = array[x, y].A;
                        arrayBytes[posicion + 1] = array[x, y].R;
                        arrayBytes[posicion + 2] = array[x, y].G;
                        arrayBytes[posicion + 3] = array[x, y].B;
                    }

            });
            return bmp;
        }
        public static byte[,] GetMatriuBytes(this Bitmap bmp)
        {
            byte[] bytesArray = bmp.GetBytes();
            return bytesArray.ToMatriu(bmp.Height, DimensionMatriz.Y);
        }
        public static void SetMatriuBytes(this Bitmap bmp, byte[,] matriuBytes)
        {
            if (bmp.Height * bmp.Width * 3 != matriuBytes.GetLength(DimensionMatriz.Y) * matriuBytes.GetLength(DimensionMatriz.X))
                throw new Exception("La matriz no tiene las medidas de la imagen");

            bmp.TrataBytes((arrayBytes) => {
                ulong posicion = 0;
                for (ulong y = 0, yFinal = (ulong)arrayBytes.GetLongLength((int)DimensionMatriz.Y); y < yFinal; y++)
                    for (ulong x = 0, xFinal = (ulong)arrayBytes.GetLongLength((int)DimensionMatriz.X); x < xFinal; x++)
                    {
                        arrayBytes[posicion++] = matriuBytes[x, y];
                    }


            });

        }

        public static void TrataBytes(this Bitmap bmp, MetodoTratarByteArray metodo)
        {
            BitmapData bmpData = bmp.LockBits();
            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;

            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            ptr.CopyTo(rgbValues);
            if (metodo != null)
            {
                metodo(rgbValues);//se modifican los bytes :D
                                  // Copy the RGB values back to the bitmap
                rgbValues.CopyTo(ptr);
            }
            // Unlock the bits.
            bmp.UnlockBits(bmpData);

        }
        public static unsafe void TrataBytes(this Bitmap bmp, MetodoTratarBytePointer metodo)
        {

            BitmapData bmpData = bmp.LockBits();
            // Get the address of the first line.

            IntPtr ptr = bmpData.Scan0;
            if (metodo != null)
            {
                metodo((byte*)ptr.ToPointer());//se modifican los bytes :D
            }
            // Unlock the bits.
            bmp.UnlockBits(bmpData);

        }
        public static int LengthBytes(this Bitmap bmp)
        {
            int multiplicadorPixel = bmp.IsArgb() ? 4 : 3;
            return bmp.Height * bmp.Width * multiplicadorPixel;
        }
        public static bool IsArgb(this Bitmap bmp)
        {
            bool isArgb = false;
            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    isArgb = true;
                    break;
            }
            return isArgb;
        }


        public static Bitmap ChangeColorCopy(this Bitmap bmp, PixelColors color)
        {
            Bitmap bmpClon = bmp.Clone() as Bitmap;
            ChangeColor(bmpClon, color);
            return bmpClon;
        }
        public static unsafe void ChangeColor(this Bitmap bmp, PixelColors color)
        {
            bmp.TrataBytes((rgbArray) => { ICambiaColor(rgbArray, bmp.IsArgb(), bmp.LengthBytes(), color); });
        }

        private static unsafe void ICambiaColor(byte* rgbImg, bool isArgb, int lenght, PixelColors color)
        {

            byte byteR, byteG, byteB;
            int incremento = 3;
            if (isArgb) incremento++;//me salto el alfa
            for (int i = 0; i < lenght; i += incremento)
            {


                byteR = rgbImg[i + Pixel.R];
                byteG = rgbImg[i + Pixel.G];
                byteB = rgbImg[i + Pixel.B];

                switch (color)
                {
                    case PixelColors.Sepia:
                        Image.IToSepia(ref byteR, ref byteG, ref byteB);
                        break;
                    case PixelColors.Inverted:
                        Image.ToInvertit(ref byteR, ref byteG, ref byteB);
                        break;
                    case PixelColors.GrayScale:
                        Image.ToEscalaDeGrises(ref byteR, ref byteG, ref byteB);
                        break;
                    case PixelColors.Blue:
                        Image.ToAzul(ref byteR, ref byteG, ref byteB);
                        break;
                    case PixelColors.Red:
                        Image.ToRojo(ref byteR, ref byteG, ref byteB);
                        break;
                    case PixelColors.Green:
                        Image.ToVerde(ref byteR, ref byteG, ref byteB);
                        break;


                }
                rgbImg[i + Pixel.R] = byteR;
                rgbImg[i + Pixel.G] = byteG;
                rgbImg[i + Pixel.B] = byteB;

            }

        }

        #endregion
        
        public static void CambiarPixel(this Bitmap bmp,Color aEnontrar,Color aDefinir)
        {
            bmp.CambiarPixel(new KeyValuePair<Color, Color>[] { new KeyValuePair<Color, Color>(aEnontrar, aDefinir)});
        }
        public static void CambiarPixel(this Bitmap bmp,IEnumerable<KeyValuePair<Color,Color>> colorsKeyValue)
        {
            MetodoColor metodo= (colores) => {
                byte[] color = colores[0] != null? colores[0] : colores[1];
                return  color;
            };
            ICambiaPixel(bmp, colorsKeyValue, metodo);
        }
        public static void MezclaPixel(this Bitmap bmp, Color aEnontrar, Color aDefinir)
        {
            bmp.MezclaPixel(new KeyValuePair<Color, Color>[] { new KeyValuePair<Color, Color>(aEnontrar, aDefinir) });
        }
        public static void MezclaPixel(this Bitmap bmp, IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue)
        {
            MetodoColor metodo = (colores) => {
                byte[] colorMezclado=null;
                int aux;
                if (colores[0] != null && colores[1] != null)
                {
                    colorMezclado = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {

                        aux = colores[0][i] + colores[1][i];
                        if (aux > 255) aux = 255;
                        colorMezclado[i] = (byte)aux;
                    }
                }
                else if (colores[0] != null)
                    colorMezclado = colores[0];
                else
                    colorMezclado = colores[1];
                return colorMezclado;
                 };
            ICambiaPixel(bmp, colorsKeyValue, metodo);
        }
        static void ICambiaPixel(Bitmap bmp, IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue, MetodoColor metodo)
        {
            DiccionarioColor diccionario = new DiccionarioColor(colorsKeyValue);
            var array = diccionario.ToArray();
            byte[] color;
            const byte AOPACA= 0xFF;
            int incremento = bmp.IsArgb() ? 4 : 3;
            bmp.TrataBytes((byteArray) =>
            {
                for (int i = 0, iFin = bmp.LengthBytes(); i < iFin; i += incremento)
                {
                    if (incremento == 4)
                    {
                        color = new byte[] { byteArray[i + Pixel.A], byteArray[i + Pixel.R], byteArray[i + Pixel.G], byteArray[i + Pixel.B]};//creo los colores permutados
                    }
                    else
                    {
                        color = new byte[] {  AOPACA, byteArray[i + Pixel.R], byteArray[i + Pixel.G], byteArray[i + Pixel.B] };
                    }
                        color = metodo(new byte[][] { diccionario.ObtenerPrimero(color), color });
                        if (color!=null)
                        {
                            if (incremento == 4)
                            {
                                byteArray[i + Pixel.A] = color[Pixel.A];
                            }

                            byteArray[i + Pixel.R] = color[Pixel.R];
                            byteArray[i + Pixel.G] = color[Pixel.G];
                            byteArray[i + Pixel.B] = color[Pixel.B];
                        }

                }
            });
        }

    }
}

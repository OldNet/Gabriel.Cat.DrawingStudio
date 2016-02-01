using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.DrawingStudio;
using AForge.Imaging.Filters;

namespace Gabriel.Cat.Extension
{
    public static class ExtensionBitmap
    {
        delegate byte[] MetodoColor(byte[] colorValue,byte[] colorKey);
        delegate void MetodoTrataMientoPixel(ref byte r, ref byte g, ref byte b);
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
            bmp.TrataBytes((arrayBytes) =>
            {
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
            bmp.TrataBytes((arrayBytes) =>
            {
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

            bmp.TrataBytes((arrayBytes) =>
            {
                ulong posicion = 0;
                for (ulong y = 0, yFinal = (ulong)arrayBytes.GetLongLength((int)DimensionMatriz.Y); y < yFinal; y++)
                    for (ulong x = 0, xFinal = (ulong)arrayBytes.GetLongLength((int)DimensionMatriz.X); x < xFinal; x++)
                    {
                        arrayBytes[posicion++] = matriuBytes[x, y];
                    }


            });

        }


        public static unsafe Bitmap ChangeColor(this Bitmap bmp, PixelColors color)
        {
            IFilter filtro=null;

            switch(color)
            {
                case PixelColors.Red:
                    filtro = new AForge.Imaging.Filters.ChannelFiltering(new AForge.IntRange(0, 255), new AForge.IntRange(0, 0), new AForge.IntRange(0, 0)); break;
                case PixelColors.Green:
                    filtro = new AForge.Imaging.Filters.ChannelFiltering(new AForge.IntRange(0, 0), new AForge.IntRange(0, 255), new AForge.IntRange(0, 0)); break;
                case PixelColors.Blue:
                    filtro = new AForge.Imaging.Filters.ChannelFiltering(new AForge.IntRange(0, 0), new AForge.IntRange(0, 0), new AForge.IntRange(0, 255)); break;
                case PixelColors.Sepia:
                    filtro = new AForge.Imaging.Filters.Sepia(); break;
                case PixelColors.GrayScale:
                    filtro =  AForge.Imaging.Filters.Grayscale.CommonAlgorithms.BT709; break;
                case PixelColors.Inverted:
                    filtro =new AForge.Imaging.Filters.Invert(); break;
            } 
            return filtro.Apply(bmp); 
        }
     
        public static Bitmap Clone(this Bitmap bmp,PixelFormat format)
        {
            return bmp.Clone(new Rectangle(new Point(), bmp.Size), format);
        }
        #endregion

        public static void CambiarPixel(this Bitmap bmp, Color aEnontrar, Color aDefinir)
        {
            bmp.CambiarPixel(new KeyValuePair<Color, Color>[] { new KeyValuePair<Color, Color>(aEnontrar, aDefinir) });
        }
        public static void CambiarPixel(this Bitmap bmp, IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue)
        {
            MetodoColor metodo = (colorValue,colorKey) =>
            {
                return colorValue;
            };
            ICambiaPixel(bmp, colorsKeyValue, metodo);
        }
        public static void EfectoPixel(this Bitmap bmp, Color aMezclarConTodos,bool saltarsePixelsTransparentes=true)
        {
            int incremento = bmp.IsArgb() ? 4 : 3;
            int aux;
            bool mezclar = true;
            const byte TRANSPARENTE = 0x00;
            bmp.TrataBytes((byteArray) =>
            {
                for (int i = 0, iFinal = bmp.LengthBytes(); i < iFinal; i += incremento)
                {
                    if (incremento == 4)
                    {
                        if(saltarsePixelsTransparentes)
                           mezclar = byteArray[i + Pixel.A] != TRANSPARENTE;
                        if (mezclar)
                        {
                            //MEZCLO LA A
                            aux = byteArray[i + Pixel.A] + aMezclarConTodos.A;
                            if (aux > 255) aux = 255;
                            byteArray[i + Pixel.A] = (byte)aux;
                        }
                    }
                    if (mezclar)
                    {
                        //MEZCLO LA R
                        aux = byteArray[i + Pixel.R] + aMezclarConTodos.R;
                        if (aux > 255) aux = 255;
                        byteArray[i + Pixel.R] = (byte)aux;
                        //MEZCLO LA G
                        aux = byteArray[i + Pixel.G] + aMezclarConTodos.G;
                        if (aux > 255) aux = 255;
                        byteArray[i + Pixel.G] = (byte)aux;
                        //MEZCLO LA B
                        aux = byteArray[i + Pixel.B] + aMezclarConTodos.B;
                        if (aux > 255) aux = 255;
                        byteArray[i + Pixel.B] = (byte)aux;
                    }
                }
            });
        }
        public static void MezclaPixel(this Bitmap bmp, Color aEnontrar, Color aDefinir)
        {
            bmp.MezclaPixel(new KeyValuePair<Color, Color>[] { new KeyValuePair<Color, Color>(aEnontrar, aDefinir) });
        }
        public static void MezclaPixel(this Bitmap bmp, IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue)
        {
            MetodoColor metodo = (colorValue, arrayKey) =>
            {
                byte[] colorMezclado = null;
                int aux;
                if (colorValue != null && arrayKey != null)
                {
                    colorMezclado = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {

                        aux = colorValue[i] + arrayKey[i];
                        aux /= 2;
                        colorMezclado[i] =(byte) aux;
                        //if (aux[i] > 255) aux[i] = 255;

                    }
                    
                }
             /*   else if (colorValue != null)
                    colorMezclado = colorValue;
                else
                    colorMezclado =Color.FromArgb(Serializar.ToInt( arrayKey));*/
                return colorMezclado;
            };
            ICambiaPixel(bmp, colorsKeyValue, metodo);
        }
        static void ICambiaPixel(Bitmap bmp, IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue, MetodoColor metodo)
        {
            DiccionarioColor2 diccionario = new DiccionarioColor2(colorsKeyValue);
            byte[] colorLeido;
            byte[] colorObtenido;
            const byte AOPACA = 0xFF;
            int incremento = bmp.IsArgb() ? 4 : 3;
            bmp.TrataBytes((byteArray) =>
            {
                for (int i = 0, iFin = bmp.LengthBytes(); i < iFin; i += incremento)
                {
                    colorLeido = new byte[] { AOPACA, byteArray[i + Pixel.R], byteArray[i + Pixel.G], byteArray[i + Pixel.B] };
                    if (incremento == 4)
                    {
                        colorLeido[Pixel.A] = byteArray[i + Pixel.A];
                    }
                    colorObtenido = metodo(diccionario.ObtenerPrimero(colorLeido), colorLeido);
                    if (colorObtenido != null)
                    {

                        if (incremento == 4)
                        {
                            byteArray[i + Pixel.A] = colorObtenido[0];
                        }

                        byteArray[i + Pixel.R] = colorObtenido[1];
                        byteArray[i + Pixel.G] = colorObtenido[2];
                        byteArray[i + Pixel.B] = colorObtenido[3];
                    }

                }
            });
        }

    }
}

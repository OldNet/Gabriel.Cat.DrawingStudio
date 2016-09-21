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
            ulong posicion = 0;
            byte a, r, g, b;
            unsafe
            {
                bmp.TrataBytes(((MetodoTratarBytePointer)((ptrBytesBmp) =>
                {
                   
                    byte* ptBytesBmp = ptrBytesBmp;
                    for (int y = 0, yFinal = bmp.Width; y < yFinal; y++)
                        for (int x = 0, xFinal = bmp.Height; x < xFinal; x++, posicion += 4)
                        {
                            a = *ptBytesBmp;
                            ptBytesBmp++;
                            r = *ptBytesBmp;
                            ptBytesBmp++;
                            g = *ptBytesBmp;
                            ptBytesBmp++;
                            b = *ptBytesBmp;
                            ptBytesBmp++;
                            matriz[x, y] = Color.FromArgb(a, r, g, b);
                        }

                })));
            }
            return matriz;
        }
        public static Bitmap GetBitmap(this Color[,] array)
        {
            Bitmap bmp = new Bitmap(array.GetLength(DimensionMatriz.X), array.GetLength(DimensionMatriz.Y));
            unsafe
            {
                bmp.TrataBytes(((MetodoTratarBytePointer)((ptrBytesBmp) =>
                {
                    ulong posicion = 0;
                    byte* ptBytesBmp = ptrBytesBmp;
                    for (ulong y = 0, yFinal = (ulong)array.GetLongLength((int)DimensionMatriz.Y); y < yFinal; y++)
                        for (ulong x = 0, xFinal = (ulong)array.GetLongLength((int)DimensionMatriz.X); x < xFinal; x++, posicion += 4)
                        {
                            *ptBytesBmp = array[x, y].A;
                            ptBytesBmp++;
                            *ptBytesBmp = array[x, y].R;
                            ptBytesBmp++;
                            *ptBytesBmp = array[x, y].G;
                            ptBytesBmp++;
                            *ptBytesBmp = array[x, y].B;
                            ptBytesBmp++;
                        }

                })));
            }
            return bmp;
        }
        public static byte[,] GetMatriuBytes(this Bitmap bmp)
        {
            byte[] bytesArray = bmp.GetBytes();
            return bytesArray.ToMatriu(bmp.Height, DimensionMatriz.Y);
        }
        public static void SetMatriuBytes(this Bitmap bmp, byte[,] matriuBytes)
        {
            if (bmp.Height * bmp.Width * (bmp.IsArgb()?4:3) != matriuBytes.GetLength(DimensionMatriz.Y) * matriuBytes.GetLength(DimensionMatriz.X))
                throw new Exception("La matriz no tiene las medidas de la imagen");
            unsafe
            {
                bmp.TrataBytes(((MetodoTratarBytePointer)((ptrBytesBmp) =>
                {
                    byte* ptBytesBmp = ptrBytesBmp;
                    for (long y = 0, yFinal = matriuBytes.GetLongLength((int)DimensionMatriz.Y); y < yFinal; y++)
                        for (long x = 0, xFinal = matriuBytes.GetLongLength((int)DimensionMatriz.X); x < xFinal; x++)
                        {
                            *ptBytesBmp = matriuBytes[x, y];
                             ptBytesBmp++;
                        }


                })));
            }
        }


        public static  Bitmap ChangeColor(this Bitmap bmp, PixelColors color)
        {
            MetodoTrataMientoPixel metodo = null;
            bool esArgb = bmp.IsArgb();
            int incremento = esArgb ? 4 : 3;
           
            Bitmap bmpResultado = bmp.Clone() as Bitmap;
            byte r, g, b;
            switch (color)
            {
                case PixelColors.Red:
                    metodo = Image.ToRojo;break;
                case PixelColors.Green:
                    metodo = Image.ToVerde; break;
                case PixelColors.Blue:
                    metodo = Image.ToAzul; break;
                case PixelColors.Sepia:
                    metodo = Image.IToSepia; break;
                case PixelColors.GrayScale:
                    metodo = Image.ToEscalaDeGrises; break;
                case PixelColors.Inverted:
                    metodo = Image.ToInvertido; break;
            }
            unsafe {
                
                bmpResultado.TrataBytes((MetodoTratarBytePointer)((ptrBytesBmp) =>
                {
                    byte* ptBytesBmp = ptrBytesBmp;
                    //aplico el filtro
                    for(int i=0,f=bmp.Height*bmp.Width*incremento;i<f;i+=incremento)
                    {
                        if (esArgb)//me salto el componente Alfa
                            ptBytesBmp++;
                        r = *ptBytesBmp;
                        ptBytesBmp++;
                        g = *ptBytesBmp;
                        ptBytesBmp++;
                        b = *ptBytesBmp;
                        ptBytesBmp-=2;//lo reseteo para poderlo modificar
                        metodo(ref r,ref g,ref b);
                        *ptBytesBmp = r;
                        ptBytesBmp++;
                        *ptBytesBmp =g;
                        ptBytesBmp++;
                        *ptBytesBmp = b;
                        ptBytesBmp++;
                    }
                }));
            }
            return bmpResultado;
           
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
            const int TOTALARGBBYTES = 4;
            int incremento = bmp.IsArgb() ? 4 : 3;
            int aux;
            bool mezclar = true;
            const byte TRANSPARENTE = 0x00;
            unsafe
            {
                bmp.TrataBytes(((MetodoTratarBytePointer)((ptrbyteArray) =>
                {
                    byte* ptByteArray = ptrbyteArray;
                    for (int i = 0, iFinal = bmp.LengthBytes(); i < iFinal; i += incremento)
                    {
                        if (incremento == TOTALARGBBYTES)
                        {
                            if (saltarsePixelsTransparentes)
                                mezclar = *ptByteArray != TRANSPARENTE;
                            if (mezclar)
                            {
                            //MEZCLO LA A
                            aux = *ptByteArray + aMezclarConTodos.A;
                                if (aux > 255) aux = 255;
                                *ptByteArray = (byte)aux;
                                ptByteArray++;
                            }
                        }
                        if (mezclar)
                        {
                        //MEZCLO LA R
                        aux = *ptByteArray + aMezclarConTodos.R;
                            if (aux > 255) aux = 255;
                            *ptByteArray = (byte)aux;
                            ptByteArray++;
                            //MEZCLO LA G
                            aux = *ptByteArray + aMezclarConTodos.G;
                            if (aux > 255) aux = 255;
                            *ptByteArray = (byte)aux;
                            ptByteArray++;
                            //MEZCLO LA B
                            aux = *ptByteArray + aMezclarConTodos.B;
                            if (aux > 255) aux = 255;
                            *ptByteArray = (byte)aux;
                            ptByteArray++;
                        }
                    }
                })));
            }
        }
        public static void MezclaPixel(this Bitmap bmp, Color aEnontrar, Color aDefinir)
        {
            bmp.MezclaPixel(new KeyValuePair<Color, Color>[] { new KeyValuePair<Color, Color>(aEnontrar, aDefinir) });
        }
        public static void MezclaPixel(this Bitmap bmp, IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue)
        {
            MetodoColor metodo = (colorValue, arrayKey) =>
            {
                const int TOTALBYTESCOLOR = 4;
                byte[] colorMezclado = null;
                int aux;
                if (colorValue != null && arrayKey != null)
                {
                    unsafe
                    {
                        colorMezclado = new byte[TOTALBYTESCOLOR];
                        fixed(byte* ptrColorMezclado = colorMezclado, ptrArrayKey=arrayKey)
                        {
                            byte* ptColorMezclado = ptrColorMezclado, ptArrayKey = ptrArrayKey;
                            for (int i = 0; i < TOTALBYTESCOLOR; i++)
                            {

                                aux = colorValue[i] + *ptArrayKey;
                                aux /= 2;
                                *ptColorMezclado = (byte)aux;
                                //if (aux[i] > 255) aux[i] = 255;
                                ptColorMezclado++;
                                ptArrayKey++;

                            }
                        }
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
            const int TOTALBYTESCOLOR = 4;
            DiccionarioColor2 diccionario = new DiccionarioColor2(colorsKeyValue);
            byte[] colorLeido;
            byte[] colorObtenido;
            const byte AOPACA = 0xFF;
            int incremento = bmp.IsArgb() ? 4 : 3;
            unsafe
            {
                bmp.TrataBytes(((MetodoTratarBytePointer)((ptrBytesBmp) =>
                {
                    byte* ptColorLeido, ptColorObtenido;
                    byte* ptBytesBmp = ptrBytesBmp;
                    for (int i = 0, iFin = bmp.LengthBytes(); i < iFin; i += incremento)
                    {
                        colorLeido = new byte[4];
                        fixed(byte* ptrColorLeido=colorLeido)
                        {
                            ptColorLeido = ptrColorLeido;
                            if (incremento == TOTALBYTESCOLOR)
                            {
                                *ptColorLeido = *ptBytesBmp;
                                ptBytesBmp++;
                            }
                            else
                            {
                                *ptColorLeido = AOPACA;
                                 
                            }
                            ptColorLeido++;
                            for (int j = 1; j < incremento; j++)
                            {
                                *ptColorLeido = *ptBytesBmp;
                                ptBytesBmp++;
                                ptColorLeido++;
                            }
                            ptBytesBmp -= incremento;//vuelvo a poner el puntero al principio del color para sobreescribirlo con el nuevo
                        }
                   
                        colorObtenido = metodo(diccionario.ObtenerPrimero(colorLeido), colorLeido);
                        if (colorObtenido != null)
                        {
                            fixed (byte* ptrColorObtenido = colorObtenido)
                            {
                                ptColorObtenido = ptrColorObtenido;
                                for (int j=0;j<incremento;j++)
                                {
                                    *ptBytesBmp = *ptColorObtenido;
                                    ptBytesBmp++;
                                    ptColorObtenido++;
                                }

 
                            }
                        }

                    }
                })));
            }
        }

    }
}

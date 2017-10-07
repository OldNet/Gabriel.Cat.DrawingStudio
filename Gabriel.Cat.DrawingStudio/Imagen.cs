using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Gabriel.Cat.Extension;

namespace Gabriel.Cat
{

    public enum PixelColors//no se si tiene un nombre descriptivo...
    {
        Red,
        Green,
        Blue,
        GrayScale,
        Sepia,
        Inverted
    }
    public class Pixel
    {
        public const int  R = 2, G = 1, B = 0, A = 3;
    }
    public class Collage : IEnumerable<ImageFragment>
    {
        Llista<ImageFragment> fragments;

        public Collage()
        {
            fragments = new Llista<ImageFragment>();
        }
        public Collage(IEnumerable<ImageFragment> imgsCollage) : this()
        {
            fragments.AddRange(imgsCollage);
        }
        public int Count
        {
            get { return fragments.Count; }
        }
        public ImageFragment this[int posicion]
        {
            get { return fragments[posicion]; }
            set { fragments[posicion] = value; }
        }
        public void Add(ImageFragment imgFragment)
        {
            fragments.Add(imgFragment);
        }
        public void Add(IEnumerable<ImageFragment> imgs)
        {
            fragments.AddRange(imgs);
        }
        public ImageFragment Add(Bitmap imatge, PointZ localizacio)
        {
            return Add(imatge, localizacio.X, localizacio.Y, localizacio.Z);
        }

        /// <summary>
        /// Añade una imagen al mosaico
        /// </summary>
        /// <param name="imagen">imagen para poner</param>
        /// <param name="localizacion">localización en la imagen</param>
        /// <param name="capa">produndidad a la que esta</param>
        /// <returns>devuelve null si no lo a podido añadir</returns>
        public ImageFragment Add(Bitmap imatge, Point localizacio, int capa = 0)
        {
            return Add(imatge, localizacio.X, localizacio.Y, capa);
        }
        public ImageFragment Add(Bitmap imagen, int x = 0, int y = 0, int z = 0)
        {
            if (imagen == null)
                throw new ArgumentNullException("imagen","Se necesita una imagen");

            ImageFragment fragment = null;
            PointZ location = new PointZ(x, y, z);
            fragment = new ImageFragment(imagen, location);
            fragments.Add(fragment);


            return fragment;
        }
        public void RemoveAll()
        {
            fragments.Clear();
        }
        public void Remove(ImageFragment fragmento)
        {
            fragments.Remove(fragmento);
        }
        public ImageFragment Remove(int x = 0, int y = 0, int z = 0)
        {
            return Remove(new PointZ(x, y, z));
        }
        public ImageFragment Remove(Point localizacion, int capa = 0)
        {
            return Remove(new PointZ(localizacion.X, localizacion.Y, capa));
        }
        public ImageFragment Remove(PointZ localizacion)
        {
            ImageFragment fragmentoQuitado = GetFragment(localizacion);

            if (fragmentoQuitado != null)
                fragments.Remove(fragmentoQuitado);

            return fragmentoQuitado;
        }
        public ImageFragment GetFragment(PointZ location)
        {
            return GetFragment(location.X, location.Y, location.Z);
        }
        public ImageFragment GetFragment(int x, int y, int z)
        {
            List<ImageFragment> fragmentosCapaZero = new List<ImageFragment>();
            bool acabado = false;
            int pos = 0;
            Rectangle rectangle;
            ImageFragment fragmento = null;

            fragments.Ordena();
            while (pos < this.fragments.Count && !acabado)
            {

                if (this.fragments[pos].Location.Z > z)
                    acabado = true;
                else if (this.fragments[pos].Location.Z == z)
                    fragmentosCapaZero.Add(this.fragments[pos]);
                pos++;
            }
            for (int i = 0; i < fragmentosCapaZero.Count && fragmento == null; i++)
            {
                rectangle = new Rectangle(fragmentosCapaZero[i].Location.X, fragmentosCapaZero[i].Location.Y, fragmentosCapaZero[i].Image.Width, fragmentosCapaZero[i].Image.Height);
                if (rectangle.Contains(x, y))
                    fragmento = fragmentosCapaZero[i];

            }
            return fragmento;
        }
        public ImageFragment[] GetFragments(PointZ location)
        {
            return GetFragments(location.X, location.Y, location.Z);
        }
        public ImageFragment[] GetFragments(int x, int y, int z)
        {
            List<ImageFragment> fragmentosSeleccionados = new List<ImageFragment>();
            ImageFragment img;

            do
            {
                img = GetFragment(x, y, z);
                if (img != null)
                {//los quito para no molestar
                    fragmentosSeleccionados.Add(img);
                    fragments.Remove(img);
                }
            } while (img != null);

            fragments.AddRange(fragmentosSeleccionados);

            return fragmentosSeleccionados.ToArray();
        }


        public  Bitmap CrearCollage()
        {
            int xFinal = 1, xInicial = 0;
            int yFinal = 1, yInicial = 0;
            int width, height;
            for (int i = 0; i < fragments.Count; i++)
            {
                if (xFinal < (fragments[i].Location.X + fragments[i].Image.Width))
                    xFinal = (fragments[i].Location.X + fragments[i].Image.Width);
                if (xInicial > fragments[i].Location.X)
                    xInicial = fragments[i].Location.X;
                if (yFinal < (fragments[i].Location.Y + fragments[i].Image.Height))
                    yFinal = (fragments[i].Location.Y + fragments[i].Image.Height);
                if (yInicial > fragments[i].Location.Y)
                    yInicial = fragments[i].Location.Y;
            }
            width = xFinal - xInicial;
            height = yFinal - yInicial;
            return CrearCollage(new Rectangle(xInicial, yInicial, width,height));
        }
        public Bitmap CrearCollage(Rectangle rctImgResultado)
        {
        	const bool ISARGBBMPTOTAL=true;
        	Bitmap bmpTotal=new Bitmap(rctImgResultado.Width,rctImgResultado.Height);
        	fragments.Sort();//deberia poner los de la Z mas grande los primeros
        	unsafe{

        		bmpTotal.TrataBytes((ptTotal)=>{
        	for(int i=0;i<fragments.Count;i++)
        	{
        		fixed(byte* ptFragmento=fragments[i].Image.GetBytes())
        			Gabriel.Cat.BitmapExtension.SetFragment(ptTotal,bmpTotal.Height,bmpTotal.Width,ISARGBBMPTOTAL,ptFragmento,fragments[i].Image.Height,fragments[i].Image.Width,fragments[i].Image.IsArgb(),rctImgResultado.GetRelativePoint(new Point(fragments[i].Location.X,fragments[i].Location.Y)));
        		
        	}
        	}
        	
        		});
        }
        public IEnumerator<ImageFragment> GetEnumerator()
        {
            fragments.Ordena();
            return fragments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


    }
    public static class Image
    {


        #region Pixels
        public static Color ToRed(this Color pixel)
        {
            return ToRed(pixel.R, 0, 0);

        }
        public static Color ToBlue(this Color pixel)
        {
            return ToBlue(0, 0, pixel.B);
        }
        public static Color ToGreen(this Color pixel)
        {
            return ToGreen(0, pixel.G, 0);
        }
        public static Color ToEscalaGrises(this Color pixel)
        {
            return ToGrayScale(pixel.R, pixel.G, pixel.B);

        }
        public static Color ToInverted(this Color pixel)
        {
            return ToInverted((byte)(255 - pixel.R), (byte)(255 - pixel.G), (byte)(255 - pixel.B));
        }
        public static Color ToSepia(this Color pixel)
        {
            return ToSepia(pixel.R, pixel.G, pixel.B);
        }
        public static Color Mezclar(this Color pixel1, Color pixel2)
        {
            return MezclaPixels(pixel1.R, pixel1.G, pixel1.B, pixel1.A, pixel2.R, pixel2.G, pixel2.B, pixel2.A);
        }
        public static Color MezclaPixels(byte byteR1, byte byteG1, byte byteB1, byte byteA1, byte byteR2, byte byteG2, byte byteB2, byte byteA2)
        {
            int a, r, g, b;

            a = byteA1 + byteA2;
            r = byteR1 + byteR2;
            g = byteG1 + byteG2;
            b = byteB1 + byteB2;

            if (a > 255) a = 255;
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;

            return Color.FromArgb(a, r, g, b);
        }

        public static Color ToRed(byte r, byte g, byte b)
        {
            return Color.FromArgb(0, 0, r);
        }
        public static Color ToBlue(byte r, byte g, byte b)
        {
            return Color.FromArgb(b, 0, 0);
        }
        public static Color ToGreen(byte r, byte g, byte b)
        {
            return Color.FromArgb(0, g, 0);
        }
        public static Color ToGrayScale(byte r, byte g, byte b)
        {
            int v = Convert.ToInt32(0.2126 * r + 0.7152 * g + 0.0722 * b);
            return Color.FromArgb(v, v, v);

        }

        public static Color ToInverted(byte r, byte g, byte b)
        {
            return Color.FromArgb(255 - r, 255 - g, 255 - b);
        }
        public static Color ToSepia(byte r, byte g, byte b)
        {
            int rInt = Convert.ToInt32(r * 0.393 + g * 0.769 + b * 0.189);
            int gInt = Convert.ToInt32(r * 0.349 + g * 0.686 + b * 0.168);
            int bInt = Convert.ToInt32(r * 0.272 + g * 0.534 + b * 0.131);
            if (rInt > 255)
                rInt = 255;
            if (gInt > 255)
                gInt = 255;
            if (bInt > 255)
                bInt = 255;
            r = (byte)rInt;
            g = (byte)gInt;
            b = (byte)bInt;
            return Color.FromArgb(r, g, b);
        }


        #endregion

    }
    public class ImageFragment : IComparable, IComparable<ImageFragment>
    {
        PointZ localizacion;
        ImageBase imagen;

        public ImageFragment(Bitmap imagen, int x = 0, int y = 0, int z = 0)
            : this(imagen, new PointZ(x, y, z))
        {

        }
        public ImageFragment(Bitmap imagen, Point localizacion, int capa = 0)
            : this(imagen, new PointZ(localizacion != default(Point) ? localizacion.X : 0, localizacion != default(Point) ? localizacion.Y : 0, capa))
        {

        }
        public ImageFragment(Bitmap imagen, PointZ localizacion)
        {
            if (imagen == null)
                throw new NullReferenceException("La imagen no puede ser null");
            this.imagen = new ImageBase(imagen);
            Location = localizacion;
        }


        public byte[,] RgbValuesMatriu
        {
            get { return imagen.Matriu; }
        }
        public byte[] RgbValues
        {
            get { return imagen.Array; }
        }
        public PointZ Location
        {
            get
            {
                return localizacion;
            }
            set
            {
                localizacion = value;
            }
        }

        public Bitmap Image
        {
            get
            {
                return imagen.Image;
            }
        }

        #region IComparable implementation
        public int CompareTo(ImageFragment other)
        {
            int compareTo;
            if (other != null)
                compareTo = Location.CompareTo(other.Location);
            else
                compareTo = -1;
            return compareTo;
        }
        public int CompareTo(Object other)
        {
            return CompareTo(other as ImageFragment);
        }

        public bool IsInRectangle(Rectangle rctImgResultado)
        {
            return rctImgResultado.Contains(new Rectangle(this.Location.X, this.Location.Y, Image.Width, Image.Height));
        }
        #endregion



    }
    public class ImageBase
    {
        public static readonly PixelFormat DefaultPixelFormat=PixelFormat.Format32bppArgb;
        Bitmap bmp;
        byte[,] matriuBmp;
        byte[] bmpArray;
        public ImageBase(Bitmap bmp)
        {

            if (bmp == null)
                throw new NullReferenceException("La imagen no puede ser null");
            this.bmp = bmp.Clone(new Rectangle(new Point(), bmp.Size), DefaultPixelFormat);//asi todos tienen el mismo PixelFormat :)

        }

        public byte[] Array
        {
            get
            {
                if (bmpArray == null)
                    bmpArray = bmp.GetBytes();
                return bmpArray;
            }
            set
            {
                bmp.SetBytes(value);
                bmpArray = value;
                matriuBmp = null;
            }
        }

        public Bitmap Image
        {
            get
            {
                return bmp;
            }
        }

        public byte[,] Matriu
        {
            get
            {
                if (matriuBmp == null)
                    matriuBmp = bmp.GetMatriuBytes();
                return matriuBmp;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                bmp.SetMatriuBytes(value);
                matriuBmp = value;
                bmpArray = null;
            }

        }


    }

}

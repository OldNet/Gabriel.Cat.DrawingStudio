using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Binaris
{
   public class CollageBinario:ElementoIEnumerableBinario
    {
        public CollageBinario() : base(new ImageFragmentBinario(),(uint)0)
        {

        }
        public override byte[] GetBytes(object obj)
        {
            if (obj is Collage)
            {
                LongitudUInt= Convert.ToUInt32((obj as Collage).Count);
            }
            return base.GetBytes(obj);
        }
        public override object GetObject(Stream bytes)
        {
            return new Collage(((object[])base.GetObject(bytes)).Casting<ImageFragment>(false));
        }
    }
    public class ImageFragmentBinario : ElementoComplejoBinario
    {
        public ImageFragmentBinario()
        {
            base.PartesElemento.Afegir(ElementoBinario.ElementosTipoAceptado(Serializar.TiposAceptados.PointZ));
            base.PartesElemento.Afegir(ElementoBinario.ElementosTipoAceptado(Serializar.TiposAceptados.Bitmap));
        }
        public override byte[] GetBytes(object obj)
        {
            List<byte> bytesObj = new List<byte>();
            ImageFragment fragment= obj as ImageFragment;
            if (fragment!=null)
            {
                bytesObj.AddRange(PartesElemento[0].GetBytes(fragment.Location));
                bytesObj.AddRange(PartesElemento[1].GetBytes(fragment.Image));
            }
            else {
                bytesObj.Add(0x00);
            }
            return bytesObj.ToArray();
        }

        public override object GetObject(object[] parts)
        {
            PointZ location = parts[0] as PointZ;
            Bitmap bmp = null;
            ImageFragment fragment = null;
            if (location != null)
                bmp = parts[1] as Bitmap;
            if (bmp != null)
                fragment = new ImageFragment(bmp, location);
            return fragment;
        }

    }
}

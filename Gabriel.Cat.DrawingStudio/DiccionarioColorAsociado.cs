using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.DrawingStudio
{
    //no funciona bien cuando coge el primero pero si que lo hace cuando hace el GetEnumerator :)
    public class DiccionarioColor : IEnumerable<KeyValuePair<byte[], byte[][]>>
    {
        A[] diccionario;

        public DiccionarioColor()
        {
            diccionario = new A[byte.MaxValue + 1];
        }

        public DiccionarioColor(IEnumerable<KeyValuePair<Color, Color[]>> colorsKeyValue) : this()
        {
            Añadir(colorsKeyValue);
        }

        public DiccionarioColor(IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue) : this()
        {
            Añadir(colorsKeyValue);
        }

        private byte Posicion(byte[] color)
        { return color[Pixel.A]; }
        /// <summary>
        /// es para ir rapido
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] ObtenerColorByteArray(byte[] key)
        {
            byte[] color = null;
            int pos = Posicion(key);
            if(diccionario[pos]!=null)
               color = diccionario[pos].ObtenerPrimero(key);
            return color;
        }

        public Color? ObtenerColor(Color key)
        {
            byte[] colorBytes = ObtenerColorByteArray(ToByteArrayInverted(key));
            Color? color = colorBytes == null ? new Color?() : Color.FromArgb(colorBytes[Pixel.B], colorBytes[Pixel.G], colorBytes[Pixel.R], colorBytes[Pixel.A]);
            return color;
        }
        public Color?[] ObtenerColores(Color key)
        {
            List<Color?> colores = new List<Color?>();
            Color? color;
            byte[] colorBytes;
            byte[][] coloresByteArray = ObtenerTodos(ToByteArrayInverted(key));
            for(int i=0;i<coloresByteArray.Length;i++)
            {
                colorBytes = coloresByteArray[i];
                color = colorBytes == null ? new Color?() : Color.FromArgb(colorBytes[Pixel.B], colorBytes[Pixel.G], colorBytes[Pixel.R], colorBytes[Pixel.A]);
                colores.Add(color);
            }
            return colores.ToArray();
        }
        public void Añadir(Color key, Color value)
        {
            byte[] keyArray = ToByteArrayInverted(key);
            byte posicion = Posicion(keyArray);
            if (diccionario[posicion] == null)
                diccionario[posicion] = new A();
            diccionario[posicion].Añadir(keyArray, ToByteArray(value));
        }

        private byte[] ToByteArray(Color color)
        {

            return new byte[] { color.A, color.R, color.G, color.B };
        }
        private byte[] ToByteArrayInverted(Color color)
        {
             return new byte[] {color.B,color.G,color.R,color.A };//lo permuto aqui y listo :) parece que color cuando se escribe en una array los colores se giran...se permutan...ARGB->BGRA
        }

        public void Añadir(IEnumerable<KeyValuePair<Color, Color[]>> colorsKeyValues)
        {
            if (colorsKeyValues != null)
                foreach (KeyValuePair<Color, Color[]> colorKeytValues in colorsKeyValues)
                    for (int i = 0; i < colorKeytValues.Value.Length; i++)
                        Añadir(colorKeytValues.Key, colorKeytValues.Value[i]);
        }

        public void Añadir(IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue)
        {
            if (colorsKeyValue != null)
                foreach (KeyValuePair<Color, Color> keyValue in colorsKeyValue)
                    Añadir(keyValue.Key, keyValue.Value);

        }
        public void Eliminar(byte[] key)
        {
            byte posicion = Posicion(key);
            if (diccionario[posicion] != null)
            {
                diccionario[posicion].Eliminar(key);
                diccionario[posicion] = null;
            }

        }

        public void Eliminar(byte[] key, int index)
        {
            byte posicion = Posicion(key);
            if(diccionario[posicion]!=null)
               diccionario[posicion].Eliminar(key, index);
        }

        public byte[][] ObtenerTodos(byte[] key)
        {
            byte[][] todos = null;
            int posicion = Posicion(key);
            if(diccionario[posicion]!=null)
              todos= diccionario[posicion].ObtenerTodos(key);
            return todos;
        }

        public IEnumerator<KeyValuePair<byte[], byte[][]>> GetEnumerator()
        {
            byte[] key;
            byte[][] value;
            Base baseA, baseR, baseG;
            List<byte[]> objetos;
            const int LENGTH = byte.MaxValue + 1;
            for (int i = 0; i < diccionario.Length; i++)
            {
                if (diccionario[i] != null)
                    for (int a = 0; a < LENGTH; a++)
                    {
                        baseA = diccionario[i].rango[a] as Base;
                        if (baseA != null)
                            for (int r = 0; r < LENGTH; r++)
                            {
                                baseR = baseA.rango[r] as Base;
                                if (baseR != null)
                                    for (int g = 0; g < LENGTH; g++)
                                    {
                                        baseG = baseR.rango[g] as Base;
                                        if (baseG != null)
                                            for (int b = 0; b < LENGTH; b++)
                                            {
                                                objetos = baseG.rango[b] as List<byte[]>;
                                                if (objetos.Count>0)
                                                {
                                                    key = new byte[] { (byte)a, (byte)r, (byte)g, (byte)b };
                                                    value = objetos.ToArray();
                                                    if (value != null)
                                                        yield return new KeyValuePair<byte[], byte[][]>(key, value);
                                                }

                                            }

                                    }

                            }
                    }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public byte[][] this[byte[] key]
        {
            get { return ObtenerTodos(key); }
        }
    }
    internal abstract class Base
    {
        internal object[] rango;
        public Base()
        {
            rango = new object[byte.MaxValue + 1];
        }
        protected abstract dynamic Objeto();
        protected abstract byte Posicion(byte[] color);
        public virtual byte[] ObtenerPrimero(byte[] key)
        {
            byte[] color = rango[Posicion(key)] == null ? null : ((Base)rango[Posicion(key)]).ObtenerPrimero(key);
            return color;
        }
        public virtual void Añadir(byte[] key, byte[] value)
        {
            byte posicion = Posicion(key);
            if (rango[posicion] == null)
                rango[posicion] = Objeto();
            ((Base)rango[posicion]).Añadir(key, value);
        }

        public virtual void Eliminar(byte[] key)
        {
            byte posicion = Posicion(key);
            if (rango[posicion] != null)
            {
                ((Base)rango[posicion]).Eliminar(key);
                rango[posicion] = null;
            }
        }

        public virtual void Eliminar(byte[] key, int index)
        {
            byte posicion = Posicion(key);
            if (rango[posicion] != null)
                ((Base)rango[posicion]).Eliminar(key, index);
        }

        public virtual byte[][] ObtenerTodos(byte[] key)
        {
            byte posicion = Posicion(key);
            byte[][] coloresAsociados = null;
            if (rango[posicion] != null)
                coloresAsociados = ((Base)rango[posicion]).ObtenerTodos(key);
            return coloresAsociados;
        }
        public byte[][] this[byte[] key]
        {
            get { return ObtenerTodos(key); }
        }

    }
    internal class A : Base
    {
        protected override dynamic Objeto()
        {
            return new R();
        }

        protected override byte Posicion(byte[] color)
        {
            return color[Pixel.A];
        }
    }
    internal class R : Base
    {
        protected override dynamic Objeto()
        {
            return new G();
        }

        protected override byte Posicion(byte[] color)
        {
            return color[Pixel.R];
        }
    }
    internal class G : Base
    {
        protected override dynamic Objeto()
        {
            return new B();
        }

        protected override byte Posicion(byte[] color)
        {
            return color[Pixel.G];
        }
    }
    internal class B : Base
    {
        public B()
        {
            for (int i = 0; i < rango.Length; i++)
            {
                rango[i] = new List<byte[]>();
            }
        }
        protected override dynamic Objeto()
        {
            throw new NotImplementedException();
        }

        protected override byte Posicion(byte[] color)
        {
            throw new NotImplementedException();
        }
        public override byte[] ObtenerPrimero(byte[] key)
        {

            byte[] color = null;
            List<byte[]> list=((List<byte[]>)rango[key[Pixel.B]]);
            if (list.Count > 0)
                color = list[0];
            return color;
        }
        public override void Añadir(byte[] key, byte[] value)
        {
            List<byte[]> list = (List<byte[]>)rango[key[Pixel.B]];
            list.Add(value);
        }
        public override void Eliminar(byte[] key)
        {
            ((List<byte[]>)rango[key[Pixel.B]]).Clear();
        }
        public override void Eliminar(byte[] key, int index)
        {
            ((List<byte[]>)rango[key[Pixel.B]]).RemoveAt(index);
        }
        public override byte[][] ObtenerTodos(byte[] key)
        {
            return ((List<byte[]>)rango[key[Pixel.B]]).ToArray();
        }
    }

    public class DiccionarioColor2
    {
        LlistaOrdenada<int, List<byte[]>> diccionario;
        public DiccionarioColor2()
        {
            diccionario = new LlistaOrdenada<int, List<byte[]>>();
        }

        public DiccionarioColor2(IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue):this()
        {
            Añadir(colorsKeyValue);
        }

        public void Añadir(IEnumerable<KeyValuePair<Color, Color>> colorsKeyValue)
        {
            if (colorsKeyValue != null)
                foreach (KeyValuePair<Color, Color> colorKeyValue in colorsKeyValue)
                    Añadir(colorKeyValue.Key, colorKeyValue.Value);
        }

        public void Añadir(Color key, Color value)
        {
            int keyArgb = key.ToArgb();
            if (!diccionario.ContainsKey(keyArgb))
                diccionario.Add(keyArgb, new List<byte[]>());
            diccionario[keyArgb].Value.Add(Serializar.GetBytes(value.ToArgb()));

        }
        public void Añadir(Color key, params Color[] values)
        {
            for (int i = 0; i < values.Length; i++)
                Añadir(key, values[i]);
        }
        public void Eliminar(Color key,int index)
        {
            int keyArgb = key.ToArgb();
            if (diccionario.ContainsKey(keyArgb))
                diccionario[keyArgb].Value.RemoveAt(index);
        }
        public void Eliminar(Color key)
        {
            diccionario.Remove(key.ToArgb());
        }
        public Color? ObtenerPrimero(Color key)
        {
            return ObtenerPrimero(key.ToArgb());
        }
        public Color? ObtenerPrimero(int key)
        {
            Color? value;
            if (diccionario.ContainsKey(key))
                value =Color.FromArgb(Serializar.ToInt( diccionario[key].Value[0]));
            else
                value = new Color?();
            return value;
        }
        /*   public byte[] ObtenerPrimero(byte[] key)
           {
               byte[] byteColor=null;
               Color? color=ObtenerPrimero(Color.FromArgb(Serializar.ToInt(key)));
               if (color != null)
                   byteColor = Serializar.GetBytes(color.Value.ToArgb());
               return byteColor;
           }*/
        public byte[] ObtenerPrimero(byte[] key)
        {
            int keyArgb = Serializar.ToInt(key);
            List<byte[]> coloresList;
            byte[] color = null;
            if (diccionario.ContainsKey(keyArgb))
            {
            	coloresList = diccionario.GetValue(keyArgb);
                if (coloresList != null && coloresList.Count > 0)
                    color = coloresList[0];
            }
            return color;
        }
        public Color?[] ObtenerTodos(Color key)
        {
            List<Color?> colores = new List<Color?>();
            int keyArgb = key.ToArgb();
            List<byte[]> coloresList;
            if (diccionario.ContainsKey(keyArgb))
            {
            	coloresList = diccionario.GetValue(keyArgb);
                for (int i = 0; i < coloresList.Count; i++)
                {
                    colores.Add(Color.FromArgb(Serializar.ToInt(coloresList[i])));
                }
            }
            return colores.ToArray();
        }
    }

}

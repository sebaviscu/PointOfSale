using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Utilities
{
    public class TicketModel
    {
        public static StringBuilder line = new StringBuilder();
        string ticket = "";
        string parte1, parte2;

        public static int max = 40;
        int cort;

        public StringBuilder Lineas
        {
            get { return line; }
        }

        public static string LineasGuion()
        {
            string LineaGuion = "----------------------------------------";   // agrega lineas separadoras -

            return line.AppendLine(LineaGuion).ToString();
        }


        public static void EncabezadoVenta()
        {
            string LineEncavesado = " Prod          Cant   Precio    Total";   // agrega lineas de  encabezados
            line.AppendLine(LineEncavesado);
        }
        public void TextoIzquierda(string par1)                          // agrega texto a la izquierda
        {
            max = par1.Length;
            if (max > 40)                                 // **********
            {
                cort = max - 40;
                parte1 = par1.Remove(40, cort);        // si es mayor que 40 caracteres, lo corta
            }
            else { parte1 = par1; }                      // **********
            line.AppendLine(ticket = parte1);

        }
        public void TextoDerecha(string par1)
        {
            ticket = "";
            max = par1.Length;
            if (max > 40)                                 // **********
            {
                cort = max - 40;
                parte1 = par1.Remove(40, cort);           // si es mayor que 40 caracteres, lo corta
            }
            else { parte1 = par1; }                      // **********
            max = 40 - par1.Length;                     // obtiene la cantidad de espacios para llegar a 40
            for (int i = 0; i < max; i++)
            {
                ticket += " ";                          // agrega espacios para alinear a la derecha
            }
            line.AppendLine(ticket += parte1 + "\n");                //Agrega el texto

        }
        public void TextoCentro(string par1)
        {
            ticket = "";
            max = par1.Length;
            if (max > 40)                                 // **********
            {
                cort = max - 40;
                parte1 = par1.Remove(40, cort);          // si es mayor que 40 caracteres, lo corta
            }
            else { parte1 = par1; }                      // **********
            max = (int)(40 - parte1.Length) / 2;         // saca la cantidad de espacios libres y divide entre dos
            for (int i = 0; i < max; i++)                // **********
            {
                ticket += " ";                           // Agrega espacios antes del texto a centrar
            }                                            // **********
            line.AppendLine(ticket += parte1 + "\n");

        }
        public void TextoExtremos(string par1, string par2)
        {
            max = par1.Length;
            if (max > 18)                                 // **********
            {
                cort = max - 18;
                parte1 = par1.Remove(18, cort);          // si par1 es mayor que 18 lo corta
            }
            else { parte1 = par1; }                      // **********
            ticket = parte1;                             // agrega el primer parametro
            max = par2.Length;
            if (max > 18)                                 // **********
            {
                cort = max - 18;
                parte2 = par2.Remove(18, cort);          // si par2 es mayor que 18 lo corta
            }
            else { parte2 = par2; }
            max = 40 - (parte1.Length + parte2.Length);
            for (int i = 0; i < max; i++)                 // **********
            {
                ticket += " ";                            // Agrega espacios para poner par2 al final
            }                                             // **********
            line.AppendLine(ticket += parte2 + "\n");                   // agrega el segundo parametro al final

        }
        public void AgregaTotales(string par1, double total)
        {
            max = par1.Length;
            if (max > 25)                                 // **********
            {
                cort = max - 25;
                parte1 = par1.Remove(25, cort);          // si es mayor que 25 lo corta
            }
            else { parte1 = par1; }                      // **********
            ticket = parte1;
            parte2 = total.ToString("c");
            max = 40 - (parte1.Length + parte2.Length);
            for (int i = 0; i < max; i++)                // **********
            {
                ticket += " ";                           // Agrega espacios para poner el valor de moneda al final
            }                                            // **********
            line.AppendLine(ticket += parte2 + "\n");

        }

        // se le pasan los Aticulos  con sus detalles
        public void AgregaArticulo(string Articulo, double precio, int cant, double subtotal)
        {
            if (cant.ToString().Length <= 3 && precio.ToString("c").Length <= 10 && subtotal.ToString("c").Length <= 11) // valida que cant precio y total esten dentro de rango
            {
                string elementos = "", espacios = "";
                bool bandera = false;
                int nroEspacios = 0;

                if (Articulo.Length > 40)                                 // **********
                {
                    //cort = max - 16;
                    //parte1 = Articulo.Remove(16, cort);          // corta a 16 la descripcion del articulo
                    nroEspacios = (3 - cant.ToString().Length);
                    espacios = "";
                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";
                    }
                    elementos += espacios + cant.ToString();

                    // colocamos el precio a la derecha
                    nroEspacios = (10 - precio.ToString().Length);
                    espacios = "";

                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";
                    }
                    elementos += espacios + precio.ToString();

                    //colocar el subtotal a la dercha
                    nroEspacios = (11 - subtotal.ToString().Length);
                    espacios = "";

                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";
                    }
                    elementos += espacios + subtotal.ToString();

                    int CaracterActual = 0;// indica en que caracter se quedo
                    for (int Longtext = Articulo.Length; Longtext > 16; Longtext++)
                    {
                        if (bandera == false)
                        {
                            line.AppendLine(Articulo.Substring(CaracterActual, 16) + elementos);
                            bandera = true;
                        }
                        else
                        {
                            line.AppendLine(Articulo.Substring(CaracterActual, 16));

                        }
                        CaracterActual += 16;
                    }
                    line.AppendLine(Articulo.Substring(CaracterActual, Articulo.Length - CaracterActual));


                }
                else
                {
                    for (int i = 0; i < (16 - Articulo.Length); i++)
                    {
                        espacios += " ";

                    }
                    elementos = Articulo + espacios;
                    nroEspacios = (3 - cant.ToString().Length);
                    espacios = "";
                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";
                    }
                    elementos += espacios + cant.ToString();

                    // colocamos el precio a la derecha
                    nroEspacios = (10 - precio.ToString().Length);
                    espacios = "";

                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";
                    }
                    elementos += espacios + precio.ToString();

                    //colocar el subtotal a la dercha
                    nroEspacios = (11 - subtotal.ToString().Length);
                    espacios = "";

                    for (int i = 0; i < nroEspacios; i++)
                    {
                        espacios += " ";
                    }
                    elementos += espacios + subtotal.ToString();
                    line.AppendLine(elementos);

                }
            }
            else
            {
                //  MessageBox.Show("Valores fuera de rango");

            }
        }
    }
}
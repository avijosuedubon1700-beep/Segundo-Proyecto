using System;

namespace Granja_adb
{
    // Clase con propiedades de encapsulación inmutable para los productos agrícolas
    public class Cultivo
    {
        public string Especie { get; }
        public double CostoSemilla { get; }
        public double ValorMercado { get; }
        public int MesesMaduracion { get; }

        public Cultivo(string especie, double costoSemilla, double valorMercado, int mesesMaduracion)
        {
            Especie = especie;
            CostoSemilla = costoSemilla;
            ValorMercado = valorMercado;
            MesesMaduracion = mesesMaduracion;
        }
    }
}
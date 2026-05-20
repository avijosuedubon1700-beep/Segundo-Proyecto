using System;

namespace Granja_adb
{
    public class Parcela
    {
        public bool TieneSiembra { get; private set; }
        public Cultivo? CultivoActivo { get; private set; }
        public int TiempoTranscurrido { get; private set; }

        public Parcela()
        {
            Vaciar();
        }

        public void AlojarCultivo(Cultivo nuevoCultivo)
        {
            TieneSiembra = true;
            CultivoActivo = nuevoCultivo;
            TiempoTranscurrido = 0;
        }

        public void Vaciar()
        {
            TieneSiembra = false;
            CultivoActivo = null;
            TiempoTranscurrido = 0;
        }

        public void IncrementarMes()
        {
            if (TieneSiembra)
            {
                TiempoTranscurrido++;
            }
        }

        public bool EvaluarMadurez()
        {
            if (!TieneSiembra || CultivoActivo == null) return false;
            return TiempoTranscurrido >= CultivoActivo.MesesMaduracion;
        }

        // Renderiza el estado biológico actual mediante un icono intuitivo
        public string ObtenerIconoProgreso()
        {
            if (!TieneSiembra || CultivoActivo == null) return " . ";

            if (EvaluarMadurez()) return "💰"; // Listo para el cobro mercantil

            double porcentaje = (double)TiempoTranscurrido / CultivoActivo.MesesMaduracion;
            if (porcentaje >= 0.5) return "🌿"; // Crecimiento avanzado
            return "🌱"; // Germinación / Brote inicial
        }
    }
}
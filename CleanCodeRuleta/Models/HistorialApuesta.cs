using System;

namespace CleanCodeRuleta.Models
{
    [Serializable]
    public class HistorialApuesta
    {
        public int NumeroApuesta { get; set; }
        public string Jugador { get; set; }
        public string ObjetivoApuesta { get; set; }
        public int Cantidad { get; set; }
    }
}

using System;

namespace CleanCodeRuleta.Models
{
    [Serializable]
    public class Ruleta
    {
        public int Id { get; set; }
        public bool Abierta { get; set; }
        public int BalanceApuestas { get; set; }
    }
}

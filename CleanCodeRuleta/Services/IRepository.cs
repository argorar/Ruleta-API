using CleanCodeRuleta.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanCodeRuleta.Services
{
    public interface IRepository
    {
        Task<int> CrearRuleta();
        Task<bool> AbrirRuleta(string idRuleta);
        Task<bool> Apostar(string objetivoApuesta, int cantidadDinero, string idRuleta, string jugador);
        Task<List<HistorialApuesta>> CerrarRuleta(string idRuleta);
        Task<List<Ruleta>> ObtenerListaRuletas();
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using CleanCodeRuleta.Models;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json.Linq;

namespace CleanCodeRuleta.Services
{
    public class Repository : IRepository
    {
        private static readonly IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "ogziB8407DlQrirjSZ3JxXVHM7g3gSLRQNp5LAga",
            BasePath = "https://ruleta-72411.firebaseio.com/"
        };

        private readonly IFirebaseClient firebaseClient;

        public Repository()
        {
            firebaseClient = new FireSharp.FirebaseClient(config);
        }

        public async Task<int> CrearRuleta()
        {
            Ruleta nuevaRuleta = new Ruleta()
            {
                Id = await NuevoId(),
                Abierta = false,
                BalanceApuestas = 0
            };
            SetResponse respuesta = await firebaseClient.SetAsync($"Ruletas/{nuevaRuleta.Id}", nuevaRuleta);
            return respuesta.ResultAs<Ruleta>().Id;
        }

        private async Task<int> NuevoId()
        {
            FirebaseResponse respuestaContadorIds = await firebaseClient.GetAsync("ContadorIds");
            int nuevoId = respuestaContadorIds.ResultAs<int>() + 1;
            await firebaseClient.SetAsync("ContadorIds", nuevoId);
            return nuevoId;
        }
        
        public async Task<bool> AbrirRuleta(string idRuleta)
        {
            if (!await ExisteRuleta(idRuleta))
                return false;
            SetResponse respuesta = await firebaseClient.SetAsync($"Ruletas/{idRuleta}/Abierta", true);
            return respuesta.ResultAs<bool>();
        }

        public async Task<bool> Apostar(string objetivoApuesta, int cantidadDinero, string idRuleta, string jugador)
        {
            if (!await PermitidoApostar(objetivoApuesta, cantidadDinero, idRuleta))
                return false;

            HistorialApuesta nuevaApuesta = new HistorialApuesta()
            {
                NumeroApuesta = await NumeroApuesta(),
                Jugador = jugador,
                ObjetivoApuesta = objetivoApuesta,
                Cantidad = cantidadDinero
            };
            PushResponse respuesta = await firebaseClient.PushAsync($"Ruletas/{idRuleta}/HistorialApuestas", nuevaApuesta);
            if (respuesta.StatusCode == System.Net.HttpStatusCode.OK)
                return await ActualizarBalanceRuleta(idRuleta, cantidadDinero);
            else
                return false;
        }

        private async Task<bool> ActualizarBalanceRuleta(string idRuleta, int cantidadDinero)
        {
            FirebaseResponse respuestaBalance = await firebaseClient.GetAsync($"Ruletas/{idRuleta}/BalanceApuestas");
            int balanceActual = respuestaBalance.ResultAs<int>();
            int balanceNuevo = balanceActual + cantidadDinero;
            SetResponse respuestaActualizacion = await firebaseClient.SetAsync($"Ruletas/{idRuleta}/BalanceApuestas", balanceNuevo);
            return respuestaActualizacion.StatusCode == System.Net.HttpStatusCode.OK;
        }

        private async Task<bool> PermitidoApostar(string objetivoApuesta, int cantidadDinero, string idRuleta)
        {
            if (!await ExisteRuleta(idRuleta))
                return false;

            if (!await EstaAbierta(idRuleta))
                return false;

            // Se asume que el crédito del jugador está validado

            if (!ApuestaEsValida(objetivoApuesta))
                return false;

            if (!CantidadEsValida(cantidadDinero))
                return false;

            return true;
        }        

        private async Task<bool> ExisteRuleta(string idRuleta)
        {
            FirebaseResponse respuesta = await firebaseClient.GetAsync($"Ruletas/{idRuleta}");
            if (respuesta.ResultAs<Ruleta>() != null)
                return true;
            else
                return false;
        }

        private async Task<bool> EstaAbierta(string idRuleta)
        {
            FirebaseResponse respuesta = await firebaseClient.GetAsync($"Ruletas/{idRuleta}");
            Ruleta ruleta = respuesta.ResultAs<Ruleta>();
            if ( ruleta != null && ruleta.Abierta)
                return true;
            else
                return false;
        }

        private bool ApuestaEsValida(string objetivoApuesta)
        {
            bool esNumerico = int.TryParse(objetivoApuesta, out int numero);
            if (esNumerico && numero >= 0 && numero <= 36)
                return true;

            if (objetivoApuesta.Equals("rojo") || objetivoApuesta.Equals("negro"))
                return true;

            return false;
        }

        private bool CantidadEsValida(int cantidadDinero)
        {
            return cantidadDinero > 0 && cantidadDinero <= 10000;
        }

        private async Task<int> NumeroApuesta()
        {
            FirebaseResponse respuesta = await firebaseClient.GetAsync("ContadorApuestas");
            int nuevoNumero = respuesta.ResultAs<int>() + 1;
            await firebaseClient.SetAsync("ContadorApuestas", nuevoNumero);
            return nuevoNumero;
        }

        public async Task<List<HistorialApuesta>> CerrarRuleta(string idRuleta)
        {
            if (!await ExisteRuleta(idRuleta))
                return null;

            SetResponse respuesta = await firebaseClient.SetAsync($"Ruletas/{idRuleta}/Abierta", false);
            if (respuesta.StatusCode == System.Net.HttpStatusCode.OK)
                return await ObtenerHistorialApuestas(idRuleta);
            return null;
        }

        private async Task<List<HistorialApuesta>> ObtenerHistorialApuestas(string idRuleta)
        {
            FirebaseResponse historialRespuesta = await firebaseClient.GetAsync($"Ruletas/{idRuleta}/HistorialApuestas");
            Dictionary<string, HistorialApuesta> apuestas = historialRespuesta.ResultAs<Dictionary<string, HistorialApuesta>>();
            return new List<HistorialApuesta>(apuestas.Values);
        }

        public async Task<List<Ruleta>> ObtenerListaRuletas()
        {
            List<Ruleta> listaRuletas = new List<Ruleta>();
            int numeroRuletas = await NumeroRuletas();
            for (int id = 1; id <= numeroRuletas; id++)
            {
                FirebaseResponse ruletaRespuesta = await firebaseClient.GetAsync($"Ruletas/{id}");
                listaRuletas.Add(SerializarRuleta(ruletaRespuesta.Body));
            }
            return listaRuletas;
        }

        private async Task<int> NumeroRuletas()
        {
            FirebaseResponse respuestaContadorRuletas = await firebaseClient.GetAsync("ContadorIds");
            return respuestaContadorRuletas.ResultAs<int>();
        }

        private Ruleta SerializarRuleta(string body)
        {
            JObject jsonRuleta = JObject.Parse(body);
            Ruleta ruleta = new Ruleta()
            {
                Id = (int)jsonRuleta.GetValue("Id"),
                Abierta = (bool)jsonRuleta.GetValue("Abierta"),
                BalanceApuestas = (int)jsonRuleta.GetValue("BalanceApuestas")
            };
            return ruleta;
        }        
    }
}

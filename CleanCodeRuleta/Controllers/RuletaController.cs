using System.Collections.Generic;
using System.Threading.Tasks;
using CleanCodeRuleta.Models;
using CleanCodeRuleta.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeRuleta.Controllers
{
    [Route("api/Ruleta")]
    [ApiController]
    public class RuletaController : ControllerBase
    {

        private readonly IRepository _repository;

        public RuletaController(IRepository repository)
        {
            _repository = repository;
        }

        [Route("crear")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CrearRuleta()
        {
            int IdNuevaRuleta = await _repository.CrearRuleta(); ;
            return CreatedAtAction(nameof(CrearRuleta),IdNuevaRuleta);
        }

        [Route("abrir/{idRuleta}")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public  async Task<IActionResult> AbrirRuleta([FromRoute]string idRuleta)
        {
            bool resultadoOperacion = await _repository.AbrirRuleta(idRuleta);
            if (resultadoOperacion)
                return Ok("Operación exitosa");
            else
                return NotFound();
        }

        [Route("apostar/{objetivoApuesta}&{cantidadDinero}&{idRuleta}")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Apostar([FromRoute]string objetivoApuesta, 
                                                 [FromRoute]int cantidadDinero, 
                                                 [FromRoute]string idRuleta,
                                                 [FromHeader]string jugador)
        {
            bool resultadoOperacion = await _repository.Apostar(objetivoApuesta, cantidadDinero, idRuleta, jugador);
            if (resultadoOperacion)
                return Ok("Operación exitosa");
            else
                return NotFound();
        }

        [Route("cerrar/{idRuleta}")]
        [HttpPost]
        public async Task<List<HistorialApuesta>> CerrarRuleta([FromRoute]string idRuleta)
        {
            List<HistorialApuesta> historialApuestas = await _repository.CerrarRuleta(idRuleta);
            return historialApuestas;
        }

        [Route("lista")]
        [HttpGet]
        public async Task<List<Ruleta>> ListaRuletas()
        {
            List<Ruleta> listaRuletas = await _repository.ObtenerListaRuletas();
            return listaRuletas;
        }
    }
}
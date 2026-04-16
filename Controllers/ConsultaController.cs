using Microsoft.AspNetCore.Mvc;
using SGMG.Dtos.Request;
using SGMG.Dtos.Response;
using SGMG.Services;

namespace SGMG.Controllers
{
    [Route("[controller]")]
    public class ConsultaController : Controller
    {
        private readonly IConsultaService _consultaService;

        public ConsultaController(IConsultaService consultaService)
        {
            _consultaService = consultaService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/api/consulta/registrar")]
        public async Task<GenericResponse<ConsultaResponseDTO>> RegistrarConsulta([FromBody] ConsultaRequestDTO dto)
        {
            return await _consultaService.AddConsultaAsync(dto);
        }

        [HttpPut]
        [Route("/api/consulta/actualizar")]
        public async Task<GenericResponse<ConsultaResponseDTO>> ActualizarConsulta([FromBody] ConsultaRequestDTO dto)
        {
            return await _consultaService.UpdateConsultaAsync(dto);
        }

        [HttpGet]
        [Route("/api/consulta/{id}")]
        public async Task<GenericResponse<ConsultaResponseDTO>> ObtenerConsulta(int id)
        {
            return await _consultaService.GetConsultaByIdAsync(id);
        }
    }
}

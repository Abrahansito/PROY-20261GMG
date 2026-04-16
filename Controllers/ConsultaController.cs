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

        // Vista principal
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // API: Registrar consulta
        [HttpPost]
        [Route("/api/consulta/registrar")]
        public async Task<GenericResponse<ConsultaResponseDTO>> RegistrarConsulta([FromBody] ConsultaRequestDTO dto)
        {
            return await _consultaService.AddConsultaAsync(dto);
        }

        // API: Actualizar consulta
        [HttpPut]
        [Route("/api/consulta/actualizar")]
        public async Task<GenericResponse<ConsultaResponseDTO>> ActualizarConsulta([FromBody] ConsultaRequestDTO dto)
        {
            return await _consultaService.UpdateConsultaAsync(dto);
        }

        // API: Obtener consulta por ID
        [HttpGet]
        [Route("/api/consulta/{id}")]
        public async Task<GenericResponse<ConsultaResponseDTO>> ObtenerConsulta(int id)
        {
            return await _consultaService.GetConsultaByIdAsync(id);
        }
    }
}

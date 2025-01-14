using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Embeddings;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    [Authorize]
    public class ChatIAController : BaseController
    {

        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger<ChatIAController> _logger;
        private readonly IMapper _mapper;

        public ChatIAController(IEmbeddingService embeddingService, ILogger<ChatIAController> logger, IMapper mapper)
        {
            _embeddingService = embeddingService;
            _logger = logger;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        [HttpGet]
        public async Task<IActionResult> GetChat()
        {

            var gResponse = new GenericResponse<List<VMChatGPTResponse>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var response = await _embeddingService.GetChatByIdUser(user.IdUsuario, user.IdTienda);

                gResponse.Object = _mapper.Map<List<VMChatGPTResponse>>(response);
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar ajuste", _logger);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] string question)
        {

            var user = ValidarAutorizacion([Roles.Administrador]);

            var gResponse = new GenericResponse<VMChatGPTResponse>();
            try
            {
                if (string.IsNullOrWhiteSpace(question))
                {
                    return BadRequest("La pregunta no puede estar vacía.");
                }

                var response = await _embeddingService.GetChatResponseAsync(question, user.IdUsuario, user.IdTienda);

                gResponse.Object = _mapper.Map<VMChatGPTResponse>(response);
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar ajuste", _logger, question);
            }
        }
    }
}

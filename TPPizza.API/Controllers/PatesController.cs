using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPPizza.API.Models;
using TPPizza.Business;

namespace TPPizza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatesController : ControllerBase
    {
        private readonly PizzaService _service;

        public PatesController(PizzaService service)
        {
            _service = service;
        }

        //GET .../api/Pates
        [HttpGet]
        public List<PateDTO> GetPates()
        {
            var pates = _service.GetPates().Select(pa => PateDTO.FromModel(pa));

            return pates.ToList();
        }
    }
}

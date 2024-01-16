using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPPizza.API.Models;
using TPPizza.Business;

namespace TPPizza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientsController : ControllerBase
    {
        private readonly PizzaService _service;

        public IngredientsController(PizzaService service)
        {
            _service = service;
        }

        //GET .../api/Ingredients
        [HttpGet]
        public List<IngredientDTO> GetIngredients()
        {
            var ingredients = _service.GetIngredients().Select(i => IngredientDTO.FromModel(i));

            return ingredients.ToList();
        }
    }
}

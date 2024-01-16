using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TPPizza.API.Models;
using TPPizza.Business;

namespace TPPizza.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PizzasController : ControllerBase
    {
        private readonly PizzaService _service;
        private readonly FileService _fileService;

        public PizzasController(PizzaService service, FileService fileService)
        {
            _service = service;
            _fileService = fileService;

        }

        //GET .../api/Pizzas
        [HttpGet]
        public List<PizzaDTO> GetPizzas()
        {
            var pizzas = _service.GetPizzas().Select(p => PizzaDTO.FromModel(p));

            return pizzas.ToList();
        }

        //GET .../api/Pizzas/[int]
        [HttpGet("{id:int}")]
        public PizzaDTO? GetPizza(int id)
        {
            var pizza = _service.GetPizza(id);

            return pizza == null ? null : PizzaDTO.FromModel(pizza);
        }

        //GET .../api/Pizzas/[id]?name=
        [HttpGet("{id}/Exist")]
        public bool GetPizzaByName(int id, [FromQuery]string name)
        {
           return _service.PizzaExists(id, name);
        }

        //POST .../api/Pizzas
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] PizzaDTO pizzaDTO)
        {
            if (pizzaDTO == null)
            {
                return BadRequest();
                //JSON null => {}
            }

            //Si pour le second param de CreatePizzaAsync
            //pizzaDTO.Ingredients.Select(i => i.Id).ToList()
            //Alors le JSON envoyé contiendra 
            //{
            //    "Name" : "PizzaName",
            //    "Ingredients" :  [
            //        { "Id": "1" }, 
            //        { "Id", "2"}
            //        ]
            //}

            var pizzaId = await _service.CreatePizzaAsync(PizzaDTO.ToModel(pizzaDTO), pizzaDTO.IngredientIds);

            return CreatedAtAction("GetPizza", new { id = pizzaId }, null);
        }

        //PUT .../api/Pizzas/[id]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id,[FromBody] PizzaDTO pizzaDTO)
        {
            if(pizzaDTO == null || id != pizzaDTO.Id)
            {
                return BadRequest();
            }

            await _service.EditPizzaAsync(PizzaDTO.ToModel(pizzaDTO), pizzaDTO.IngredientIds);

            return Ok();
        }

        //DELETE .../api/Pizzas/[id]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pizza = _service.GetPizza(id);

            if(pizza == null)
            {
                return NotFound();
            }

            await _fileService.DeleteAsync(pizza.Name);
            await _service.DeletePizzaAsync(pizza);

            return Ok();
        }
    }
}

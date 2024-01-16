using TPPizza.DAL.Models;

namespace TPPizza.API.Models
{
    //DTO => Data Transfert Object
    public class PizzaDTO
    {
        public int Id { get; set; }

        //[JsonPropertyName("Nom")]
        public string Name { get; set; }

        public int PateId { get; set; }

        public PateDTO? Pate { get; set; }

        public List<int> IngredientIds { get; set; }

        public List<IngredientDTO>? Ingredients { get; set; }

        public static PizzaDTO FromModel(Pizza pizza)
        {
            return new PizzaDTO
            {
                Id = pizza.Id,
                Name = pizza.Name,
                PateId = pizza.PateId,
                Pate = PateDTO.FromModel(pizza.Pate),
                Ingredients = pizza.Ingredients.Select(i => IngredientDTO.FromModel(i)).ToList(),
            };
        }

        public static Pizza ToModel(PizzaDTO pizza)
        {
            return new Pizza
            {
                Id = pizza.Id,
                Name = pizza.Name,
                PateId = pizza.PateId
            };
        }
    }
}

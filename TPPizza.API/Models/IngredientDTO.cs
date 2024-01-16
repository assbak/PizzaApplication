using TPPizza.DAL.Models;

namespace TPPizza.API.Models
{
    public class IngredientDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static IngredientDTO FromModel(Ingredient ingredient)
        {
            return new IngredientDTO
            {
                Id = ingredient.Id,
                Name = ingredient.Name
            };
        }
    }
}

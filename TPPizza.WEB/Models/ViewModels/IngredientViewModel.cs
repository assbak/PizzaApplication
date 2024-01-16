using System.Text.Json.Serialization;

namespace TPPizza.WEB.Models.ViewModels
{
    public class IngredientViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}

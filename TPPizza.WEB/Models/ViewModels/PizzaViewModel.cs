using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TPPizza.WEB.Models.ViewModels
{
    public class PizzaViewModel
    {
        [JsonPropertyName("Id")]
        public int PizzaId { get; set; }

        [Display(Name = "Nom de la pizza")]
        [Required(ErrorMessage = "Merci de saisir un nom de pizza")]
        [JsonPropertyName("Name")]
        public string PizzaName { get; set; }

        [Display(Name = "Pâte")]
        public int PateId { get; set; }

        [ValidateNever]
        public PateViewModel Pate { get; set; }

        [ValidateNever]
        public List<IngredientViewModel> Ingredients { get; set; } = new List<IngredientViewModel>();

        [Display(Name = "Ingredients")]
        [ValidateNever]
        public string IngredientNames => string.Join(", ", this.Ingredients.Select(x => x.Name));

        //Si on ne souhaite pas serialiser l'image.
        //[JsonIgnore]
        public IFormFile? Image { get; set; }

        [ValidateNever]
        public string ImageBase64 { get; set; }
    }
}

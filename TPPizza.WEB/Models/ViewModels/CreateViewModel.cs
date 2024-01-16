using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TPPizza.WEB.Models.ViewModels
{
    public class CreateViewModel
    {
        public PizzaViewModel Pizza { get; set; }

        [ValidateNever]
        public SelectList Pates { get; set; }
        [ValidateNever]
        public SelectList Ingredients { get; set; }

        [Display(Name = "Ingredients")]
        [Required(ErrorMessage = "Veuillez selectionner des ingrédients")]
        public List<int> IngredientIds { get; set; }
    }
}

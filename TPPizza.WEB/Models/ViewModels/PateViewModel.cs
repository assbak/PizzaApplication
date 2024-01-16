using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace TPPizza.WEB.Models.ViewModels
{
    public class PateViewModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }


        [Display(Name = "Pâte")]
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPPizza.DAL.Models
{
    public class Pizza
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int PateId { get; set; }

        public Pate Pate { get; set; }

        public ICollection<Ingredient> Ingredients { get; set;}
    }
}

using TPPizza.DAL.Models;

namespace TPPizza.API.Models
{
    public class PateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static PateDTO FromModel(Pate pate)
        {
            return new PateDTO
            {
                Id = pate.Id,
                Name = pate.Name
            };
        }
    }
}

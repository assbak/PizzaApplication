using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TPPizza.DAL.Models
{
    public class PizzaDbContext : IdentityDbContext
    {
        public PizzaDbContext(DbContextOptions<PizzaDbContext> options)
            : base(options)
        {
                
        }

        public DbSet<Pizza> Pizzas { get; set; }
        public DbSet<Pate> Pates { get; set; }
        public DbSet<Ingredient> Ingredients { get; set;}
    }
}

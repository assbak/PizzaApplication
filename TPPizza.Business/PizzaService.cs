using Microsoft.EntityFrameworkCore;
using TPPizza.DAL.Models;

namespace TPPizza.Business
{
    public class PizzaService
    {
        private readonly PizzaDbContext _context;
        public PizzaService(PizzaDbContext context)
        {
            _context = context;
        }

        public List<Ingredient> GetIngredients()
        {
            return _context.Ingredients.ToList();
        }

        public List<Pate> GetPates()
        {
            return _context.Pates.ToList();
        }

        public List<Pizza> GetPizzas()
        {
            return _context.Pizzas
                .Include(p => p.Pate)
                .Include(p => p.Ingredients).ToList();
        }

        public Pizza? GetPizza(int id)
        {
            return _context.Pizzas
                .Include(p => p.Pate)
                .Include(p => p.Ingredients)
                .FirstOrDefault(p => p.Id == id);
        }

        public async Task<int> CreatePizzaAsync(Pizza p, List<int> ingredientIds)
        {
            p.Ingredients = ingredientIds.Select(id => _context.Ingredients.First(i => i.Id == id)).ToList();

            _context.Pizzas.Add(p);
            await _context.SaveChangesAsync();

            return p.Id;
        }

        public async Task EditPizzaAsync(Pizza piz, List<int> ingredientsIds)
        {
            var pizza = _context.Pizzas.Include(p => p.Ingredients).First(p => p.Id == piz.Id);

            pizza.Name = piz.Name;
            pizza.PateId = piz.PateId;

            pizza.Ingredients = ingredientsIds
                .Select(id => _context.Ingredients.First(i => i.Id == id))
                .ToList();

            await _context.SaveChangesAsync();
        }

        public async Task DeletePizzaAsync(Pizza p)
        {
            _context.Pizzas.Remove(p);

            await _context.SaveChangesAsync();
        }

        public bool PizzaExists(int pizzaId, string pizzaName)
        {
            return _context.Pizzas.Where(p => p.Id != pizzaId).Any(p => p.Name == pizzaName);
        }
    }
}
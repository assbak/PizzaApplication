using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TPPizza.WEB.Models.ViewModels;
using TPPizza.WEB.Utils;

namespace TPPizza.WEB.Controllers
{
    [Authorize]
    public class PizzasController : Controller
    {
        private readonly HttpClient _httpClient;

        public PizzasController(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient.CreateClient("apiClient");
        }

        // GET: Pizzas
        public async Task<IActionResult> IndexAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JwtToken"));

            var httpResponse = await _httpClient.GetAsync("api/Pizzas");

            if (httpResponse.IsSuccessStatusCode)
            {
                var pizzas = await httpResponse.Content.ReadFromJsonAsync<IEnumerable<PizzaViewModel>>();

                return View(pizzas);
            }
            else
            {
                return View(new List<PizzaViewModel>());
            }
        }

        // GET: Pizzas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JwtToken"));

            var httpResponse = await _httpClient.GetAsync($"api/Pizzas/{id}");

            if (httpResponse.IsSuccessStatusCode)
            {
                var pizza = await httpResponse.Content.ReadFromJsonAsync<PizzaViewModel>();

                if (pizza == null)
                {
                    return NotFound();
                }

                httpResponse = await _httpClient.GetAsync($"api/Files/{pizza.PizzaName}");

                if(httpResponse.IsSuccessStatusCode)
                {
                    var base64 = await httpResponse.Content.ReadAsStringAsync();

                    pizza.ImageBase64 = base64;
                }

                return View(pizza);
            }
            else
            {
                return NotFound();
            }
        }

        // GET: Pizzas/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync()
        {
            var vm = new CreateViewModel();

            await PopulateSelectListAsync(vm);

            return View(vm);
        }

        // POST: Pizzas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync([Bind("Pizza,IngredientIds")] CreateViewModel cvm)
        {
            var errors = await GenerateErrorMessageForRulesAsync(cvm);

            foreach (var (key, message) in errors)
            {
                ModelState.AddModelError(key, message);
            }

            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(new { Name = cvm.Pizza.PizzaName, cvm.Pizza.PateId, cvm.IngredientIds });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JwtToken"));

                var httpResponse = await _httpClient.PostAsync($"api/Pizzas", content);

                if (httpResponse.IsSuccessStatusCode)
                {
                    if (cvm.Pizza.Image != null)
                    {
                        //Stream image => Bytes[] => Base64 (string)
                        var bytes = await cvm.Pizza.Image.GetAllBytesAsync();

                        var data = Convert.ToBase64String(bytes);

                        var fileName = $"{cvm.Pizza.PizzaName}{Path.GetExtension(cvm.Pizza.Image.FileName)}";

                        var file = new { fileName, cvm.Pizza.Image.ContentType, data };

                        httpResponse = await _httpClient.PostAsJsonAsync($"api/Files", file);
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            await PopulateSelectListAsync(cvm);

            return View(cvm);
        }

        // GET: Pizzas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JwtToken"));

            var httpResponse = await _httpClient.GetAsync($"api/Pizzas/{id}");

            if (httpResponse.IsSuccessStatusCode)
            {
                var pizza = await httpResponse.Content.ReadFromJsonAsync<PizzaViewModel>();

                if (pizza == null)
                {
                    return NotFound();
                }

                var vm = new CreateViewModel
                {
                    Pizza = pizza,
                    IngredientIds = pizza.Ingredients.Select(i => i.Id).ToList()
                };

                await PopulateSelectListAsync(vm);

                return View(vm);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: Pizzas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Pizza,IngredientIds")] CreateViewModel cvm)
        {
            cvm.Pizza.PizzaId = id;

            var errors = await GenerateErrorMessageForRulesAsync(cvm);
            foreach (var (key, message) in errors)
            {
                ModelState.AddModelError(key, message);
            }

            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(new { Id = cvm.Pizza.PizzaId, Name = cvm.Pizza.PizzaName, cvm.Pizza.PateId, cvm.IngredientIds });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JwtToken"));

                var httpResponse = await _httpClient.PutAsync($"api/Pizzas/{id}", content);

                if (httpResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            await PopulateSelectListAsync(cvm);

            return View(cvm);
        }

        // GET: Pizzas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JwtToken"));

            var httpResponse = await _httpClient.DeleteAsync($"api/Pizzas/{id}");

            if (httpResponse.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return NotFound();
            }
        }

        private async Task PopulateSelectListAsync(CreateViewModel cvm)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JwtToken"));

            var httpResponse = await _httpClient.GetAsync($"api/Ingredients");

            if (httpResponse.IsSuccessStatusCode)
            {
                var ingredients = await httpResponse.Content.ReadFromJsonAsync<IEnumerable<IngredientViewModel>>();

                cvm.Ingredients = new SelectList(ingredients, "Id", "Name");
            }

            httpResponse = await _httpClient.GetAsync($"api/Pates");

            if (httpResponse.IsSuccessStatusCode)
            {
                var pates = await httpResponse.Content.ReadFromJsonAsync<IEnumerable<PateViewModel>>();

                cvm.Pates = new SelectList(pates, "Id", "Name");
            }
        }

        private async Task<List<(string key, string message)>> GenerateErrorMessageForRulesAsync(CreateViewModel cvm)
        {
            var errors = new List<(string key, string message)>();

            if (cvm.IngredientIds.Count < 2 || cvm.IngredientIds.Count > 5)
            {
                errors.Add(("IngredientIds", "Merci de selectionner entre 2 et 5 ingredients"));
            }

            // Mise en place des parameters
            // Passage de multiples parametres si nécessaire
            var query = new Dictionary<string, string>()
            {
                ["name"] = cvm.Pizza.PizzaName
            };

            // Transformation des parametres
            // api/Pizzas/{cvm.Pizza.PizzaId}/Exist?name=NomDeLaPizza
            var uri = QueryHelpers.AddQueryString($"api/Pizzas/{cvm.Pizza.PizzaId}/Exist", query);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JwtToken"));

            var httpResponse = await _httpClient.GetAsync(uri);

            if (httpResponse.IsSuccessStatusCode)
            {
                var exist = await httpResponse.Content.ReadFromJsonAsync<bool>();

                if (exist)
                {
                    errors.Add(("Pizza.PizzaName", "Merci de saisir un autre nom"));
                }
            }

            return errors;
        }
    }
}

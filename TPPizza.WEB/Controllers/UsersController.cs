using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TPPizza.WEB.Models.ViewModels;

namespace TPPizza.WEB.Controllers
{
    public class UsersController : Controller
    {
        private readonly HttpClient _httpClient;

        public UsersController(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient.CreateClient("apiClient");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAsync([Bind] RegisterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var responseHttp = await _httpClient.PostAsJsonAsync("api/Users/Register", viewModel);

                if (responseHttp.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    var errors = await responseHttp.Content.ReadAsStringAsync();
                    ModelState.AddModelError("Errors", errors);
                }
            }

            return View(viewModel);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsync([Bind] UserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var responseHttp = await _httpClient.PostAsJsonAsync("api/Users/Login", viewModel);

                if (responseHttp.IsSuccessStatusCode)
                {
                    var infos = await responseHttp.Content.ReadFromJsonAsync<SignInViewModel>();

                    List<Claim> claims;

                    if (infos.Roles is null)
                    {
                        claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Sid, infos.Id),
                            new Claim(ClaimTypes.Name, viewModel.Email)
                        };
                    }
                    else
                    {
                        claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Sid, infos.Id),
                            new Claim(ClaimTypes.Name, viewModel.Email),
                            new Claim(ClaimTypes.Role, infos.Roles.First())
                        };
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    //Stockage du Bearer token dans la session HTTP (navigateur)
                    HttpContext.Session.SetString("JwtToken", infos.Token);

                    return RedirectToAction("Index", "Pizzas");
                }
                else
                {
                    ModelState.AddModelError("Password", "Email ou mot de passe invalide");
                }
            }
            return View(viewModel);
        }

        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Forbidden()
        {
            return View();
        }
    }
}

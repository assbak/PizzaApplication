using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using TPPizza.API.Models;

namespace TPPizza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UsersController(
            UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel input)
        {
            var user = CreateUser();

            user.Email = input.Email;
            user.UserName = new MailAddress(input.Email).User;

            var result = await _userManager.CreateAsync(user, input.Password);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] RegisterModel input)
        {
            var user = await SignIn(input);

            if (user is not null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var token = GenerateJwtToken(user);

                return Ok(new { roles, user.Id, Token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] RegisterModel model)
        {
            if (!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.Password))
            {
                var user = await SignIn(model);

                if (user is not null)
                {
                    var token = GenerateJwtToken(user);

                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
            }

            return BadRequest();
        }

        private JwtSecurityToken GenerateJwtToken(IdentityUser? user)
        {
            var claims = new[]
                                {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("UserId", user.Id.ToString()),
                        new Claim("Email", user.Email)
                    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: signIn);

            return token;
        }

        private IdentityUser CreateUser()
        {
            return Activator.CreateInstance<IdentityUser>();
        }

        private async Task<IdentityUser?> SignIn(RegisterModel input)
        {
            var userName = new MailAddress(input.Email).User;

            var result = await _signInManager.PasswordSignInAsync(userName, input.Password, false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return await _userManager.FindByEmailAsync(input.Email);
            }

            return null;
        }
    }
}

using ApiCraftSystem.Data;
using ApiCraftSystem.Helper.Utility;
using ApiCraftSystem.Repositories.AccountService.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace ApiCraftSystem.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            JwtTokenGenerator tokenGenerator,
            ILogger<AuthController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.UserName);
            if (user == null)
                return Unauthorized("Invalid credentials");

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var token = _tokenGenerator.GenerateToken(user);
                _logger.LogInformation("User logged in.");
                return Ok(new { token });
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return Forbid("Account is locked.");
            }
            else if (result.RequiresTwoFactor)
            {
                return BadRequest("2FA is not supported in this API.");
            }

            return Unauthorized("Invalid login attempt");
        }
    }
}

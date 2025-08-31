using JwtPratik.Data;
using JwtPratik.DTOs;
using JwtPratik.Models;
using JwtPratik.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JwtPratik.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var exists = await _db.Users.AnyAsync(u => u.Email == req.Email);
            if (exists) return BadRequest(new { message = "Email zaten kayıtlı." });

            var (hash, salt) = PasswordHasher.HashPassword(req.Password);

            var user = new User { Email = req.Email, PasswordHash = hash, PasswordSalt = salt };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = user.Id }, new { user.Id, user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == req.Email);
            if (user is null) return Unauthorized(new { message = "Email veya şifre hatalı." });

            var ok = PasswordHasher.Verify(req.Password, user.PasswordHash, user.PasswordSalt);
            if (!ok) return Unauthorized(new { message = "Email veya şifre hatalı." });

            var jwt = _tokenService.CreateToken(user.Id.ToString(), user.Email);
            return Ok(new { token = jwt });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
            var sub = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            return Ok(new { userId = sub, email });
        }
    }
}

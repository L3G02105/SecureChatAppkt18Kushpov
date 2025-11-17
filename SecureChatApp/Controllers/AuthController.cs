using Microsoft.AspNetCore.Mvc;
using SecureChatApp.Models;
using SecureChatApp.Services;
using System.Collections.Concurrent;

namespace SecureChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static ConcurrentDictionary<string, string> _users = new(); // Simple in-memory store
        private readonly TokenService _tokenService;

        public AuthController(IConfiguration config)
        {
            _tokenService = new TokenService(config["Jwt:Key"]);
        }

        [HttpPost("register")]
        public IActionResult Register(AuthRequest request)
        {
            if (!_users.TryAdd(request.Username, request.Password))
                return BadRequest("User already exists");
            return Ok();
        }

        [HttpPost("login")]
        public IActionResult Login(AuthRequest request)
        {
            if (_users.TryGetValue(request.Username, out var password) && password == request.Password)
            {
                var token = _tokenService.GenerateToken(request.Username);
                return Ok(new { token });
            }
            return Unauthorized();
        }
    }
}

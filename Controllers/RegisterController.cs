using Microsoft.AspNetCore.Mvc;
using SecureNote.Models;
using SecureNote.Services;
using System.ComponentModel.DataAnnotations;

namespace SecureNote.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly AppDbContext _db;

        public RegisterController(AppDbContext db)
        {
            _db = db;
        }

        // POST api/<RegisterController>
        [HttpPost]
        public ActionResult Post([FromForm] RegisterDTO data)
        {
            if (data.Code != "*****************")
            {
                return Unauthorized("Invalid invitation code");
            }

            // Ensure username is free
            if (_db.Users.Any(u => u.Username == data.Username))
            {
                return BadRequest($"Username '{data.Username}' already taken");
            }

            User user = new()
            {
                Username = data.Username,
                PasswordHash = Authentication.PasswordHash(data.Password),
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            Authentication.Signin(Response, user);
            return Ok();
        }
    }

    public class RegisterDTO
    {
        [RegularExpression(@"^\w{4,20}$")]
        [Required]
        public string Username { get; set; }

        [RegularExpression(@"^\w{4,20}$")]
        [Required]
        public string Password { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
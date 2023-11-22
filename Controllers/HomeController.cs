using Microsoft.AspNetCore.Mvc;
using SecureNote.Filters;
using SecureNote.Models;
using SecureNote.Services;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Data;

namespace SecureNote.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;

        public HomeController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [Auth]
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (int.TryParse(User.Identity.Name, out int userId))
                {
                    User? u = _db.Users.Find(userId);
                    if (u != null)
                    {
                        List<Claim> roleClaims = HttpContext.User.FindAll(ClaimTypes.Role).ToList();
                        foreach (var role in roleClaims)
                        {
                            if (role.Value == "Epic")
                            {
                                ViewBag.IsEpic = true;
                            }
                        }
                        ViewBag.User = u;
                        ViewBag.ShowLogout = true;
                        ViewBag.Notes = _db.Notes.Where(_ => _.UserId == userId).ToList();
                        ViewBag.PublicNotes = _db.Notes.Where(_ => _.IsPublic).ToList();
                        return View("Dashboard");
                    }
                }

                // Invalid session
                Authentication.Signout(Response);
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginDTO login)
        {
            User? user = _db.Users.Where(_ => _.Username == login.Username).FirstOrDefault();
            if (user != null) {
                if (user.PasswordHash == Authentication.PasswordHash(login.Password))
                {
                    Authentication.Signin(Response, user);
                    return RedirectToAction("Index");
                }
            }

            ViewBag.LoginError = "Invalid credentials";
            return View("Index");
        }

        public IActionResult LogOut()
        {
            Authentication.Signout(Response);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Auth]
        public IActionResult NewNote(Note note)
        {
            User? user = Authentication.AuthorizedUser(User.Identity, _db, Response);
            if (user == null)
            {
                return Unauthorized();
            }

            note.UserId = user.Id;
            _db.Notes.Add(note);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordDTO data)
        {
            // Verify user exists
            User? user = _db.Users.Where(_ => _.Username == data.Username).FirstOrDefault();
            if (user != null)
            {
                // Create a long unguessable code
                Random rnd = new ();
                int code = rnd.Next(100000, 999999);
                _db.PwdResetCodes.Add(new()
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    Code = code,
                });
                _db.SaveChanges();

                //TODO: Send code to user
            }

            // Never disclose if the user existed or not, that would result in a user enumeration leak.
            ViewBag.Ok = "Enter the 6 digit reset code we sent you. It's valid for 30 minutes.";
            return View("ResetPassword");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View("ResetPassword");
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordDTO data)
        {
            // Make sure code is valid and not expired
            PwdResetCode? prc = _db.PwdResetCodes
                .Where(_ => _.Code == data.Code && _.CreatedAt < DateTime.UtcNow.AddMinutes(30))
                .FirstOrDefault();
            if (prc != null)
            {
                // Make sure user exists
                User? user = _db.Users.Find(prc.UserId);
                if (user != null)
                {
                    // Update user
                    user.PasswordHash = Authentication.PasswordHash(data.Password);
                    _db.Users.Update(user);

                    // Remove reset code
                    _db.PwdResetCodes.Where(_ => _.UserId == user.Id).ExecuteDelete();
                    _db.SaveChanges();

                    ViewBag.Ok = $"Password for user '{user.Username}' successfully reset!";
                    return View("ResetPassword");
                }
            }

            ViewBag.Error = "Invalid or expired code";
            return View("ResetPassword");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public class LoginDTO
        {
            [RegularExpression(@"^\w{4,20}$")]
            [Required]
            public string Username { get; set; }

            [RegularExpression(@"^\w{4,20}$")]
            [Required]
            public string Password { get; set; }
        }

        public class ForgotPasswordDTO
        {
            [RegularExpression(@"^\w{4,20}$")]
            [Required]
            public string Username { get; set; }
        }

        public class ResetPasswordDTO
        {
            [Required]
            public int Code{ get; set; }

            [RegularExpression(@"^\w{4,20}$")]
            [Required]
            public string Password { get; set; }
        }
    }
}
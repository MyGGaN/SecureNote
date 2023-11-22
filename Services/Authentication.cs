using SecureNote.Filters;
using SecureNote.Models;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace SecureNote.Services
{
    public class Authentication
    {
        public static void Signin(HttpResponse response, User user)
        {
            Session s = new()
            {
                UserId = user.Id,
                IsEpic = user.IsEpic,
            };

            string strJson = JsonSerializer.Serialize(s);
            var bytes = Encoding.UTF8.GetBytes(strJson);
            string cookieData = Convert.ToBase64String(bytes);

            // Create session cookie
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(1),
                Path = "/"
            };

            response.Cookies.Append("Session", cookieData, cookieOptions);
        }

        public static void Signout(HttpResponse response)
        {
            response.Cookies.Delete("Session");
        }
        public static string PasswordHash(string password)
        {
            byte[] hashBytes = SHA1.HashData(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }

        public static User? AuthorizedUser(IIdentity? identity, AppDbContext db, HttpResponse response)
        {
            if (identity != null && identity.IsAuthenticated)
            {
                if (int.TryParse(identity.Name, out int userId))
                {
                    User? u = db.Users.Find(userId);
                    if (u != null)
                    {
                        return u;
                    }
                }

                // Invalid session
                Signout(response);
            }

            return null;
        }
    }
}

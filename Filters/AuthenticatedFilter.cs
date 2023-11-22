using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using System.Security.Claims;
using System.Text.Json;

namespace SecureNote.Filters
{
    public sealed class AuthAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string Realm { get; set; }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context?.HttpContext?.Request?.Cookies == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cookie = context.HttpContext.Request.Cookies["Session"];
            if (cookie == null) { return; }

            // Decode
            var base64EncodedBytes = Convert.FromBase64String(cookie);
            string cookieData = Encoding.UTF8.GetString(base64EncodedBytes);
            if (string.IsNullOrEmpty(cookieData)) { return; }

            // Deserialize
            Session? s = JsonSerializer.Deserialize<Session>(cookieData);
            if (s == null) { return; }

            // Construct claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, s.UserId.ToString()),
                new Claim(ClaimTypes.Role, s.IsEpic ? "Epic" : "User"),
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Password");
            context.HttpContext.User = new ClaimsPrincipal(claimsIdentity);
        }
    }

    public class Session
    {
        public int UserId { get; set; }
        public bool IsEpic { get; set; }
    }
}

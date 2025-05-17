using EcommerceApi.Models;
using System.Security.Claims;

namespace EcommerceApi.Services
{
    public class JwtReader
    {



        public static int GetUserId(ClaimsPrincipal user)
        {

            var identity = user.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return 0;
            }
            //Get user id from token
            var userIdClaim = identity.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
            {
                return 0;
            }
            int userId;
            try
            {
                userId = int.Parse(userIdClaim.Value);
            }
            catch (FormatException)
            {
                return 0;
            }
            return userId;
        }

        public static string GetUserRole(ClaimsPrincipal user) 
        {
            var identity = user.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return "";
            }
            //Get user id from token
            var userIdClaim = identity.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("role"));
            if (userIdClaim == null)
            {
                return "";
            }
            return userIdClaim.Value;

        }

        public static Dictionary<string, string> GetUserRoles(ClaimsPrincipal user) {

            Dictionary<string, string> claims = new Dictionary<string, string>();

            var identity = user.Identities as ClaimsIdentity;
            if(identity != null)
            {
                foreach(var claim in identity.Claims)
                {
                    claims.Add(claim.Type, claim.Value);

                }
            }
        
            return claims;
        }

        //private readonly IHttpContextAccessor _httpContextAccessor;

        //public JwtReader(IHttpContextAccessor httpContextAccessor)
        //{
        //    _httpContextAccessor = httpContextAccessor;
        //}

        //public string GetUserId()
        //{
        //    var userId = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;
        //    return userId ?? string.Empty;
        //}

        //public string GetUserRole()
        //{
        //    var role = _httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value;
        //    return role ?? string.Empty;
        //}
    }
}

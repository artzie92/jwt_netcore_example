using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace JWTExample.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        [Authorize]
        public IList<string> GetValues()
        {
            var someClaim = this.HttpContext.User.Claims.FirstOrDefault();
            return new List<string>
            {
                "Example of JWT auth for https://developerlife.pl",
                $"User: {someClaim.Value}"
            };
        }
    }
}
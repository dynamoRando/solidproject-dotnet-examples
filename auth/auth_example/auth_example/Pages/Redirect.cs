using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Web;

namespace auth_example.Pages
{
    // handle the incoming redirect
    // from the Solid Community Server

    [Route("api/[controller]")]
    [ApiController]
    public class Redirect : Controller
    {
        // the address of the Community Server
        static string identityProvider = "http://localhost:3000";

        [HttpGet]
        public string Get()
        {
            // https://identitymodel.readthedocs.io/en/latest/client/token.html#requesting-a-token-using-the-authorization-code-grant-type
            var info = Request.QueryString.Value;

            Console.WriteLine("Redirect!");
            Console.WriteLine(info);
            Debug.Write(info);

            if (info is not null)
            {
                var values = HttpUtility.ParseQueryString(info);

                string authCode = values.GetValues(0).First();
                Console.WriteLine(authCode);
            }

            return string.Empty;
        }

    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace auth_example.Pages
{
    // handle the incoming redirect
    // from the Solid Community Server

    // https://stackoverflow.com/questions/60022519/how-to-add-controller-not-view-support-to-a-server-side-blazor-project
    // https://stackoverflow.com/questions/63624005/how-to-implement-openid-connect-from-a-private-provider-in-the-c-sharp-asp-net

    [Authorize(Policy = "Read")]
    [Route("api/[controller]")]
    [ApiController]
    public class Redirect : Controller
    {
        public IActionResult Index()
        {
            var info = Request.QueryString.Value;

            Console.WriteLine("Redirect!");
            Console.WriteLine(info);
            Debug.Write(info);

            return Redirect("/");
        }

        [HttpGet]
        public string Get()
        {
            return "Hello, World!";
        }
    }
}

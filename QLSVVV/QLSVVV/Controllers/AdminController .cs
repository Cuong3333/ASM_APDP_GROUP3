using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace QLSVVV.Controllers
{

    [Authorize(Roles = "Admin")]


    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            /*var auth = User.Identity;*/
            return View();
        }
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using QLSVVV.Data;
using QLSVVV.Models;
using BCrypt.Net;

namespace QLSVVV.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly QLSVVVContext _context;

        public AuthenticationController(QLSVVVContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password))
            {
                ViewBag.error = "Please enter both username and password.";
                return View(user);
            }

            var result = _context.User
                .FirstOrDefault(u => u.UserName == user.UserName);

            if (result != null && BCrypt.Net.BCrypt.Verify(user.Password, result.Password))
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, result.UserName),
            new Claim(ClaimTypes.Role, result.Role)
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
               
                // Redirect user based on their role
                if (result.Role == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (result.Role == "Teacher")
                {
                    return RedirectToAction("Index", "Teachers");
                }
                else if (result.Role == "Student")
                {
                    return RedirectToAction("Index", "Students");
                    
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ViewBag.error = "Invalid user!";
                return View(user);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineChatMvc.Data;
using OnlineChatMvc.Models;
using System.Diagnostics;
using System.Security.Claims;
using System.Xml.Linq;

namespace OnlineChatMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ChatContext _context;

        public HomeController(ILogger<HomeController> logger, ChatContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(string name)
        {
            var user = _context.Users.FirstOrDefault(x => x.Name == name);

            if (user == null)
            {
              user = new User
                {
                   Name = name,
                   Role = UserRole.User
                };

                _context.Users.Add(user);
                _context.SaveChanges();
            }

            //пользователь заблокирован
            if (user.Role == UserRole.Banned)
            {
                ModelState.AddModelError("userBanned","Пользователь заблокирован.");

                return View("Index", GetMessages());
            }

            var role = user.Role.ToString();

            var claims = new List<Claim>{
                new Claim("Id", user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, role),

            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal (claimsIdentity));
            
            return RedirectToAction("Index");
            
        }


        public IActionResult Index()
        {
            return View(GetMessages());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private List<Message> GetMessages()
        {
            var messages = _context.Messages
                .Include(p => p.User)
                .OrderByDescending(x => x.Id).Take(50).ToList();

            return messages;
        }
    }
}
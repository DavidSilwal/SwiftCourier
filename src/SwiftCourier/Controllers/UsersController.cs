using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using SwiftCourier.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using SwiftCourier.ViewModels;

namespace SwiftCourier.Controllers
{
    public class UsersController : BaseController
    {
        private readonly ILogger _logger;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public UsersController(
            UserManager<User> userManager,
            ILoggerFactory loggerFactory, ApplicationDbContext context) : base(userManager, context)
        {
            _logger = loggerFactory.CreateLogger<AccountController>();
        }
        
        public async Task<IActionResult> Index()
        {
            if (!HasPermission("VIEW_USERS"))
            {
                return Unauthorized();
            }

            return View(await _userManager.Users.ToListAsync());
        }
        
        public async Task<IActionResult> Details(int? id)
        {
            if (!HasPermission("VIEW_USERS"))
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return HttpNotFound();
            }

            User user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }
        
        public IActionResult Create()
        {
            if (!HasPermission("CREATE_USERS"))
            {
                return Unauthorized();
            }

            ViewBag.Roles = _context.Roles.ToList().ToListViewModel();

            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (!HasPermission("CREATE_USERS"))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var user = model.ToEntity();

                var result = await _userManager.CreateAsync(user, model.Password);

                if(result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(result);
                }
            }

            ViewBag.Roles = _context.Roles.ToList().ToListViewModel();

            return View(model);
        }
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (!HasPermission("EDIT_USERS"))
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return HttpNotFound();
            }

            var user = await _userManager.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id.Value);

            if (user == null)
            {
                return HttpNotFound();
            }


            ViewBag.Roles = _context.Roles.ToList().ToListViewModel();

            return View(user.ToViewModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (!HasPermission("EDIT_USERS"))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == model.Id);

                if (user == null)
                {
                    return HttpNotFound();
                }
                
                await _userManager.SetEmailAsync(user, model.Email);
                await _userManager.SetUserNameAsync(user, model.UserName);

                user = model.UpdateEntity(user);

                var result = await _userManager.UpdateAsync(user);

                if(result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(result);
                }
            }

            ViewBag.Roles = _context.Roles.ToList().ToListViewModel();

            return View(model);
        }

        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (!HasPermission("CHANGE_USERS_PASSWORDS"))
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return HttpNotFound();
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id.Value);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(UserChangePasswordViewModel model)
        {
            if (!HasPermission("CHANGE_USERS_PASSWORDS"))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == model.Id);

                if (user == null)
                {
                    return HttpNotFound();
                }

                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, model.Password);

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(result);
                }
            }

            return View(model);
        }

        [ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!HasPermission("DELETE_USERS"))
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return HttpNotFound();
            }

            User user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!HasPermission("DELETE_USERS"))
            {
                return Unauthorized();
            }

            User user = await _userManager.FindByIdAsync(id.ToString());
            await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }
    }
}

using DACN_TBDD_TGDD.Models;
using DACN_TBDD_TGDD.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DACN_TBDD_TGDD.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;

        public UserController(DataContext dataContext, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = dataContext;
        }

        // GET: /Admin/User/Index
        [Route("Index")]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 5; // Number of items per page
            var totalUsers = await _dataContext.Users.CountAsync(); // Get total number of users

            // Create a Paginate instance
            var paginate = new Paginate(totalUsers, page, pageSize);

            var users = await (from u in _dataContext.Users
                               join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                               join r in _dataContext.Roles on ur.RoleId equals r.Id
                               select new { User = u, RoleName = r.Name })
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            // Pass pagination info and user data to the view
            ViewBag.Pager = paginate;
            return View(users);
        }


        // GET: /Admin/User/Create
        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }

        // POST: /Admin/User/Create
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
                if (createUserResult.Succeeded)
                {
                    var role = await _roleManager.FindByIdAsync(user.RoleId);
                    if (role != null)
                    {
                        var addToRoleResult = await _userManager.AddToRoleAsync(user, role.Name);
                        if (!addToRoleResult.Succeeded)
                        {
                            foreach (var error in addToRoleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(user);
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in createUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(user);
        }

        // GET: /Admin/User/Delete/5
        [HttpGet]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Admin/User/DeleteConfirmed
        [HttpPost]
        [Route("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["success"] = "User deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Delete", user);
        }

        // GET: /Admin/User/Edit/5
        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name", user.RoleId);

            return View(new AppUserModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId
            });
        }


        // POST: /Admin/User/Edit/5
        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUserModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.UserName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!string.IsNullOrEmpty(model.RoleId))
                    {
                        var role = await _roleManager.FindByIdAsync(model.RoleId);
                        if (role != null)
                        {
                            var addToRoleResult = await _userManager.AddToRoleAsync(user, role.Name);
                            if (!addToRoleResult.Succeeded)
                            {
                                foreach (var error in addToRoleResult.Errors)
                                {
                                    ModelState.AddModelError("", error.Description);
                                }
                                return View(model);
                            }
                        }
                    }

                    TempData["success"] = "User updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name", model.RoleId);

            return View(model);
        }
    }
}

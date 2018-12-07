using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UserService.Models;
using UserService.Models.ViewModels;
using UserService.Services;
using UserService.Data;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace UserService.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Users/[action]")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ApplicationUsers users = new ApplicationUsers();
            users.Users = _userManager.Users.ToList();
            return View(users);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            ApplicationUser user = _userManager.Users.Where(r => r.Id == id).SingleOrDefault();
            if (user == null) return View(new ApplicationUser());
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var existedUser = await _userManager.FindByIdAsync(id);
            if (existedUser != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(existedUser);

                if (!result.Succeeded)
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.ToString());
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ToggleAdmin(string id)
        {
            var role = await _roleManager.FindByNameAsync("Admin");
            if (role == null)
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var user = await _userManager.FindByIdAsync(id);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                var existedUser = await _userManager.FindByIdAsync(user.Id);
                IdentityResult result;
                if (existedUser != null)
                {
                    existedUser.Name = user.Name;
                    existedUser.Middlename = user.Middlename;
                    existedUser.Email = user.Email;
                    existedUser.Surname = user.Surname;
                    result = await _userManager.UpdateAsync(existedUser);
                    user = existedUser;
                }
                else
                {
                    result = await _userManager.CreateAsync(user);
                }

                if (result.Succeeded)
                {
                    string token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var urlEncode = HttpUtility.UrlEncode(token);
                    var callbackUrl = $"{Request.Scheme}://{Request.Host.Value}/Identity/Account/ResetPassword?userId={user.Id}&code={urlEncode}";
                    await _emailSender.SendEmailAsync(user.Email, "Password reset", callbackUrl);
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.ToString());
                    }
                }
            }
            return View(user);
        }
    }
}
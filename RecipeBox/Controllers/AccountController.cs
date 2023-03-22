using System;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RecipeBox.Models;
using System.Threading.Tasks;
using RecipeBox.ViewModels;

namespace RecipeBox.Controllers
{
  public class AccountController : Controller
  {
    private readonly RecipeBoxContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController (UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RecipeBoxContext db)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _db = db;
    }

    public ActionResult Index()
    {
      return View();
    }

    public IActionResult Register()
    {
      return View();
    }

    [HttpPost]
    public async Task<ActionResult> Register (RegisterViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }      else
      {
        ApplicationUser user = new ApplicationUser 
        { 
          UserName = model.Email,
          Email = model.Email,
          TwoFactorEnabled = true,
          
        };
        IdentityResult result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
          return RedirectToAction("Index");
        }
        else
        {
          foreach (IdentityError error in result.Errors)
          {
            ModelState.AddModelError("", error.Description);
          }
          return View(model);
        }
      }
    }
        public IActionResult ResetPass()
    {
      return View();
    }

        public ActionResult Login()
    {
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Login(LoginViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }
      else
      {
        ApplicationUser appUser= await _userManager.FindByEmailAsync(model.Email);
        if (appUser != null)
        {
          // await _signInManager.SignOutAsync();
          Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(appUser, model.Password, false, true);
          if (result.Succeeded)
            // return RedirectToAction("Index");
            return RedirectToAction("LoginTwoStep", new { appUser.Email });
          if (result.RequiresTwoFactor)
          {
            return RedirectToAction("LoginTwoStep", new { appUser.Email });
          }
        }
        ModelState.AddModelError(nameof(model.Email), "Login Failed: Invalid Email or Password");
        return View(model);
      
        // Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: true, lockoutOnFailure: false);
        // if (result.Succeeded)
        // {
        //   return RedirectToAction("Index");
        // }
        // else
        // {
        //   ModelState.AddModelError("", "There is something wrong with your email or username. Please try again.");
        //   return View(model);
        // }
      }
    }
    public async Task<IActionResult> LoginTwoStep(string email)
    {
      var user = await _userManager.FindByEmailAsync(email);
      var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
      EmailHelper emailHelper = new EmailHelper();
      bool emailResponse = emailHelper.SendEmailTwoFactorCode(user.Email, token);
      return View();
    }
    [HttpPost]
    public async Task<IActionResult> LoginTwoStep(TwoFactor twoFactor, string returnUrl)
    {
      if(!ModelState.IsValid)
      {
        return View(twoFactor.TwoFactorCode);
      }
      var result = await _signInManager.TwoFactorSignInAsync("Email", twoFactor.TwoFactorCode, false, false);
      if (result.Succeeded)
      {
        return Redirect(returnUrl ?? "/");
      }
      else
      {
        ModelState.AddModelError("", "Invalid Login Attempt");
        return View();
      }
    }

    [HttpPost]
    public async Task<ActionResult> LogOff()
    {
      await _signInManager.SignOutAsync();
      return RedirectToAction("Index");
    }
  }
}
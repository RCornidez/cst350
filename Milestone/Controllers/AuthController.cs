using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milestone.Interfaces;
using Milestone.Models;

namespace Milestone.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly IUserManager _userService;

    public AuthController(ILogger<AuthController> logger, IUserManager userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult ProcessLogin(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Login", model);

        int userId = _userService.CheckCredentials(model.Username, model.Password);

        if (userId == 0)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View("Login", model);
        }

        HttpContext.Session.SetInt32("UserId", userId);
        return RedirectToAction("StartGame", "Game");
    }

    [AllowAnonymous]
    public IActionResult RegisterSuccess()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult ProcessRegister(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Register", model);

        var user = new UserModel
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Sex = model.Sex,
            Age = model.Age,
            State = model.State,
            Email = model.Email,
            Username = model.Username,
            PasswordHash = model.Password
        };

        _userService.AddUser(user);

        return RedirectToAction("RegisterSuccess");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

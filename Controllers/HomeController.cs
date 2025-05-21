using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using prova2.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography;


namespace prova2.Controllers;

public class HomeController : Controller
{
    public HomeController()
    {
        
    }

    public IActionResult Index()
    {
        if(ValidaLogin())
            return RedirectToAction("Login");        
        else
            return View();
    }

    public IActionResult Pagina2()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(Usuario model)
    {
        Repositorio<Usuario> repo = new Repositorio<Usuario>();
        List<Usuario> usuarios = repo.Listar();
        var usuario = usuarios.FirstOrDefault(p => p.Login.Contains(model.Login));
        Hash hash = new Hash(SHA256.Create());

        if(usuario != null && hash.validarSenha(model.Senha, usuario.Senha))
        {
            HttpContext.Session.SetString("UsuarioLogado", model.Login);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, model.Login));
            identity.AddClaim(new Claim(ClaimTypes.Name, model.Login));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

            var autPrincipal = new ClaimsPrincipal(identity);

            var principal = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                IsPersistent = true,

            };

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(autPrincipal),
                principal
            );

            return RedirectToAction("Index");
        } else if(usuario != null && !hash.validarSenha(model.Senha, usuario.Senha)) {
            ViewBag.Errors = "Senha inválida";
            return View(model);
        }
        else
        {
            ViewBag.Errors = "Usuário não encontrado";
            return View(model);
        }

    }

    public IActionResult Sair() {
        HttpContext.SignOutAsync().GetAwaiter();
        return View("Login");
    }

    private bool ValidaLogin()
{
    if (HttpContext.User.Identity.IsAuthenticated)
    {
        return false;
    }
    else
    {
        return true;
    }
}
}

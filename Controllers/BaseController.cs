using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using prova2.Models;


namespace prova2.Controllers;

public class BaseController : Controller
{
    public BaseController()
    {
        
    }

    public IActionResult Index()
    {
        var login = HttpContext.Session.GetString("UsuarioLogado");
        if(login == null)
        {
            return RedirectToAction("Login");
        }
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

        if(usuarios.Any(p => p.Login.Equals(model.Login) && p.Senha.Equals(model.Senha)))
        {
            HttpContext.Session.SetString("UsuarioLogado", model.Login);
            return RedirectToAction("Index");
        }
        else
        {
            return View(model);
        }
    }
}

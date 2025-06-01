using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using prova2.Models;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
namespace prova2.Controllers;
[Authorize]

public class UsuariosController : Controller
{
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(ILogger<UsuariosController> logger)
    {
        _logger = logger;
    }

    public IActionResult Usuario()
    {
        return View();
    }

    public IActionResult Listar()
    {
        Repositorio<Usuario> repo = new Repositorio<Usuario>();
        List<Usuario> lista = repo.Listar();
        return View(lista);
    }

    [HttpGet]
    public IActionResult Usuario(int id = 0)
    {
        Repositorio<Usuario> repo = new Repositorio<Usuario>();
        Usuario usuario = repo.Buscar(id);

        usuario.Senha = "";

        return View(usuario);
    }

    [HttpPost]
    public IActionResult Usuario(Usuario model)
    {
        if(!ModelState.IsValid) {
            return View(model);
        }   
        Repositorio<Usuario> repo = new Repositorio<Usuario>();
        Hash hash = new Hash(SHA256.Create());
        model.Senha = hash.CriptografarSenha(model.Senha);
        if (model.Id != null) {
            repo.Atualizar(model);
        }
        return RedirectToAction("Listar");
    }

    public IActionResult Editar(int id)
    {
        Repositorio<Usuario> repo = new Repositorio<Usuario>();
        Usuario Usuario = repo.Buscar(id);
        
        if (Usuario == null) {
            return NotFound();
        }

        return View(Usuario);
    }

    [HttpPost]
    public IActionResult Editar(Usuario Usuario)
    {
        if(!ModelState.IsValid) {
            return View(Usuario);
        }
        Repositorio<Usuario> repo = new Repositorio<Usuario>();
        Hash hash = new Hash(SHA256.Create());
        Usuario.Senha = hash.CriptografarSenha(Usuario.Senha);
        repo.Atualizar(Usuario);

        return RedirectToAction("Listar");
    }

    public IActionResult Apagar(int id)
    {
        Repositorio<Usuario> repo = new Repositorio<Usuario>();
        Usuario Usuario = repo.Buscar(id);
        
        if (Usuario == null) {
            return NotFound();
        }

        return View(Usuario);
    }

    [HttpPost]
    public IActionResult Apagar(Usuario Usuario)
    {
        Repositorio<Usuario> repo = new Repositorio<Usuario>();
        repo.Remover(Usuario.Id);
        return RedirectToAction("Listar");
    }
}

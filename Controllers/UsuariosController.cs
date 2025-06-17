using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using prova2.Models;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace prova2.Controllers;

[Authorize]
public class UsuariosController : Controller
{
    private readonly ILogger<UsuariosController> _logHandler;

    public UsuariosController(ILogger<UsuariosController> logger)
    {
        _logHandler = logger;
    }

    public IActionResult Listar()
    {
        Repositorio<Usuario> gerenciarUsuarios = new Repositorio<Usuario>();
        List<Usuario> listaDeUsuarios = gerenciarUsuarios.Listar();
        return View(listaDeUsuarios);
    }

    [HttpGet]
    public IActionResult Criar()
    {

        return View("FormularioUsuario", new Usuario());
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        Repositorio<Usuario> gerenciarUsuarios = new Repositorio<Usuario>();
        Usuario usuarioDetalhe = gerenciarUsuarios.Buscar(id);

        if (usuarioDetalhe == null)
        {
            _logHandler.LogWarning($"Usuário com ID {id} não encontrado para edição.");
            return RedirectToAction("Listar", new { MensagemErro = "Usuário não encontrado para edição." });
        }

        usuarioDetalhe.Senha = "";
        return View("FormularioUsuario", usuarioDetalhe);
    }

    [HttpPost]
    public IActionResult Salvar(Usuario dadosUsuario)
    {
        if (!ModelState.IsValid)
        {
            foreach (var modelStateEntry in ModelState.Values)
            {
                foreach (var error in modelStateEntry.Errors)
                {
                    _logHandler.LogError($"Erro de Validação: {error.ErrorMessage}");
                }
            }
            foreach (var key in ModelState.Keys)
            {
                if (ModelState[key].Errors.Any())
                {
                    _logHandler.LogError($"Campo '{key}' com erro(s):");
                    foreach (var error in ModelState[key].Errors)
                    {
                        _logHandler.LogError($"- {error.ErrorMessage}");
                    }
                }
            }
            return View("FormularioUsuario", dadosUsuario);
        }

        Repositorio<Usuario> gerenciarUsuarios = new Repositorio<Usuario>();
        Hash criadorHash = new Hash(SHA256.Create());
        dadosUsuario.Senha = criadorHash.CriptografarSenha(dadosUsuario.Senha);

        bool isNewUser = dadosUsuario.Id == null || dadosUsuario.Id == 0;

        if (isNewUser)
        {
            var loginExistente = gerenciarUsuarios.Listar().FirstOrDefault(u => u.Login.Equals(dadosUsuario.Login, StringComparison.OrdinalIgnoreCase));
            if (loginExistente != null)
            {
                ModelState.AddModelError("Login", "Este login já está em uso.");
                return View("FormularioUsuario", dadosUsuario);
            }
            gerenciarUsuarios.Adicionar(dadosUsuario);
        }
        else
        {
            var loginExistente = gerenciarUsuarios.Listar().FirstOrDefault(u => u.Login.Equals(dadosUsuario.Login, StringComparison.OrdinalIgnoreCase) && u.Id != dadosUsuario.Id);
            if (loginExistente != null)
            {
                ModelState.AddModelError("Login", "Este login já está em uso por outro usuário.");
                return View("FormularioUsuario", dadosUsuario);
            }


            if (string.IsNullOrEmpty(dadosUsuario.Senha))
            {
                var usuarioOriginal = gerenciarUsuarios.Buscar(dadosUsuario.Id);
                if (usuarioOriginal != null)
                {
                    dadosUsuario.Senha = usuarioOriginal.Senha;
                }
            }
            gerenciarUsuarios.Atualizar(dadosUsuario);
        }

        return RedirectToAction("Listar");
    }

    [HttpGet]
    public IActionResult Apagar(int id)
    {
        Repositorio<Usuario> gerenciarUsuarios = new Repositorio<Usuario>();
        Usuario usuarioParaRemover = gerenciarUsuarios.Buscar(id);

        if (usuarioParaRemover == null)
        {
            return NotFound();
        }

        return View("ConfirmarApagarUsuario", usuarioParaRemover);
    }

    [HttpPost]
    [ActionName("Apagar")]
    public IActionResult ConfirmarApagar(int id)
    {
        Repositorio<Usuario> gerenciarUsuarios = new Repositorio<Usuario>();
        gerenciarUsuarios.Remover(id);
        return RedirectToAction("Listar");
    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using prova2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace prova2.Controllers;

using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System;
using System.IO;
using System.Text.Json;
using System.Globalization;
using System.Linq;
using System.Text;

[Authorize]
public class PessoasController : BaseController
{
    private readonly ILogger<Controller> _logger;
    private readonly IWebHostEnvironment _appEnvironment;

    public PessoasController(ILogger<PessoasController> logger, IWebHostEnvironment appEnvironment)
    {
        _logger = logger;
        _appEnvironment = appEnvironment;
    }

    public List<string> GetEstados()
    {
        string caminhoArquivo = @"Cidades.json";
        if (System.IO.File.Exists(caminhoArquivo))
        {
            string json = System.IO.File.ReadAllText(caminhoArquivo);
            var estadosWrapper = JsonSerializer.Deserialize<EstadosWrapper>(json);

            List<string> nomesEstados = new List<string>();

            if (estadosWrapper?.Estados != null)
            {
                foreach (var estadoDict in estadosWrapper.Estados)
                {
                    foreach (var estado in estadoDict)
                    {
                        nomesEstados.Add(estado.Key);
                    }
                }
            }
            else
            {
                _logger.LogWarning("estadosWrapper ou estadosWrapper.Estados é nulo");
            }

            return nomesEstados;
        }
        else
        {
            _logger.LogWarning($"Arquivo não encontrado: {caminhoArquivo}");
            return new List<string>();
        }
    }

    [HttpGet]
    public JsonResult GetCidades(string estado)
    {
        var cidades = GetCidadesPorEstado(estado);
        return Json(cidades);
    }

   
    public List<string> GetCidadesPorEstado(string nomeEstado)
    {
        if (string.IsNullOrEmpty(nomeEstado))
        {
            _logger.LogWarning("nomeEstado é nulo ou vazio");
            return new List<string>();
        }

        string caminhoArquivo = @"Cidades.json";
        if (System.IO.File.Exists(caminhoArquivo))
        {
            string json = System.IO.File.ReadAllText(caminhoArquivo);
            var estadosWrapper = JsonSerializer.Deserialize<EstadosWrapper>(json);

            if (estadosWrapper?.Estados != null)
            {
                string estadoNormalizado = nomeEstado.NormalizeString();
                foreach (var estadoDict in estadosWrapper.Estados)
                {
                    foreach (var estado in estadoDict.Keys)
                    {
                        if (estado.NormalizeString().Equals(estadoNormalizado, StringComparison.OrdinalIgnoreCase))
                        {
                            return estadoDict[estado];
                        }
                    }
                }
            }
        }

        return new List<string>();
    }

    [HttpGet]
    public IActionResult Pessoa(int id = 0)
    {
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();
        Pessoa pessoa = repo.Buscar(id);

        var nomesEstados = GetEstados();

        ViewBag.Estados = new SelectList(nomesEstados, pessoa?.Estado);

        if (pessoa?.Estado != null)
        {
            var cidades = GetCidadesPorEstado(pessoa.Estado);
            ViewBag.Cidades = new SelectList(cidades, pessoa.Cidade);
        }
        else
        {
            ViewBag.Cidades = new SelectList(new List<string>());
        }

        return View(pessoa);
    }

    public IActionResult Pessoa()
    {
        Repositorio<Pessoa> repoPessoa = new Repositorio<Pessoa>();
        var pessoas = repoPessoa.Listar();
        return View(pessoas);
    }

    public IActionResult Listar(string MensagemErro = null)
    {
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();
        List<Pessoa> pessoas = repo.Listar();
        ViewBag.MensagemErro = MensagemErro;
        
        return View(pessoas);
    }

    [HttpPost]
    public IActionResult Pessoa(Pessoa model, IFormFile anexo)
    {
        if(!ModelState.IsValid) {
            var nomesEstados = GetEstados();

            ViewBag.Estados = new SelectList(nomesEstados, model?.Estado);

            if (model?.Estado != null)
            {
                var cidades = GetCidadesPorEstado(model.Estado);
                ViewBag.Cidades = new SelectList(cidades, model.Cidade);
            }
            else
            {
                ViewBag.Cidades = new SelectList(new List<string>());
            }
            return View(model);
        }   
        string caminho = null;
        if (anexo != null && anexo.Length > 0)
        {
            caminho = _appEnvironment.WebRootPath + "/imagens/" + anexo.FileName;
            using(FileStream stream = new FileStream(caminho, FileMode.Create))
            {
                anexo.CopyTo(stream);
            }
            caminho = "/imagens/" + anexo.FileName;
        }
        model.Imagem = caminho;
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();

        List<Pessoa> pessoas = repo.Listar();
        var pessoaCodigoFiscalExistente = pessoas.FirstOrDefault(p => p.CodigoFiscal.Contains(model.CodigoFiscal));
        var pessoaInscricaoEstadualExistente = pessoas.FirstOrDefault(p => p.InscricaoEstadual.Contains(model.InscricaoEstadual));

        if (pessoaCodigoFiscalExistente != null && model.Id == null) {
            ViewBag.Errors = "Código Fiscal já existe";
            return View(model);
        }

        if (pessoaInscricaoEstadualExistente != null && model.Id == null) {
            ViewBag.Errors = "Inscrição Estadual já existe";
            return View(model);
        }

        if (pessoaCodigoFiscalExistente != null && model.Id != null && model.Id != pessoaCodigoFiscalExistente.Id) {
            ViewBag.Errors = "Código Fiscal já existe";
            return View(model);
        }

        if (pessoaInscricaoEstadualExistente != null && model.Id != null && model.Id != pessoaInscricaoEstadualExistente.Id) {
            ViewBag.Errors = "Inscrição Estadual já existe";
            return View(model);
        }

        if (model.InscricaoEstadual.Length > 15)
        {
            ViewBag.Errors = "Inscrição Estadual deve ter até 15 caracteres";
            return View(model);
        }

        if (model.Id != null && model.Id != 0) {

            var pessoaDados = pessoas.FirstOrDefault(p => p.Id == model.Id);

            if (model.Imagem == null && pessoaDados.Imagem != null) {
                model.Imagem = pessoaDados.Imagem;
            }

            repo.Atualizar(model);
        } else {
            try {
                repo.Adicionar(model);
            } catch (Exception ex) {
                _logger.LogError("Erro", ex);
                throw ex;
            }
        }
        return RedirectToAction("Listar");
    }

    public FileContentResult Exportar(int id) {
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();
        Pessoa model = repo.Buscar(id);
        string json = JsonSerializer.Serialize(model);

        return File(new System.Text.UTF8Encoding().GetBytes(json), "text/json", "Dados Json");
    }

    public IActionResult Apagar(int id)
    {
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();
        Pessoa pessoa = repo.Buscar(id);
        
        if (pessoa == null)
        {
            return NotFound();
        }

        return View(pessoa);
    }

    [HttpPost]
    public IActionResult Apagar(Pessoa pessoa)
    {
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();
        repo.Remover(pessoa.Id);
        return RedirectToAction("Listar");
    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using prova2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json; // Remover Reflection.Metadata.Ecma335 se não estiver em uso
using System;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;

[Authorize]

public class PessoasController : Controller
{
    private readonly ILogger<PessoasController> _logHandler;
    private readonly IWebHostEnvironment _envHost;

    public PessoasController(ILogger<PessoasController> logger, IWebHostEnvironment appEnvironment)
    {
        _logHandler = logger;
        _envHost = appEnvironment;
    }

    private const string _jsonEstadosPath = "Cidades.json";

    public List<string> ObterNomesEstados()
    {
        if (!System.IO.File.Exists(_jsonEstadosPath))
        {
            _logHandler.LogWarning($"Arquivo de estados não encontrado: {_jsonEstadosPath}");
            return new List<string>();
        }

        string contentJson = System.IO.File.ReadAllText(_jsonEstadosPath);
        var estadosData = JsonSerializer.Deserialize<EstadosWrapper>(contentJson);

        if (estadosData?.Estados == null || !estadosData.Estados.Any())
        {
            _logHandler.LogWarning("Não há estados disponíveis no arquivo JSON.");
            return new List<string>();
        }

        List<string> nomesUnicosEstados = new List<string>();
        foreach (var estadoEntry in estadosData.Estados)
        {
            foreach (var nomeEstadoKey in estadoEntry.Keys)
            {
                nomesUnicosEstados.Add(nomeEstadoKey);
            }
        }
        return nomesUnicosEstados;
    }

    [HttpGet]
    public JsonResult GetCidades(string estado)
    {
        var cidadesDoEstado = ObterCidadesPorNomeEstado(estado);
        return Json(cidadesDoEstado);
    }

    public List<string> ObterCidadesPorNomeEstado(string nomeDoEstado)
    {
        if (string.IsNullOrWhiteSpace(nomeDoEstado))
        {
            _logHandler.LogWarning("Nome do estado fornecido é nulo ou vazio.");
            return new List<string>();
        }

        if (!System.IO.File.Exists(_jsonEstadosPath))
        {
            return new List<string>();
        }

        string rawJsonContent = System.IO.File.ReadAllText(_jsonEstadosPath);
        var estadoContainer = JsonSerializer.Deserialize<EstadosWrapper>(rawJsonContent);

        if (estadoContainer?.Estados == null)
        {
            return new List<string>();
        }

        string normalizedStateName = nomeDoEstado;

        foreach (var stateDictionary in estadoContainer.Estados)
        {
            foreach (var stateKey in stateDictionary.Keys)
            {
                if (stateKey.Equals(normalizedStateName, StringComparison.OrdinalIgnoreCase))
                {
                    return stateDictionary[stateKey];
                }
            }
        }
        return new List<string>();
    }

    // Ação para exibir a lista de pessoas (a Home da Pessoas)
    public IActionResult Listar(string mensagemAlerta = null)
    {
        Repositorio<Pessoa> gerenciadorPessoas = new Repositorio<Pessoa>();
        List<Pessoa> listaDePessoas = gerenciadorPessoas.Listar();
        ViewBag.MensagemErro = mensagemAlerta;

        return View(listaDePessoas);
    }

    // Ação para exibir o formulário para criar uma NOVA pessoa
    [HttpGet]
    public IActionResult Criar()
    {
        var todosOsEstados = ObterNomesEstados();
        ViewBag.Estados = new SelectList(todosOsEstados); // Para nova pessoa, não há estado selecionado
        ViewBag.Cidades = new SelectList(new List<string>()); // Cidades vazias inicialmente

        return View("FormularioPessoa", new Pessoa()); // Renderiza a View FormularioPessoa
    }

    // Ação para exibir o formulário para EDITAR uma pessoa existente
    [HttpGet]
    public IActionResult Editar(int id)
    {
        Repositorio<Pessoa> gestorPessoas = new Repositorio<Pessoa>();
        Pessoa pessoaDetalhe = gestorPessoas.Buscar(id);

        if (pessoaDetalhe == null)
        {
            _logHandler.LogWarning($"Pessoa com ID {id} não encontrada para edição.");
            return RedirectToAction("Listar", new { MensagemErro = "Pessoa não encontrada para edição." });
        }

        var todosOsEstados = ObterNomesEstados();
        ViewBag.Estados = new SelectList(todosOsEstados, pessoaDetalhe.Estado);

        List<string> cidadesRelacionadas = new List<string>();
        if (pessoaDetalhe.Estado != null)
        {
            cidadesRelacionadas = ObterCidadesPorNomeEstado(pessoaDetalhe.Estado);
        }
        ViewBag.Cidades = new SelectList(cidadesRelacionadas, pessoaDetalhe.Cidade);

        return View("FormularioPessoa", pessoaDetalhe); // Renderiza a View FormularioPessoa com os dados da pessoa
    }

    // Ação para salvar (criar ou atualizar) uma pessoa
    [HttpPost]
    public IActionResult Salvar(Pessoa dadosPessoa, IFormFile? arquivoAnexo)
    {
        if (!ModelState.IsValid)
        {
            // Logar erros de validação para depuração
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

            var estadosDisponiveis = ObterNomesEstados();
            ViewBag.Estados = new SelectList(estadosDisponiveis, dadosPessoa?.Estado);

            List<string> cidadesDoEstado = new List<string>();
            if (dadosPessoa?.Estado != null)
            {
                cidadesDoEstado = ObterCidadesPorNomeEstado(dadosPessoa.Estado);
            }
            ViewBag.Cidades = new SelectList(cidadesDoEstado, dadosPessoa?.Cidade);
            return View("FormularioPessoa", dadosPessoa); // Retorna para a View FormularioPessoa com os dados e erros
        }

        string caminhoImagemSalva = null;
        if (arquivoAnexo != null && arquivoAnexo.Length > 0)
        {
            caminhoImagemSalva = Path.Combine(_envHost.WebRootPath, "imagens", arquivoAnexo.FileName);
            using (FileStream fileOutput = new FileStream(caminhoImagemSalva, FileMode.Create))
            {
                arquivoAnexo.CopyTo(fileOutput);
            }
            dadosPessoa.Imagem = "/imagens/" + arquivoAnexo.FileName;
        }

        Repositorio<Pessoa> gerenciarDadosPessoa = new Repositorio<Pessoa>();
        List<Pessoa> todasAsPessoas = gerenciarDadosPessoa.Listar();

        bool isNewRecord = dadosPessoa.Id == null || dadosPessoa.Id == 0;

        if (dadosPessoa.InscricaoEstadual != null && dadosPessoa.InscricaoEstadual.Length > 15)
        {
            ViewBag.Errors = "Inscrição Estadual deve ter no máximo 15 caracteres.";
            return View("FormularioPessoa", dadosPessoa);
        }

        var fiscalCodeMatch = todasAsPessoas.FirstOrDefault(p => p.CodigoFiscal != null && p.CodigoFiscal.Equals(dadosPessoa.CodigoFiscal, StringComparison.OrdinalIgnoreCase));
        var stateRegistrationMatch = todasAsPessoas.FirstOrDefault(p => p.InscricaoEstadual != null && p.InscricaoEstadual.Equals(dadosPessoa.InscricaoEstadual, StringComparison.OrdinalIgnoreCase));

        if (fiscalCodeMatch != null && (isNewRecord || fiscalCodeMatch.Id != dadosPessoa.Id))
        {
            ViewBag.Errors = "Código Fiscal já existe para outra pessoa.";
            return View("FormularioPessoa", dadosPessoa);
        }

        if (stateRegistrationMatch != null && (isNewRecord || stateRegistrationMatch.Id != dadosPessoa.Id))
        {
            ViewBag.Errors = "Inscrição Estadual já existe para outra pessoa.";
            return View("FormularioPessoa", dadosPessoa);
        }

        if (dadosPessoa.Id != null && dadosPessoa.Id != 0)
        {
            var pessoaOriginal = gerenciarDadosPessoa.Buscar(dadosPessoa.Id);
            if (dadosPessoa.Imagem == null && pessoaOriginal?.Imagem != null)
            {
                dadosPessoa.Imagem = pessoaOriginal.Imagem;
            }
            gerenciarDadosPessoa.Atualizar(dadosPessoa);
        }
        else
        {
            gerenciarDadosPessoa.Adicionar(dadosPessoa);
        }
        return RedirectToAction("Listar");
    }

    [HttpGet]
    public IActionResult Apagar(int id)
    {
        Repositorio<Pessoa> repositorioRemocao = new Repositorio<Pessoa>();
        Pessoa pessoaParaRemover = repositorioRemocao.Buscar(id);

        if (pessoaParaRemover == null)
        {
            return NotFound();
        }

        return View("ConfirmarApagar", pessoaParaRemover); // View de confirmação de exclusão
    }

    [HttpPost]
    [ActionName("Apagar")] // Garante que essa ação responda ao POST de /Pessoas/Apagar
    public IActionResult ConfirmarApagar(int id) // Recebe o ID do formulário de confirmação
    {
        Repositorio<Pessoa> repositorioDelete = new Repositorio<Pessoa>();
        repositorioDelete.Remover(id); // Remove usando o ID diretamente
        return RedirectToAction("Listar");
    }
}
using System.ComponentModel.DataAnnotations;
namespace prova2.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Pessoa : Entity
{
    [Required(ErrorMessage="O campo nome é obrigatório")]
    public string? Nome { get; set; }

    [Required(ErrorMessage="O campo Codigo Fiscal é obrigatório")]
    public string? CodigoFiscal { get; set; }

    [Required(ErrorMessage="O campo Inscricao Estadual é obrigatório")]
    public string? InscricaoEstadual { get; set; }

    [Required(ErrorMessage="O campo Nome Fantasia é obrigatório")]
    public string? NomeFantasia { get; set; }

    [Required(ErrorMessage="O campo Endereço é obrigatório")]
    public string? Endereco { get; set; }

    [Required(ErrorMessage="O campo Número é obrigatório")]
    public string? Numero { get; set; }

    [Required(ErrorMessage="O campo Bairro é obrigatório")]
    public string? Bairro { get; set; }

    [Required(ErrorMessage="O campo Cidade é obrigatório")]
    public string? Cidade { get; set; }

    [Required(ErrorMessage="O campo Estado é obrigatório")]
    public string? Estado { get; set; }

    [Required(ErrorMessage="O campo Data Nascimento é obrigatório")]
    public DateTime? DataNascimento { get; set; }

    public string ?Imagem{ get; set; }
}

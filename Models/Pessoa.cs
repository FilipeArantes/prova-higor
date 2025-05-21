using System.ComponentModel.DataAnnotations;
namespace prova2.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Pessoa : Entity
{
    [Required(ErrorMessage="O campo nome é obrigatório")]
    [Display(Name = "Nome")]
    public string? Nome { get; set; }

    [Required(ErrorMessage="O campo Codigo Fiscal é obrigatório")]
    [Display(Name = "Codigo Fiscal")]
    public string? CodigoFiscal { get; set; }

    [Required(ErrorMessage="O campo Inscricao Estadual é obrigatório")]
    [Display(Name = "Inscricao Estadual")]
    public string? InscricaoEstadual { get; set; }

    [Required(ErrorMessage="O campo Nome Fantasia é obrigatório")]
    [Display(Name = "Nome Fantasia")]
    public string? NomeFantasia { get; set; }

    [Required(ErrorMessage="O campo Endereço é obrigatório")]
    [Display(Name = "Endereço")]
    public string? Endereco { get; set; }

    [Required(ErrorMessage="O campo Número é obrigatório")]
    [Display(Name = "Número")]
    public string? Numero { get; set; }

    [Required(ErrorMessage="O campo Bairro é obrigatório")]
    [Display(Name = "Bairro")]
    public string? Bairro { get; set; }

    [Required(ErrorMessage="O campo Cidade é obrigatório")]
    [Display(Name = "Cidade")]
    public string? Cidade { get; set; }

    [Required(ErrorMessage="O campo Estado é obrigatório")]
    [Display(Name = "Estado")]
    public string? Estado { get; set; }

    [Required(ErrorMessage="O campo Data Nascimento é obrigatório")]
    [Display(Name = "Data Nascimento")]
    public DateTime? DataNascimento { get; set; }

    public string ?Imagem{ get; set; }
}

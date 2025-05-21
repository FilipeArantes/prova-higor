using System.ComponentModel.DataAnnotations;
namespace prova2.Models;
using System.ComponentModel.DataAnnotations.Schema;
public class Usuario : Entity
{
    [Required(ErrorMessage="O campo Login é obrigatório")]
    public string? Login {get; set;}
    [Required(ErrorMessage="O campo Senha é obrigatório")]
    public string? Senha {get; set;}   
}
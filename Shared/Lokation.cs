using System.ComponentModel.DataAnnotations;
namespace Shared;

public class Lokation
{
   public int _id { get; set; } 
   public string LokationNavn { get; set; }
   public string Adresse { get; set;}
   
   [Required(ErrorMessage = "Telefonnummer skal udfyldes.")]
   [RegularExpression(@"^\d{8}$", ErrorMessage = "Telefonnummeret skal bestå af præcis 8 tal.")]
   public string LokationTelefon { get; set; }
   public string LokationType { get; set; }
} 
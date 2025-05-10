using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Shared;

public class Lokation
{
   [BsonId] // Brug denne som ID i stedet for _id
   [BsonRepresentation(BsonType.Int32)] 
   public int LokationId { get; set; } 
   public string LokationNavn { get; set; }
   public string Adresse { get; set; }
   
   [Required(ErrorMessage = "Telefonnummer skal udfyldes.")]
   [RegularExpression(@"^\d{8}$", ErrorMessage = "Telefonnummeret skal bestå af præcis 8 tal.")]
   public string LokationTelefon { get; set; }
   public string LokationType { get; set; }
} 
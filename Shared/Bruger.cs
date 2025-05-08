using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Shared;

public class Bruger
{
    [BsonId] // Brug denne som ID i stedet for _id
    [BsonRepresentation(BsonType.Int32)] 
    public int BrugerId { get; set; }
    [Required(ErrorMessage = "Navn er påkrævet")]
    public string Navn { get; set; }
    [Required(ErrorMessage = "Email er påkrævet")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Adgangskode er påkrævet")]
    public string Adgangskode { get; set; }
    [Required(ErrorMessage = "Telefon-nr er påkrævet")]
    public int BrugerTelefon { get; set; }
    public string Rolle { get; set; }
    public string Billede { get; set; }
    [Required(ErrorMessage = "Startdato er påkrævet")]
    public DateOnly StartDato { get; set; }
    [Required(ErrorMessage = "Lokation er påkrævet")]
    public Lokation Koekken { get; set; }
}
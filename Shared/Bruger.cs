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
    
    [Required(ErrorMessage = "Telefonnummer skal udfyldes.")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "Telefonnummeret skal bestå af præcis 8 tal.")]
    public string BrugerTelefon { get; set; }
    
    public string Rolle { get; set; }
    
    public string Erhverv { get; set; }
    
    public bool Aktiv { get; set; } = true;
    public string Billede { get; set; }
    [Required(ErrorMessage = "Startdato er påkrævet")]
    public DateOnly StartDato { get; set; }
    
    public DateOnly SlutDato { get; set; }
    
    [Required(ErrorMessage = "Lokation er påkrævet")]
    public Lokation? Koekken { get; set; }
    public Elevplan? MinElevplan { get; set; }
}
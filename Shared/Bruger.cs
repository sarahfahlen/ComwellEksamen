using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared;
[BsonIgnoreExtraElements]

public class Bruger
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId _id { get; set; }
    public int BrugerId { get; set; }
    [Required(ErrorMessage = "Navn er påkrævet")]
    public string Navn { get; set; }
    [Required(ErrorMessage = "Email er påkrævet")]
    public string Email { get; set; }
    public string? Adgangskode { get; set; }
    
    [Required(ErrorMessage = "Telefonnummer skal udfyldes.")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "Telefonnummeret skal bestå af præcis 8 tal.")]
    public string BrugerTelefon { get; set; }
    
    public string Rolle { get; set; }
    
    public string? Erhverv { get; set; }
    
    public bool Aktiv { get; set; } = true;
    public string? Billede { get; set; }
    
    public DateOnly? StartDato { get; set; }
    
    public DateOnly? SlutDato { get; set;}
    
  
    public Lokation? Afdeling { get; set; }
    [JsonIgnore]
    public Elevplan? MinElevplan { get; set; }
}
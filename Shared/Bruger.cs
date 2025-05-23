using System.ComponentModel.DataAnnotations;

namespace Shared;
public class Bruger
{
    public int _id { get; set; }
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
    public int? AfdelingId { get; set; }
    public Elevplan? MinElevplan { get; set; }
}
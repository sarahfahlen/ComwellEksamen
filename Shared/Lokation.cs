using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Shared;

public class Lokation
{
   [BsonId] // Brug denne som ID i stedet for _id
   [BsonRepresentation(BsonType.Int32)] 
   public int LokationId { get; set; } 
   public string Navn { get; set; }
   public string Adresse { get; set; }
   public int Telefon { get; set; }
   public string Type { get; set; }
}
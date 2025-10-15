using System.ComponentModel.DataAnnotations.Schema;

namespace server.Entities;

[Table("companies")]
public class Company
{
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("symbol")]
    public string Symbol { get; set; } = null!;
    
    [Column("name")]
    public string Name { get; set; } = null!;
    

}
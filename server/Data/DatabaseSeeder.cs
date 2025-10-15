using server.Data;
using server.Entities;

public static class DataSeeder
{
    public static void SeedTopCompanies(AppDbContext db)
    {
        var topCompanies = new[]
        {
            new Company { Name = "NVIDIA", Symbol = "NVDA" },
            new Company { Name = "Microsoft", Symbol = "MSFT" },
            new Company { Name = "Apple", Symbol = "AAPL" },
            new Company { Name = "Alphabet", Symbol = "GOOGL" },
        };

        foreach (var c in topCompanies)
        {
            if (!db.Companies.Any(x => x.Symbol == c.Symbol))
                db.Companies.Add(c);
        }

        db.SaveChanges();
    }
}
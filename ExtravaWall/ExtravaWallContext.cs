using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Bogus;
using Bogus.Extensions;

namespace ExtravaWall;

public class ExtravaWallContext : DbContext {

    public ExtravaWallContext(DbContextOptions<ExtravaWallContext> options)
        : base(options) {
    }

    override protected void OnModelCreating(ModelBuilder builder) {
        FakeData.Init(100);
        builder.Entity<User>().HasData(FakeData.Users);
    }

    public DbSet<User> Users { get; set; }
}

public static class FakeData {
    public static List<User> Users = new List<User>();

    public static void Init(int count) {
        Randomizer.Seed = new Random(456473);
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid()))
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName));

        FakeData.Users.AddRange(userFaker.Generate(count));
    }
}


[Table("users")]
[Index(nameof(Email), IsUnique = true)]
public class User {

    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; }

    [Column("last_name")]
    public string LastName { get; set; }

    [Column("email")]
    public string Email { get; set; }

}
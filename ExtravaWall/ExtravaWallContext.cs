using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Bogus;
using Bogus.Extensions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace ExtravaWall;


public class ExtravaWallContext : DoltDbContextBase<ExtravaWallContext> {

    public ExtravaWallContext(IServiceProvider serviceProvider, ILogger logger, IDataSourceProvider dataSourceProvider, DbContextOptions<ExtravaWallContext> options)
        : base(serviceProvider, logger, dataSourceProvider, options) {
    }

    override protected void OnModelCreating(ModelBuilder builder) {
        _logger.Information("Creating database");
        base.OnModelCreating(builder);
    }

    public bool Seed() {
        if (!(_serviceProvider.GetService<IDatabaseCreator>() as RelationalDatabaseCreator)?.Exists() ?? false) {
            return false;
        }

        //Database.SetDbConnection(_dataSourceProvider.ResetConnection());
        if (!Branch("The Sacred Timeline")) {
            throw new Exception("Failed to branch");
            return false;
        }

        _logger.Information("Seeding database");
        if (!Users.Any()) {
            FakeData.Init(100);
            Users.AddRange(FakeData.Users);
            SaveChanges();
        }

        Commit("Seed commit");
        return true;
    }

    public DbSet<User> Users { get; set; }
}

public static class FakeData {
    private const int SEED = 546357;
    private static IDictionary<Guid, User> userDictionary = new Dictionary<Guid, User>();
    public static IEnumerable<User> Users => userDictionary.Values;
    public static void Init(int count) {
        var userFaker = new ExtravaFake<User>(SEED)
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName));
        userFaker.Generate(count).ForEach(u => userDictionary.TryAdd(u.Id, u));
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

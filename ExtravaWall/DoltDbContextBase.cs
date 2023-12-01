using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtravaWall;
public abstract class DoltDbContextBase<T> : DbContext where T : DbContext {
    protected IServiceProvider _serviceProvider;
    protected ILogger _logger;
    protected IDataSourceProvider _dataSourceProvider;

    public string CurrentBranch { get; private set; } = "main";
    public string PreviousBranch { get; private set; } = "";

    public DoltDbContextBase(IServiceProvider serviceProvider, ILogger logger, IDataSourceProvider dataSourceProvider, DbContextOptions<T> options)
        : base(options) {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _dataSourceProvider = dataSourceProvider;
    }

    override protected void OnModelCreating(ModelBuilder builder) {
        builder.Entity<SystemBranch>().ToTable("dolt_branches", t => t.ExcludeFromMigrations());
    }
    public DbSet<SystemBranch> SystemBranches { get; set; }

    public string MakeSafeBranchName(string branch) {
        return Regex.Replace(branch.ToLower(), @"[^a-z0-9\-_/]", "-");
    }

    public void CreateDbIfNotExists() {
        Database.ExecuteSql($"CREATE DATABASE IF NOT EXISTS `extravawall`; USE `extravawall`;");
        _dataSourceProvider.CurrentDataSource = "extravawall";
        Database.SetDbConnection(_dataSourceProvider.ResetConnection());
    }

    public bool Commit(string message) {
        if (!AddAll()) {
            _logger.Error("Failed to add all");
            return false;
        }

        var commitResult = Database.SqlQueryRaw<string>($"CALL DOLT_COMMIT('-A', '--allow-empty', '-m', '{message}');").AsEnumerable().FirstOrDefault();
        if (commitResult is null) {
            _logger.Error($"Failed to commit: {commitResult ?? "No hash"}");
            return false;
        } else {
            _logger.Error($"Commit successful: {commitResult}");
        }
        return true;
    }

    public bool AddAll() {
        var addResult = Database.SqlQueryRaw<int>($"CALL DOLT_ADD('-A');").AsEnumerable().FirstOrDefault();
        if (addResult != 0) {
            _logger.Error($"Failed to add: {addResult}");
            return false;
        }
        return true;
    }

    public bool Branch(string branch) {
        branch = MakeSafeBranchName(branch);
        if (SystemBranches.Any(b => b.Name == branch)) {
            _logger.Information($"Branch '{branch}' already exists; Switching to it.");
        } else {
            _logger.Information($"Branch '{branch}' does not exist; Creating it.");
            int createStatus = Database.SqlQueryRaw<int>($"CALL DOLT_BRANCH('-c', '{CurrentBranch}', '{branch}');").AsEnumerable().FirstOrDefault();
            if (createStatus != 0) {
                _logger.Error($"Failed to create branch '{branch}'");
                return false;
            }
        }

        return Checkout(branch);
    }

    public bool Checkout(string branch) {
        branch = MakeSafeBranchName(branch);
        var checkoutResult = Database.SqlQueryRaw<CommandResult>($"CALL DOLT_CHECKOUT('{branch}');").AsEnumerable().FirstOrDefault();
        if (checkoutResult is null || checkoutResult.status != 0) {
            _logger.Error($"Failed to checkout branch '{branch}': {checkoutResult?.message ?? "No message"}");
            return false;
        }

        if (Database.SqlQuery<string>($"select active_branch();").AsEnumerable().FirstOrDefault()?.CompareTo(branch) == 0) {
            _logger.Information($"Switched to branch '{branch}'");
            PreviousBranch = CurrentBranch;
            CurrentBranch = branch;
        } else {
            _logger.Error($"Failed to switch to branch '{branch}'");
            return false;
        }

        return true;
    }
}

public record CommandResult(int status, string message);

public class SystemBranch {

    [Key, Column("name")]
    public string Name { get; set; }

    [Column("hash")]
    public string Hash { get; set; }

    [Column("latest_committer")]
    public string LatestCommitter { get; set; }

    [Column("latest_committer_email")]
    public string LatestCommitterEmail { get; set; }

    [Column("latest_commit_date")]
    public DateTime LatestCommitDate { get; set; }

    [Column("latest_commit_message")]
    public string LatestCommitMessage { get; set; }

    [Column("remote")]
    public string Remote { get; set; }

    [Column("branch")]
    public string Branch { get; set; }
}

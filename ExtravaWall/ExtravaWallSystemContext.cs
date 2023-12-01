using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Bogus;
using Bogus.Extensions;
using System.Text.RegularExpressions;

namespace ExtravaWall;

public class ExtravaWallSystemContext : DoltDbContextBase<ExtravaWallSystemContext> {

    public ExtravaWallSystemContext(IServiceProvider serviceProvider, ILogger logger, IDataSourceProvider dataSourceProvider, DbContextOptions<ExtravaWallSystemContext> options)
        : base(serviceProvider, logger, dataSourceProvider, options) {
    }

    public void CreateDbIfNotExists() {
        Database.ExecuteSql($"CREATE DATABASE IF NOT EXISTS `extravawall`; USE `extravawall`;");
        _dataSourceProvider.CurrentDataSource = "extravawall";
        Database.SetDbConnection(_dataSourceProvider.ResetConnection());
    }
}
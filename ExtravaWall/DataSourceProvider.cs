using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus.DataSets;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace ExtravaWall {

    public class DataSourceProvider : IDataSourceProvider {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private MySqlConnection? _connection;

        public string CurrentDataSource { get; set; }
        public DataSourceProvider(IServiceProvider serviceprovider, ILogger logger, IConfiguration configuration) {
            _serviceProvider = serviceprovider;
            _logger = logger;
            _configuration = configuration;
        }

        public string GetConnectionString() {
            if (string.IsNullOrWhiteSpace(CurrentDataSource)) {
                CurrentDataSource = "";
            }

            string? configConnection = _configuration.GetConnectionString("Primary");
            if (string.IsNullOrWhiteSpace(configConnection)) {
                _logger.Error("No connection string found in configuration");
                throw new Exception("No connection string found in configuration");
            }

            configConnection = configConnection?.Replace("DATABASE_NAME", CurrentDataSource) ?? "";
            _logger.Information($"Using connection string: {configConnection}");
            return configConnection;
        }

        public MySqlConnection GetConnection() {
            var connectionString = GetConnectionString();
            if (_connection is null) {
                _connection = new MySqlConnection(connectionString);
            }
            return _connection;
        }

        public MySqlConnection ResetConnection() {
            _connection = null;
            return GetConnection();
        }
    }
    public interface IDataSourceProvider {
        string CurrentDataSource { set; get; }
        string GetConnectionString();

        MySqlConnection GetConnection();
        MySqlConnection ResetConnection();
    }
}
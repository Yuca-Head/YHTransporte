using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;

public sealed class DbConnectionFactory(IConfiguration configuration)
{
    private enum Databases
    {
        Default,
        Backup
    }

    public string ConnectionString => CreateBuilder(Databases.Backup).ConnectionString ?? 
    throw new InvalidOperationException("Connection string not configured.");
    private SqlConnectionStringBuilder CreateBuilder(Databases databases)
    => databases switch
    {
        Databases.Backup => new SqlConnectionStringBuilder
        {
            DataSource = configuration["BackupDatabase:Server"],
            InitialCatalog = configuration["BackupDatabase:Name"],
            UserID = configuration["BackupDatabase:User"],
            Password = configuration["BackupDatabase:Password"],
            Encrypt = true
        },
        _ => new SqlConnectionStringBuilder
        {
            DataSource = configuration["YHDatabase:Server"],
            InitialCatalog = configuration["YHDatabase:Name"],
            UserID = configuration["YHDatabase:User"],
            Password = configuration["YHDatabase:Password"],
            Encrypt = true
        }
    };

    public SqlConnection Create()
    => new(ConnectionString);
}
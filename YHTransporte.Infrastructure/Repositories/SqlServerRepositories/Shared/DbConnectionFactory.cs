using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.Shared;

public sealed class DbConnectionFactory(IConfiguration configuration)
{
    
    private readonly string _connectionString =
            configuration.GetConnectionString("AzureConnection")
            ?? throw new InvalidOperationException(
                "Connection string not configured.");   

    public SqlConnection Create()
        => new(_connectionString);
}
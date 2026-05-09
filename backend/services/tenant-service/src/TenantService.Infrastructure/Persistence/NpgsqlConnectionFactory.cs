using ClinicSaaS.BuildingBlocks.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace TenantService.Infrastructure.Persistence;

/// <summary>
/// Factory Npgsql resolve connection string từ options/env và mở kết nối PostgreSQL.
/// </summary>
public sealed class NpgsqlConnectionFactory : IPostgreSqlConnectionFactory
{
    private readonly PostgreSqlOptions _options;

    /// <summary>
    /// Khởi tạo factory với PostgreSQL options đã bind từ configuration.
    /// </summary>
    /// <param name="options">Options chứa connection string hoặc tên biến môi trường.</param>
    public NpgsqlConnectionFactory(IOptions<PostgreSqlOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Tạo kết nối PostgreSQL đã mở cho repository sử dụng.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác mở kết nối.</param>
    /// <returns>Kết nối Npgsql đã mở, caller chịu trách nhiệm dispose.</returns>
    public async ValueTask<NpgsqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = ResolveConnectionString();
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        return connection;
    }

    private string ResolveConnectionString()
    {
        var fromEnvironment = string.IsNullOrWhiteSpace(_options.ConnectionStringEnvironmentVariable)
            ? null
            : Environment.GetEnvironmentVariable(_options.ConnectionStringEnvironmentVariable);
        var connectionString = string.IsNullOrWhiteSpace(fromEnvironment)
            ? _options.ConnectionString
            : fromEnvironment;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"PostgreSQL connection string is missing. Set '{_options.ConnectionStringEnvironmentVariable}' or PostgreSql:ConnectionString.");
        }

        return connectionString;
    }
}

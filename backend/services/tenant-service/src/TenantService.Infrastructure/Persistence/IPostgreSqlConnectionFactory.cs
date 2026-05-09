using Npgsql;

namespace TenantService.Infrastructure.Persistence;

/// <summary>
/// Factory tạo kết nối PostgreSQL đã mở cho Tenant Service persistence.
/// </summary>
public interface IPostgreSqlConnectionFactory
{
    /// <summary>
    /// Tạo và mở kết nối PostgreSQL bằng connection string đã resolve từ config/env.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác mở kết nối.</param>
    /// <returns>Kết nối Npgsql đã mở, caller chịu trách nhiệm dispose.</returns>
    ValueTask<NpgsqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}

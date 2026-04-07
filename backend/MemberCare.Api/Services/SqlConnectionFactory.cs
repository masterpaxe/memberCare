using Npgsql;

namespace MemberCare.Api.Services;

public sealed class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MemberCareDb")
            ?? throw new InvalidOperationException("Connection string 'MemberCareDb' is not configured.");
    }

    public NpgsqlConnection CreateOpenConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}

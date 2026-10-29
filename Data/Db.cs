using Microsoft.Data.SqlClient;

namespace EcommerceInmobiliaria.Data;

/// <summary>Pequeño ayudante ADO.NET con consultas parametrizadas (evita inyección SQL).</summary>
public class Db
{
    private readonly string _cs;

    public Db(IConfiguration config)
    {
        _cs = config.GetConnectionString("Default")
              ?? throw new InvalidOperationException("Falta ConnectionStrings:Default en appsettings.json");
    }

    public static SqlParameter P(string name, object? value) => new SqlParameter(name, value ?? DBNull.Value);

    public async Task<List<T>> QueryAsync<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] ps)
    {
        await using var cn = new SqlConnection(_cs);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddRange(ps);
        await using var rd = await cmd.ExecuteReaderAsync();
        var list = new List<T>();
        while (await rd.ReadAsync()) list.Add(map(rd));
        return list;
    }

    public async Task<T?> QueryFirstAsync<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] ps) where T : class
    {
        var list = await QueryAsync(sql, map, ps);
        return list.FirstOrDefault();
    }

    public async Task<int> ExecAsync(string sql, params SqlParameter[] ps)
    {
        await using var cn = new SqlConnection(_cs);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddRange(ps);
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<T> ScalarAsync<T>(string sql, params SqlParameter[] ps)
    {
        await using var cn = new SqlConnection(_cs);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddRange(ps);
        var o = await cmd.ExecuteScalarAsync();
        if (o == null || o == DBNull.Value) return default!;
        return (T)Convert.ChangeType(o, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
    }

    public async Task InTransactionAsync(Func<SqlConnection, SqlTransaction, Task> work)
    {
        await using var cn = new SqlConnection(_cs);
        await cn.OpenAsync();
        await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();
        try
        {
            await work(cn, tx);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}

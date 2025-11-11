using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public interface IDbConnectionFactory
{
    IDbConnection CreateWrite();
    IDbConnection CreateRead();
}

public sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _w, _r;
    public DbConnectionFactory(IConfiguration cfg)
    {
        _w = cfg.GetConnectionString("WriteConnection")!;
        _r = cfg.GetConnectionString("ReadConnection")!;
    }
    public IDbConnection CreateWrite() => new SqlConnection(_w);
    public IDbConnection CreateRead() => new SqlConnection(_r);
}

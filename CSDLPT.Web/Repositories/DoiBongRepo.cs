using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using CSDLPT.Web.Models;

public sealed class DoiBongRepo : IDoiBongRepo
{
    private readonly IDbConnectionFactory _f;
    public DoiBongRepo(IDbConnectionFactory f) => _f = f;

    // READ (ReadConnection): lấy từ v_DOIBONG
    public async Task<IEnumerable<DoiBong>> GetAllAsync()
    {
        using var db = _f.CreateRead();
        const string sql = @"SELECT MaDB, TenDB, MaSan, HLV, CLB FROM dbo.v_DOIBONG";
        return await db.QueryAsync<DoiBong>(sql);
    }

    public async Task<DoiBong?> FindAsync(string maDb, string clb)
    {
        using var db = _f.CreateRead();
        const string sql = @"SELECT MaDB,TenDB,MaSan,HLV,CLB
                         FROM dbo.v_DOIBONG
                         WHERE MaDB=@MaDB AND CLB=@CLB";
        return await db.QuerySingleOrDefaultAsync<DoiBong>(sql, new { MaDB = maDb, CLB = clb });
    }

    // WRITE (WriteConnection): ghi vào view → trigger RPC
    public async Task<int> CreateAsync(DoiBong x)
    {
        using var db = _f.CreateWrite();
        const string sql = @"INSERT dbo.v_DOIBONG(MaDB, TenDB, MaSan, HLV, CLB)
                             VALUES (@MaDB, @TenDB, @MaSan, @HLV, @CLB)";
        return await db.ExecuteAsync(sql, x);
    }

    public async Task<int> UpdateAsync(DoiBong x)
    {
        using var db = _f.CreateWrite();
        // Không cho đổi CLB (khóa định tuyến) — chỉ update các cột còn lại
        const string sql = @"UPDATE dbo.v_DOIBONG
                             SET TenDB=@TenDB, MaSan=@MaSan, HLV=@HLV
                             WHERE MaDB=@MaDB";
        return await db.ExecuteAsync(sql, x);
    }

    public async Task<int> DeleteAsync(string maDb)
    {
        using var db = _f.CreateWrite();
        const string sql = @"DELETE dbo.v_DOIBONG WHERE MaDB=@MaDB";
        return await db.ExecuteAsync(sql, new { MaDB = maDb });
    }
}

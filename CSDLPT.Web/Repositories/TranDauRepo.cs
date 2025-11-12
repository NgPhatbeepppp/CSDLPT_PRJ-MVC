// file: Repositories/TranDauRepo.cs
using CSDLPT.Web.Infrastructure;
using CSDLPT.Web.Models;
using Dapper;
using System.Data;

namespace CSDLPT.Web.Repositories
{
    public class TranDauRepo : ITranDauRepo
    {
        // Đã đổi tên _dbFactory thành _connectionFactory để đồng nhất
        private readonly IDbConnectionFactory _connectionFactory;

        public TranDauRepo(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// ĐỌC TOÀN CỤC (Global Read)
        /// </summary>
        public async Task<IEnumerable<TranDau>> GetAllAsync()
        {
            // Luôn dùng WriteConnection để đọc global view (v_TRANDAU) trên Coordinator
            using var conn = _connectionFactory.CreateWriteConnection();
            const string sql = "SELECT MaTD, NgayThiDau, MaSan, LuotDau, VongDau FROM dbo.v_TRANDAU";
            return await conn.QueryAsync<TranDau>(sql);
        }

        /// <summary>
        /// ĐỌC TOÀN CỤC (Global Read)
        /// </summary>
        public async Task<TranDau?> GetByIdAsync(string maTD)
        {
            // Luôn dùng WriteConnection để đọc global view (v_TRANDAU) trên Coordinator
            using var conn = _connectionFactory.CreateWriteConnection();
            const string sql = "SELECT MaTD, NgayThiDau, MaSan, LuotDau, VongDau FROM dbo.v_TRANDAU WHERE MaTD = @MaTD";
            return await conn.QueryFirstOrDefaultAsync<TranDau>(sql, new { MaTD = maTD });
        }

        /// <summary>
        /// GHI (Write) - Chạy trigger INSERT trên Coordinator
        /// </summary>
        public async Task CreateAsync(TranDau tranDau)
        {
            using var conn = _connectionFactory.CreateWriteConnection();

            // MaTD được CSDL (trigger) tự sinh, C# không chèn vào
            const string sql = @"
                INSERT INTO dbo.v_TRANDAU (NgayThiDau, MaSan, LuotDau, VongDau) 
                VALUES (@NgayThiDau, @MaSan, @LuotDau, @VongDau)";

            await conn.ExecuteAsync(sql, tranDau);
        }

        /// <summary>
        /// GHI (Write) - Chạy trigger UPDATE trên Coordinator
        /// </summary>
        public async Task UpdateAsync(TranDau tranDau)
        {
            using var conn = _connectionFactory.CreateWriteConnection();

            const string sql = @"
                UPDATE dbo.v_TRANDAU 
                SET NgayThiDau = @NgayThiDau, 
                    LuotDau = @LuotDau, 
                    VongDau = @VongDau
                WHERE MaTD = @MaTD";

            await conn.ExecuteAsync(sql, tranDau);
        }

        /// <summary>
        /// GHI (Write) - Chạy trigger DELETE trên Coordinator
        /// </summary>
        public async Task DeleteAsync(string maTD)
        {
            using var conn = _connectionFactory.CreateWriteConnection();
            const string sql = "DELETE FROM dbo.v_TRANDAU WHERE MaTD = @MaTD";
            await conn.ExecuteAsync(sql, new { MaTD = maTD });
        }
    }
}
/* Nội dung file: CSDLPT.Web/Repositories/TranDauRepo.cs 
Mục đích: Cập nhật CreateAsync (MaTD tự sinh) và UpdateAsync
*/
using CSDLPT.Web.Infrastructure;
using CSDLPT.Web.Models;
using Dapper;
using System.Data;

namespace CSDLPT.Web.Repositories
{
    public class TranDauRepo : ITranDauRepo
    {
        private readonly IDbConnectionFactory _connectionFactory;

        // Mọi thao tác (Đọc v_* và Ghi v_*) đều qua Coordinator
        private IDbConnection Connection => _connectionFactory.CreateWriteConnection();

        public TranDauRepo(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<TranDau>> GetAllAsync()
        {
            using (var conn = Connection)
            {
                return await conn.QueryAsync<TranDau>("SELECT * FROM v_TRANDAU");
            }
        }

        public async Task<TranDau?> GetByIdAsync(string maTD)
        {
            using (var conn = Connection)
            {
                var query = "SELECT * FROM v_TRANDAU WHERE MaTD = @Id";
                return await conn.QuerySingleOrDefaultAsync<TranDau>(query, new { Id = maTD });
            }
        }

        // === PHIÊN BẢN CHÍNH XÁC (Sửa lỗi) ===
        public async Task CreateAsync(TranDau tranDau)
        {
            using (var conn = Connection)
            {
                // KHÔNG INSERT MaTD (vì CSDL tự sinh)
                // Trigger sẽ route dựa trên MaSan
                var sql_insert = @"INSERT INTO v_TRANDAU (NgayThiDau, MaSan, LuotDau, VongDau, MaDoiNha, MaDoiKhach) 
                                    VALUES (@NgayThiDau, @MaSan, @LuotDau, @VongDau, @MaDoiNha, @MaDoiKhach)";

                await conn.ExecuteAsync(sql_insert, tranDau);
            }
        }

        // === CẬP NHẬT PHƯƠNG THỨC NÀY ===
        public async Task UpdateAsync(TranDau tranDau)
        {
            using (var conn = Connection)
            {
                var sql = @"UPDATE v_TRANDAU 
                            SET NgayThiDau = @NgayThiDau, 
                                MaSan = @MaSan, 
                                LuotDau = @LuotDau, 
                                VongDau = @VongDau,
                                MaDoiNha = @MaDoiNha,
                                MaDoiKhach = @MaDoiKhach
                            WHERE MaTD = @MaTD"; // MaTD không đổi

                await conn.ExecuteAsync(sql, tranDau);
            }
        }

        public async Task DeleteAsync(string maTD)
        {
            using (var conn = Connection)
            {
                var sql = "DELETE FROM v_TRANDAU WHERE MaTD = @Id";
                await conn.ExecuteAsync(sql, new { Id = maTD });
            }
        }
    }
}
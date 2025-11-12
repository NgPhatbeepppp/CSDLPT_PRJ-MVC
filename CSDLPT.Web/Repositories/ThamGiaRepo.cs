/* Nội dung file: CSDLPT.Web/Repositories/ThamGiaRepo.cs 
Mục đích: Triển khai 'async' cho THAMGIA, dùng CreateWriteConnection
*/
using CSDLPT.Web.Infrastructure;
using CSDLPT.Web.Models;
using Dapper;
using System.Data;

namespace CSDLPT.Web.Repositories
{
    public class ThamGiaRepo : IThamGiaRepo
    {
        private readonly IDbConnectionFactory _connectionFactory;

        // Mọi thao tác (Đọc v_* và Ghi v_*) đều qua Coordinator
        private IDbConnection Connection => _connectionFactory.CreateWriteConnection();

        public ThamGiaRepo(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ThamGia>> GetByMaTDAsync(string maTD)
        {
            using (var conn = Connection) // Đọc toàn cục
            {
                var query = "SELECT * FROM v_THAMGIA WHERE MaTD = @MaTD";
                return await conn.QueryAsync<ThamGia>(query, new { MaTD = maTD });
            }
        }

        public async Task<int> CreateAsync(ThamGia thamGia)
        {
            using (var conn = Connection) // Ghi toàn cục
            {
                var sql = @"INSERT INTO v_THAMGIA (MaTD, MaCT, SoTrai) 
                            VALUES (@MaTD, @MaCT, @SoTrai)";
                return await conn.ExecuteAsync(sql, thamGia);
            }
        }

        public async Task<int> UpdateAsync(ThamGia thamGia)
        {
            using (var conn = Connection) // Ghi toàn cục
            {
                var sql = @"UPDATE v_THAMGIA 
                            SET SoTrai = @SoTrai
                            WHERE MaTD = @MaTD AND MaCT = @MaCT";
                return await conn.ExecuteAsync(sql, thamGia);
            }
        }

        public async Task<int> DeleteAsync(string maTD, string maCT)
        {
            using (var conn = Connection) // Ghi toàn cục
            {
                var sql = @"DELETE FROM v_THAMGIA 
                            WHERE MaTD = @MaTD AND MaCT = @MaCT";
                return await conn.ExecuteAsync(sql, new { MaTD = maTD, MaCT = maCT });
            }
        }
    }
}
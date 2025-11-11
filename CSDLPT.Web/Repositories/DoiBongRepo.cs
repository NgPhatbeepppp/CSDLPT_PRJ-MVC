using CSDLPT.Web.Models;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CSDLPT.Web.Repositories
{
    public class DoiBongRepo : IDoiBongRepo
    {
        private readonly IDbConnectionFactory _connectionFactory;

        // 1. DbConnectionFactory đã được tiêm (inject) vào (nhờ Program.cs)
        public DoiBongRepo(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // 2. === CẬP NHẬT HÀM GETALLASYNC ===
        public async Task<IEnumerable<DoiBong>> GetAllAsync(bool isGlobal = false)
        {
            if (isGlobal)
            {
                // --- KỊCH BẢN 1: ĐỌC TOÀN CỤC (GLOBAL READ) ---
                // Dùng kết nối Coordinator
                using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
                {
                    // Truy vấn vào VIEW TOÀN CỤC (v_DOIBONG từ Giai đoạn 4)
                    const string sqlGlobal = "SELECT * FROM dbo.v_DOIBONG ORDER BY TenDB";
                    // Lỗi (ví dụ: timeout) sẽ tự động ném ra để Controller bắt
                    return await connection.QueryAsync<DoiBong>(sqlGlobal);
                }
            }
            else
            {
                // --- KỊCH BẢN 2: ĐỌC CỤC BỘ (LOCAL READ - MẶC ĐỊNH) ---
                // Dùng kết nối Local Fragment
                using (var connection = _connectionFactory.CreateConnection(ConnectionType.ReadLocalFragment))
                {
                    // Truy vấn thẳng vào BẢNG MẢNH CỤC BỘ
                    // Thêm cột 'Node' để UI biết đây là dữ liệu cục bộ
                    const string sqlLocal = "SELECT *, 'C' AS Node FROM dbo.DOIBONG_C ORDER BY TenDB";
                    return await connection.QueryAsync<DoiBong>(sqlLocal);
                }
            }
        }

        // 3. === CẬP NHẬT CÁC HÀM GHI (WRITE OPERATIONS) ===
        // Các hàm này BẮT BUỘC phải dùng kết nối Coordinator và VIEW TOÀN CỤC

        public async Task<int> CreateAsync(DoiBong doiBong)
        {
            // Luôn dùng kết nối Coordinator để kích hoạt RPC
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                // Luôn GHI vào VIEW TOÀN CỤC
                const string sql = @"
                    INSERT INTO dbo.v_DOIBONG (MaDB, TenDB, CLB)
                    VALUES (@MaDB, @TenDB, @CLB);";
                return await connection.ExecuteAsync(sql, doiBong);
            }
        }

        public async Task<int> UpdateAsync(DoiBong doiBong)
        {
            // Luôn dùng kết nối Coordinator
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                // Luôn UPDATE trên VIEW TOÀN CỤC
                const string sql = @"
                    UPDATE dbo.v_DOIBONG
                    SET TenDB = @TenDB, CLB = @CLB
                    WHERE MaDB = @MaDB;";
                return await connection.ExecuteAsync(sql, doiBong);
            }
        }

        public async Task<int> DeleteAsync(string id)
        {
            // Luôn dùng kết nối Coordinator
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                // Luôn DELETE trên VIEW TOÀN CỤC
                const string sql = "DELETE FROM dbo.v_DOIBONG WHERE MaDB = @MaDB;";
                return await connection.ExecuteAsync(sql, new { MaDB = id });
            }
        }

        // 4. === CẬP NHẬT HÀM GETBYID (DÙNG CHO EDIT/DELETE) ===
        // Hàm này nên đọc từ View Toàn Cục để đảm bảo lấy đúng dữ liệu
        // (ngay cả khi item đó thuộc Node A hoặc B)
        public async Task<DoiBong> GetByIdAsync(string id)
        {
            // Dùng kết nối Coordinator
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                // Truy vấn VIEW TOÀN CỤC
                const string sql = "SELECT * FROM dbo.v_DOIBONG WHERE MaDB = @MaDB;";
                return await connection.QuerySingleOrDefaultAsync<DoiBong>(sql, new { MaDB = id });
            }
        }
    }
}
using CSDLPT.Web.Models;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
namespace CSDLPT.Web.Repositories
{
    public class DoiBongRepo : IDoiBongRepo
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly string _localNode; // [THÊM VÀO] 2. Biến lưu tên Node

        // [SỬA LẠI] 3. Cập nhật Constructor để tiêm IConfiguration
        public DoiBongRepo(IDbConnectionFactory connectionFactory, IConfiguration configuration)
        {
            _connectionFactory = connectionFactory;

            // Đọc tên Node từ appsettings.json (Bước 2)
            // Nếu không tìm thấy, mặc định là "UNK" (Unknown)
            _localNode = configuration.GetValue<string>("SiteConfig:Node") ?? "UNK";
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
                    // Truy vấn vào VIEW TOÀN CỤC (đã tạo ở Bước 1)
                    const string sqlGlobal = "SELECT * FROM dbo.v_DOIBONG ORDER BY TenDB";

                    // Lỗi sẽ tự ném ra nếu Bước 1 chưa chạy
                    return await connection.QueryAsync<DoiBong>(sqlGlobal);
                }
            }
            else
            {
                // --- KỊCH BẢN 2: ĐỌC CỤC BỘ (LOCAL READ - MẶC ĐỊNH) ---
                // Dùng kết nối Local Fragment
                using (var connection = _connectionFactory.CreateConnection(ConnectionType.ReadLocalFragment))
                {
                    // [SỬA LẠI] 4. Sửa 2 lỗi:
                    // 1. Dùng tên bảng đúng là 'dbo.DOIBONG' (theo file ...DDL_v3.sql)
                    // 2. Dùng biến '_localNode' thay vì hardcode 'C'
                    var sqlLocal = $"SELECT *, '{_localNode}' AS Node FROM dbo.DOIBONG ORDER BY TenDB";

                    return await connection.QueryAsync<DoiBong>(sqlLocal);
                }
            }
        }

        // 3. === CÁC HÀM GHI (WRITE OPERATIONS) ===
        // Các hàm này (Create, Update, Delete) đã đúng logic.
        // Chúng chỉ thất bại trước đó vì View 'v_DOIBONG' (ở Bước 1) chưa tồn tại.
        // Chúng ta không cần sửa C# ở đây.

        public async Task<int> CreateAsync(DoiBong doiBong)
        {
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                const string sql = @"
                    INSERT INTO dbo.v_DOIBONG (MaDB, TenDB, CLB)
                    VALUES (@MaDB, @TenDB, @CLB);";
                return await connection.ExecuteAsync(sql, doiBong);
            }
        }

        public async Task<int> UpdateAsync(DoiBong doiBong)
        {
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                const string sql = @"
                    UPDATE dbo.v_DOIBONG
                    SET TenDB = @TenDB, CLB = @CLB
                    WHERE MaDB = @MaDB;";
                return await connection.ExecuteAsync(sql, doiBong);
            }
        }

        public async Task<int> DeleteAsync(string id)
        {
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                const string sql = "DELETE FROM dbo.v_DOIBONG WHERE MaDB = @MaDB;";
                return await connection.ExecuteAsync(sql, new { MaDB = id });
            }
        }

        // 4. === HÀM GETBYID ===
        // Hàm này cũng đã đúng logic, không cần sửa C#.
        public async Task<DoiBong> GetByIdAsync(string id)
        {
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                const string sql = "SELECT * FROM dbo.v_DOIBONG WHERE MaDB = @MaDB;";
                return await connection.QuerySingleOrDefaultAsync<DoiBong>(sql, new { MaDB = id });
            }
        }
    }
}
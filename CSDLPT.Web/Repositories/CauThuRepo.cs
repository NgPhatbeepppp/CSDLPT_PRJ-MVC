using CSDLPT.Web.Models;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CSDLPT.Web.Infrastructure;
namespace CSDLPT.Web.Repositories
{
    // Lớp này triển khai Interface ICauThuRepo
    public class CauThuRepo : ICauThuRepo
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly string _localNode;

        // Sử dụng DI (Dependency Injection) để inject Factory và Configuration
        public CauThuRepo(IDbConnectionFactory connectionFactory, IConfiguration configuration)
        {
            _connectionFactory = connectionFactory;
            // Đọc Node hiện tại (A, B, hay C) từ appsettings.json
            _localNode = configuration.GetValue<string>("SiteConfig:Node") ?? "UNK";
        }

        // --- HÀM ĐỌC DANH SÁCH (READ ALL) ---
        public async Task<IEnumerable<CauThu>> GetAllAsync(bool isGlobal = false)
        {
            if (isGlobal)
            {
                // 1. Chế độ TOÀN CỤC:
                // Kết nối đến Coordinator (Node C)
                using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
                {
                    // Luôn đọc từ View toàn cục v_CAUTHU
                    const string sqlGlobal = "SELECT * FROM dbo.v_CAUTHU ORDER BY HoTen";
                    return await connection.QueryAsync<CauThu>(sqlGlobal);
                }
            }
            else
            {
                // 2. Chế độ CỤC BỘ (Mặc định):
                // Kết nối đến Mảnh cục bộ (Node A, B, hoặc C)
                using (var connection = _connectionFactory.CreateConnection(ConnectionType.ReadLocalFragment))
                {
                    // Đọc trực tiếp từ bảng CAUTHU tại mảnh
                    // và gán tên Node cục bộ vào kết quả
                    var sqlLocal = $"SELECT *, '{_localNode}' AS Node FROM dbo.CAUTHU ORDER BY HoTen";
                    return await connection.QueryAsync<CauThu>(sqlLocal);
                }
            }
        }

        // --- HÀM TẠO MỚI (CREATE) ---
        // [QUAN TRỌNG] Phải chạy qua Coordinator
        public async Task<int> CreateAsync(CauThu cauThu)
        {
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                // INSERT vào View v_CAUTHU tại Coordinator
                // Trigger tg_v_CAUTHU_INS sẽ bắt lệnh này
                // và gọi RPC đến SP tại Node A/B/C tương ứng
                const string sql = @"
                    INSERT INTO dbo.v_CAUTHU (MaCT, HoTen, ViTri, NgaySinh, MaDB, SoAo, QuocTich)
                    VALUES (@MaCT, @HoTen, @ViTri, @NgaySinh, @MaDB, @SoAo, @QuocTich);";
                return await connection.ExecuteAsync(sql, cauThu);
            }
        }

        // --- HÀM CẬP NHẬT (UPDATE) ---
        // [QUAN TRỌNG] Phải chạy qua Coordinator
        public async Task<int> UpdateAsync(CauThu cauThu)
        {
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                // UPDATE trên View v_CAUTHU
                // Trigger tg_v_CAUTHU_UPD sẽ bắt lệnh này
                // Lưu ý: Không cho phép đổi MaDB (khóa phân mảnh dẫn xuất)
                const string sql = @"
                    UPDATE dbo.v_CAUTHU
                    SET HoTen = @HoTen, 
                        ViTri = @ViTri, 
                        NgaySinh = @NgaySinh, 
                        SoAo = @SoAo, 
                        QuocTich = @QuocTich
                        -- MaDB = @MaDB, (Không nên cho đổi MaDB khi Update)
                    WHERE MaCT = @MaCT;";
                return await connection.ExecuteAsync(sql, cauThu);
            }
        }

        // --- HÀM XÓA (DELETE) ---
        // [QUAN TRỌNG] Phải chạy qua Coordinator
        public async Task<int> DeleteAsync(string id)
        {
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                // DELETE trên View v_CAUTHU
                // Trigger tg_v_CAUTHU_DEL sẽ bắt lệnh này
                const string sql = "DELETE FROM dbo.v_CAUTHU WHERE MaCT = @MaCT;";
                return await connection.ExecuteAsync(sql, new { MaCT = id });
            }
        }

        // --- HÀM LẤY CHI TIẾT (GET BY ID) ---
        // [QUAN TRỌNG] Phải chạy qua Coordinator để đảm bảo tìm thấy dù nó ở mảnh nào
        public async Task<CauThu> GetByIdAsync(string id)
        {
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                // SELECT từ View v_CAUTHU
                const string sql = "SELECT * FROM dbo.v_CAUTHU WHERE MaCT = @MaCT";
                return await connection.QuerySingleOrDefaultAsync<CauThu>(sql, new { MaCT = id });
            }
        }

        // --- (Triển khai hàm tùy chọn GetByDoiBongAsync) ---
        public async Task<IEnumerable<CauThu>> GetByDoiBongAsync(string maDB, bool isGlobal = false)
        {
            // Logic tương tự GetAllAsync nhưng thêm mệnh đề WHERE MaDB = @MaDB
            if (isGlobal)
            {
                using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
                {
                    const string sqlGlobal = "SELECT * FROM dbo.v_CAUTHU WHERE MaDB = @MaDB ORDER BY HoTen";
                    return await connection.QueryAsync<CauThu>(sqlGlobal, new { MaDB = maDB });
                }
            }
            else
            {
                using (var connection = _connectionFactory.CreateConnection(ConnectionType.ReadLocalFragment))
                {
                    var sqlLocal = $"SELECT *, '{_localNode}' AS Node FROM dbo.CAUTHU WHERE MaDB = @MaDB ORDER BY HoTen";
                    return await connection.QueryAsync<CauThu>(sqlLocal, new { MaDB = maDB });
                }
            }
        }
    }
}
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;


namespace CSDLPT.Web.Infrastructure
{
    // 1. Định nghĩa Enum để quản lý các loại kết nối
    public enum ConnectionType
    {

        /// Kết nối GHI: Luôn trỏ đến Coordinator DB (CSDL_PHANTAN_Coord)
        /// Dùng cho:
        /// 1. Mọi thao tác GHI (Insert, Update, Delete)
        /// 2. Thao tác ĐỌC TOÀN CỤC (Global Read) qua View v_global_*
        WriteCoordinator,


        /// Kết nối ĐỌC CỤC BỘ: Trỏ đến DB Mảnh tại node hiện tại (ví dụ: CSDL_PHANTAN_C)
        /// Dùng cho:
        /// 1. Thao tác ĐỌC CỤC BỘ (Local Read) mặc định.

        ReadLocalFragment
    }

    // 2. Cập nhật Interface (giao diện)
    public interface IDbConnectionFactory
    {

        /// Hàm tạo kết nối chung, linh hoạt nhất.
        IDbConnection CreateConnection(ConnectionType type);


        /// (Tùy chọn) Hàm helper để GHI (luôn là Coordinator)

        IDbConnection CreateWriteConnection();

        /// (Tùy chọn) Hàm helper để ĐỌC CỤC BỘ (luôn là Local Fragment)

        IDbConnection CreateLocalReadConnection();
    }

    // 3. Cập nhật Class triển khai (Implementation)
    public sealed class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _writeConnection;
        private readonly string _localReadConnection;

        public DbConnectionFactory(IConfiguration cfg)
        {
            // Đọc 2 chuỗi kết nối đã định nghĩa trong appsettings.json
            _writeConnection = cfg.GetConnectionString("WriteCoordinator")!;
            _localReadConnection = cfg.GetConnectionString("ReadLocalFragment")!;

            // Kiểm tra lỗi cấu hình
            if (string.IsNullOrEmpty(_writeConnection))
                throw new InvalidOperationException("Connection string 'WriteConnection' not found in appsettings.json.");
            if (string.IsNullOrEmpty(_localReadConnection))
                throw new InvalidOperationException("Connection string 'ReadConnection_Local' not found in appsettings.json.");
        }

        // 4. Triển khai hàm chính
        public IDbConnection CreateConnection(ConnectionType type)
        {
            // Dùng switch expression để trả về kết nối SQL tương ứng
            return type switch
            {
                ConnectionType.WriteCoordinator => new SqlConnection(_writeConnection),
                ConnectionType.ReadLocalFragment => new SqlConnection(_localReadConnection),
                _ => throw new ArgumentOutOfRangeException(nameof(type), "Invalid connection type specified.")
            };
        }

        // 5. Triển khai các hàm helper (từ interface)
        public IDbConnection CreateWriteConnection() => CreateConnection(ConnectionType.WriteCoordinator);
        public IDbConnection CreateLocalReadConnection() => CreateConnection(ConnectionType.ReadLocalFragment);
    }


}
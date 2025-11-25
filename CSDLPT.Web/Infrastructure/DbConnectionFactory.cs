using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http; 
using System.Security.Claims;

namespace CSDLPT.Web.Infrastructure
{
    // 1. Giữ nguyên Enum cũ
    public enum ConnectionType
    {
        WriteCoordinator,   // Luôn là Site C
        ReadLocalFragment   // Tự động theo User (A hoặc B), nếu chưa login thì về C
    }

    // 2. Giữ nguyên Interface cũ
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection(ConnectionType type);
        IDbConnection CreateWriteConnection();
        IDbConnection CreateLocalReadConnection();
    }

    // 3. Class triển khai (Đã nâng cấp logic định tuyến)
    public sealed class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Inject thêm IHttpContextAccessor để biết ai đang đăng nhập
        public DbConnectionFactory(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public IDbConnection CreateConnection(ConnectionType type)
        {
            string connectionStringName = "";

            switch (type)
            {
                case ConnectionType.WriteCoordinator:
                    //  Luôn trỏ vào key "Coordinator" trong appsettings
                    connectionStringName = "Coordinator";
                    break;

                case ConnectionType.ReadLocalFragment:
                    var user = _httpContextAccessor.HttpContext?.User;
                    var sourceSite = user?.FindFirst("SourceSite")?.Value;

                    if (!string.IsNullOrEmpty(sourceSite))
                    {
                        // Nếu user là SiteA/SiteB/SiteC -> Lấy chuỗi kết nối tương ứng
                        connectionStringName = sourceSite;
                    }
                    else
                    {
                        // Nếu chưa login -> Mặc định về Coordinator để còn login được
                        connectionStringName = "Coordinator";
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Invalid connection type specified.");
            }

            // Lấy chuỗi kết nối thực tế từ appsettings.json
            string connectionString = _configuration.GetConnectionString(connectionStringName);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in appsettings.json.");
            }

            return new SqlConnection(connectionString);
        }

        // Các hàm helper giữ nguyên
        public IDbConnection CreateWriteConnection() => CreateConnection(ConnectionType.WriteCoordinator);
        public IDbConnection CreateLocalReadConnection() => CreateConnection(ConnectionType.ReadLocalFragment);
    }
}
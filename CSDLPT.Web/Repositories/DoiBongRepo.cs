using CSDLPT.Web.Models;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CSDLPT.Web.Infrastructure;

namespace CSDLPT.Web.Repositories
{
    public class DoiBongRepo : IDoiBongRepo
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly string _localNode;

        public DoiBongRepo(IDbConnectionFactory connectionFactory, IConfiguration configuration)
        {
            _connectionFactory = connectionFactory;
            _localNode = configuration.GetValue<string>("SiteConfig:Node") ?? "UNK";
        }

        
        public async Task<IEnumerable<DoiBong>> GetAllAsync(bool isGlobal = false)
        {
            if (isGlobal)
            {
                using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
                {
                    const string sqlGlobal = "SELECT * FROM dbo.v_DOIBONG ORDER BY TenDB";
                    return await connection.QueryAsync<DoiBong>(sqlGlobal);
                }
            }
            else
            {
                using (var connection = _connectionFactory.CreateConnection(ConnectionType.ReadLocalFragment))
                {
                    var sqlLocal = $"SELECT *, '{_localNode}' AS Node FROM dbo.DOIBONG ORDER BY TenDB";
                    return await connection.QueryAsync<DoiBong>(sqlLocal);
                }
            }
        }

       
        public async Task<int> CreateAsync(DoiBong doiBong)
        {
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                // Thêm MaSan và HLV vào câu lệnh
                const string sql = @"
                    INSERT INTO dbo.v_DOIBONG (MaDB, TenDB, MaSan, HLV, CLB)
                    VALUES (@MaDB, @TenDB, @MaSan, @HLV, @CLB);";
                return await connection.ExecuteAsync(sql, doiBong);
            }
        }

      
        public async Task<int> UpdateAsync(DoiBong doiBong)
        {
            using (var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                // 1. Chỉ SET các trường được phép thay đổi (TenDB, MaSan, HLV)
                // 2. KHÔNG BAO GIỜ SET khóa phân mảnh (CLB)
                const string sql = @"
                    UPDATE dbo.v_DOIBONG
                    SET TenDB = @TenDB, 
                        MaSan = @MaSan, 
                        HLV = @HLV
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

       
        public async Task<DoiBong> GetByIdAsync(string id)
        {
            using var connection = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator);
            string sql = "SELECT * FROM dbo.v_DOIBONG WHERE MaDB = @MaDB";
            return await connection.QuerySingleOrDefaultAsync<DoiBong>(sql, new { MaDB = id });
        }
        public async Task<(IEnumerable<DoiBong>, List<string>)> GetAllSafeAsync()
        {
            // Kết nối tới Coordinator (Node C)
            using (var conn = _connectionFactory.CreateConnection(ConnectionType.WriteCoordinator))
            {
                // Gọi SP vừa tạo ở Bước 1.2
                var sql = "dbo.sp_FetchGlobal_DoiBong_Safe";

                // Dùng QueryMultiple để hứng 2 bảng kết quả
                using (var multi = await conn.QueryMultipleAsync(sql, commandType: CommandType.StoredProcedure))
                {
                    // Bảng 1: Dữ liệu Đội bóng
                    var data = await multi.ReadAsync<DoiBong>();

                    // Bảng 2: Danh sách lỗi (NodeName, ErrorMsg)
                    var errorRecords = await multi.ReadAsync<dynamic>();

                    // Chuyển đổi dynamic sang List<string> cho dễ hiển thị
                    var errorMessages = new List<string>();
                    foreach (var err in errorRecords)
                    {
                        errorMessages.Add($"Cảnh báo: {err.NodeName} đang ngoại tuyến. Dữ liệu hiển thị chưa đầy đủ.");
                    }

                    return (data, errorMessages);
                }
            }
        }
    }
}
// file: Repositories/SanRepo.cs
using CSDLPT.Web.Infrastructure;
using CSDLPT.Web.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CSDLPT.Web.Repositories
{
    public class SanRepo : ISanRepo
    {
       
        private readonly IDbConnectionFactory _connectionFactory;

        public SanRepo(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

       
        public async Task<IEnumerable<SanDropdownViewModel>> GetAllForDropdownAsync()
        {
            // Danh sách này được định nghĩa tĩnh dựa trên thiết kế hệ thống 3 node
            //
            var staticSanList = new List<SanDropdownViewModel>
            {
                new SanDropdownViewModel { MaSan = "SD1", TenSan = "Sân Vận Động 1 (Thuộc Node A)" },
                new SanDropdownViewModel { MaSan = "SD2", TenSan = "Sân Vận Động 2 (Thuộc Node B)" },
                new SanDropdownViewModel { MaSan = "SD3", TenSan = "Sân Vận Động 3 (Thuộc Node C)" }
            };

            // Trả về danh sách tĩnh ngay lập tức (dùng Task.FromResult để giữ chữ ký async)
            return await Task.FromResult(staticSanList);


        }
    }
}
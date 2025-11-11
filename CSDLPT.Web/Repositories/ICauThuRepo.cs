using CSDLPT.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSDLPT.Web.Repositories
{
    public interface ICauThuRepo
    {
        // Lấy danh sách (toàn cục hoặc cục bộ)
        Task<IEnumerable<CauThu>> GetAllAsync(bool isGlobal = false);

        // (Tùy chọn) Lấy cầu thủ theo đội bóng (cho trang chi tiết đội bóng)
        Task<IEnumerable<CauThu>> GetByDoiBongAsync(string maDB, bool isGlobal = false);

        // CRUD (luôn thực thi qua Coordinator)
        Task<CauThu> GetByIdAsync(string id);
        Task<int> CreateAsync(CauThu cauThu);
        Task<int> UpdateAsync(CauThu cauThu);
        Task<int> DeleteAsync(string id);
    }
}
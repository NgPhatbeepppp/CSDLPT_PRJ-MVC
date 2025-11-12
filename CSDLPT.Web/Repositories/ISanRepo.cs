using CSDLPT.Web.Models;

namespace CSDLPT.Web.Repositories
{
    public interface ISanRepo
    {
        // Lấy danh sách Sân (MaSan, TenSan) cho dropdown
        Task<IEnumerable<SanDropdownViewModel>> GetAllForDropdownAsync();
    }
}
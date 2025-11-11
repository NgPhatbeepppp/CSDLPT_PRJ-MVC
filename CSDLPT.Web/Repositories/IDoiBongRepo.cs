using CSDLPT.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSDLPT.Web.Repositories
{
    public interface IDoiBongRepo
    {
        // === CẬP NHẬT ===
        // Thêm tham số 'isGlobal', mặc định là 'false' (ưu tiên local)
        Task<IEnumerable<DoiBong>> GetAllAsync(bool isGlobal = false);

        
        Task<DoiBong> GetByIdAsync(string id);
        Task<int> CreateAsync(DoiBong doiBong);
        Task<int> UpdateAsync(DoiBong doiBong);
        Task<int> DeleteAsync(string id);
    }
}
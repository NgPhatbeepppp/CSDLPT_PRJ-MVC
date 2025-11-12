using CSDLPT.Web.Models;

namespace CSDLPT.Web.Repositories
{
    public interface IThamGiaRepo
    {
        Task<IEnumerable<ThamGia>> GetByMaTDAsync(string maTD);
        Task<int> CreateAsync(ThamGia thamGia);
        Task<int> UpdateAsync(ThamGia thamGia);
        Task<int> DeleteAsync(string maTD, string maCT);
    }
}
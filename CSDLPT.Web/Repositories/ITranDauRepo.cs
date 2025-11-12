// file: Repositories/ITranDauRepo.cs
using CSDLPT.Web.Models;

namespace CSDLPT.Web.Repositories
{
    public interface ITranDauRepo
    {
        Task<IEnumerable<TranDau>> GetAllAsync();
        Task<TranDau?> GetByIdAsync(string maTD);
        Task CreateAsync(TranDau tranDau);
        Task UpdateAsync(TranDau tranDau);
        Task DeleteAsync(string maTD);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using CSDLPT.Web.Models;

public interface IDoiBongRepo
{
    Task<IEnumerable<DoiBong>> GetAllAsync();
    Task<DoiBong?> FindAsync(string maDb, string clb);
    Task<int> CreateAsync(DoiBong x);
    Task<int> UpdateAsync(DoiBong x);
    Task<int> DeleteAsync(string maDb);
}

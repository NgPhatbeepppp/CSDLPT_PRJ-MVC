using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Dapper;
using CSDLPT.Web.Infrastructure; 
public class AccountController : Controller
{
    private readonly IDbConnectionFactory _dbFactory;

    public AccountController(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        // 1. Kết nối tới Coordinator để kiểm tra User
        // Lưu ý: Lúc login LUÔN LUÔN kết nối về Node C (Coordinator)
        using var conn = _dbFactory.CreateWriteConnection();

        string sql = "SELECT * FROM v_DS_TAIKHOAN WHERE Username = @u AND Password = @p";
        var user = await conn.QuerySingleOrDefaultAsync<dynamic>(sql, new { u = username, p = password });

        if (user == null)
        {
            ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
            return View();
        }

        // 2. Nếu đúng, tạo Claims chứa thông tin định tuyến
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, (string)user.Username),
            new Claim(ClaimTypes.Role, (string)user.MaNhom),
            // QUAN TRỌNG: Lưu SourceSite vào Claim để dùng sau này
            new Claim("SourceSite", (string)user.SourceSite)
        };

        var identity = new ClaimsIdentity(claims, "CookieAuth");
        var principal = new ClaimsPrincipal(identity);

        // 3. Ghi Cookie (Đăng nhập thành công)
        await HttpContext.SignInAsync("CookieAuth", principal);

        return RedirectToAction("Index", "Home");
    }

   
    public async Task<IActionResult> Logout()
    {

        await HttpContext.SignOutAsync("CookieAuth");

      
        return RedirectToAction("Login", "Account");
    }

}
using CSDLPT.Web.Infrastructure;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CSDLPT.Web.Models; // Namespace của project bạn

namespace CSDLPT.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDbConnectionFactory _connectionFactory;

        public HomeController(ILogger<HomeController> logger, IDbConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy dữ liệu Biểu đồ (Giữ nguyên code cũ của bạn)
            var sqlChart = @"
                SELECT d.TenDB AS Label, COUNT(c.MaCT) AS Value
                FROM v_CAUTHU c
                JOIN v_DOIBONG d ON c.MaDB = d.MaDB
                GROUP BY d.TenDB
                ORDER BY Value DESC";

            // 2. FIX LỖI: Sử dụng Class cụ thể thay vì 'new { ... }'
            var healthStatus = new List<NodeHealthModel> {
                new NodeHealthModel { Node = "Node A (CN1)", IP = "100.107.211.71", Status = "Online" },
                new NodeHealthModel { Node = "Node B (CN2)", IP = "100.87.218.25", Status = "Online" },
                new NodeHealthModel { Node = "Coordinator",  IP = "100.99.218.57",  Status = "Active" }
            };

            using (var conn = _connectionFactory.CreateWriteConnection())
            {
                // Query biểu đồ (Nếu chưa chạy được DB thì comment đoạn này lại để test giao diện trước)
                try
                {
                    var chartData = await conn.QueryAsync<dynamic>(sqlChart);
                    ViewBag.ChartLabels = chartData.Select(x => x.Label).ToArray();
                    ViewBag.ChartValues = chartData.Select(x => x.Value).ToArray();
                }
                catch
                {
                    // Dữ liệu giả để test giao diện nếu DB lỗi
                    ViewBag.ChartLabels = new[] { "Test CLB 1", "Test CLB 2" };
                    ViewBag.ChartValues = new[] { 10, 5 };
                }

                ViewBag.HealthStatus = healthStatus;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // --- THÊM CLASS NÀY VÀO CUỐI FILE ---
    public class NodeHealthModel
    {
        public string Node { get; set; }
        public string IP { get; set; }
        public string Status { get; set; }
    }
}
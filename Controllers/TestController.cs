// 
// HTTP CONTEXT
/*
http context 
- request:  Chứa toàn bộ thông tin request - gửi lên (URL, headers, params, body, method, v.v.)
- response: Chứa toàn bộ thông tin response - kết quả trả về (status code, headers, body, v.v.)

-  "trung tâm kết nối client và server"
*/
namespace dotnet05_api_base.Controllers
{
    using System.Text;
    using System.Text.Json;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    // 
    public class TestController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(new { Message = "Hello from TestController!" });
        }
        
        [HttpGet("{id}")]
        //{id} tương ứng với [FromRoute] int id : api/test/123
        // ?name tương ứng với [FromQuery] string name : api/test/123?name=abc
        public async Task<IActionResult> Get(int id, string name, [FromHeader] string token)
        {
            // Lấy thông tin từ http context
            var routeId = HttpContext.Request.RouteValues["id"];
            var queryName = HttpContext.Request.Query["name"];

            // lấy thông tin từ header
            var headerToken = HttpContext.Request.Headers["token"].ToString();

            // lấy ra url của request : api/test/12?name=abc
            var requestUrl = HttpContext.Request.Path;
            // ip address của client
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Method (GET, POST, ...), v.v.
            var method = HttpContext.Request.Method;


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine($"[ROUTE]  Id lấy từ http context: {routeId}");
            Console.WriteLine($"[QUERY]  Name lấy từ http context: {queryName}");
            Console.WriteLine($"[HEADER] Token lấy từ http context: {headerToken}");
            Console.WriteLine($"[URL]    URL của request: {requestUrl}");
            Console.WriteLine($"[IP]     IP của client: {clientIp}");
            Console.WriteLine($"[METHOD] Method của request: {method}");
            Console.WriteLine("---------------------------------------------");
            Console.ResetColor();


            return Ok(new { routeId, queryName });
        }
        // http context lấy body

        //dọc body
        [HttpPost("demo")]
        public async Task<IActionResult> Demo()
        {
            // Cho phép đọc nhiều lần
            HttpContext.Request.EnableBuffering();

            // Đọc raw body
            using var reader = new StreamReader(
                HttpContext.Request.Body,
                Encoding.UTF8,
                leaveOpen: true);

            var bodyString = await reader.ReadToEndAsync();

            // Reset stream về đầu để tránh lỗi model binding phía sau
            HttpContext.Request.Body.Position = 0;

            Console.WriteLine("Body raw:");
            Console.WriteLine(bodyString);

            return Ok(new { data = bodyString });
        }
   
        // api lỗi 500
        [HttpGet("chia")]
        public async Task<IActionResult> Chia(int a, int b)
        {
            var result = a / b; // nếu b = 0 sẽ lỗi 500
            return Ok(new { result });
        }
    }
}

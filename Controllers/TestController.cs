// 
// HTTP CONTEXT
/*
http context 
- request:  Chứa toàn bộ thông tin request - gửi lên (URL, headers, params, body, method, v.v.)
- response: Chứa toàn bộ thông tin response - kết quả trả về (status code, headers, body, v.v.)

-  "trung tâm kết nối client và server"
*/

/*
AUTHEN: xác thực danh tính  
VD : có thẻ nhân viên sẽ qua được cổng bảo vệ
Mã lỗi :L 401


AUTHOR: phân quyền người dùng -> bạn được phép làm gì
vd: có thẻ nhân viên, nhưng chỉ được vào khu vực văn phòng, không được vào kho hàng
mãz lỗi : 403




tạo tk nhận mk -> 123456 ->  băm  => mk đã băm

đăng nhập -> 123456 -> băm -> so sánh mk đã băm với mk đã lưu trong csdl -> nếu trùng khớp thì đăng nhập thành công, ngược lại thất bại


*/
namespace dotnet05_api_base.Controllers
{
    using System.Text;
    using System.Text.Json;
    using dotnet05_api_base.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OutputCaching;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;

    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    // 
    public class TestController(CybersoftMarketplaceContext context,IMemoryCache _cache) : ControllerBase
    {
        [HttpGet]
        // [Authorize]/* chỉ cần xác minh thôi có token hợp lệ là dc*/
        // [Authorize(Roles = "ADMIN")]/* ngoài việc xác minh token hợp lệ, còn phải kiểm tra role trong token có phải là admin hay không*/
        // [Authorize(Roles = "ADMIN, USER")]/* ngoài việc xác minh token hợp lệ, còn phải kiểm tra role trong token có phải là admin/ user hay không*/


// dùng filter
[TypeFilter(typeof(AuthorFilter))]// dùng filter để kiểm tra role ADMIN mà không cần phải thêm nhiều attribute Authorize ở các controller khác nhau
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



        // hàm thực thi test action filter
        [HttpGet("action-filter")]
        [TypeFilter(typeof(LogActionFilter))]// dùng service filter để inject service vào filter
        public async Task<IActionResult> Log()
        {
            // mô phỏng công việc tốn thời gian
            Console.WriteLine("-----  Thực thi action method ----------");
            await Task.Delay(1000);

            return Ok(new { Message = "This action is decorated with LogActionFilter." });
        }

        // hàm thực thi test exception filter
        [HttpGet("exception-filter")]
        [TypeFilter(typeof(ExceptionFilter))]// dùng service filter để inject service vào filter
        public async Task<IActionResult> Exception()
        {
            // mô phỏng lỗi
            throw new Exception("Đây là lỗi được ném ra từ action method.");
        }

        // hàm thực thi test resource filter (simple cache filter)
        [HttpGet("thongke/{id}")]
        [TypeFilter(typeof(SimpleCacheFilter))]// dùng service filter để inject service vào filter
        public async Task<IActionResult> ThongKe(int id)
        {
            // mô phỏng công việc tốn thời gian (vd: truy vấn csdl, tính toán, v.v.)
            Console.WriteLine($"-----  Thực thi action method thống kê  cho id = {id} ----------");
            // giả sử lấy ra product id
            var res = await context.Products.FindAsync(id);
            return Ok(res);
        }

/*
vd api : api/product/5
UseRouting() nhận request → tìm controller khớp với `api/product/{id}`

→ `UseEndpoints()` thực thi `GetById(5)`

→ Trả về response.

*/


// THỨ TỰ
/*

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(...);



*/


// thứ tự chạy của filter
/*
middlewaree
⬇
authorization filter (AuthorFilter)
⬇
resource filter (SimpleCacheFilter) 
⬇
model binding (lấy dữ liệu từ route, query, header, body, v.v.
⬇
action filter (LogActionFilter) 
⬇
Thực thi action method (vd: GetById(5))
⬇
action filter (ActionExecuted)
⬇
result filter (ResultFilter)
⬇
resource filter (excuted) 





*/
    }
}

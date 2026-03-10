using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class SimpleCacheFilter : IResourceFilter
{
    // phương thức này sẽ được gọi trước khi tài nguyên (resource) được xử lý , chạy trước tất cả các filter khác (như action filter, authorization filter, v.v.) và trước khi action method được gọi
    static Dictionary<string, IActionResult> _cache = new Dictionary<string, IActionResult>();
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        // khi gọi api thống kê " api/thongke/1 " lần đầu tiên thì sẽ không có trong cache, nên sẽ thực thi bình thường và lưu kết quả vào cache, những lần sau sẽ kiểm tra là url đó đã có cache hay chưa
        var url = context.HttpContext.Request.Path.ToString();
        if (_cache.ContainsKey(url))
        {
            context.Result = _cache[url]; // trả về kết quả đã được lưu trong cache mà không cần phải thực thi action method
            Console.WriteLine($"[SimpleCacheFilter] - Trả về kết quả từ cache cho url: {url}");
        }
    }
    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        // vào method thực thi action 
        // nhân kết quả từ action method và lưu vào cache để những lần sau 
        var url = context.HttpContext.Request.Path.ToString();
        if (_cache.ContainsKey(url))
        {
            // đã có cache rồi thì đè giá trị mới
            _cache[url] = context.Result; // cập nhật kết quả mới vào cache
            Console.WriteLine($"[SimpleCacheFilter] - Cập nhật cache cho url: {url}");
        }
        else
        {
            // chưa có cache thì lưu vào cache
            _cache[url] = context.Result; // lưu kết quả của action method vào cache với key là url của request
            Console.WriteLine($"[SimpleCacheFilter] - Lưu kết quả vào cache cho url: {url}");
        }
    }


}
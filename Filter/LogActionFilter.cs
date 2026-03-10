using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

public class LogActionFilter : IActionFilter
{
    // phương thức này sẽ được gọi trước khi action method được thực thi 
    Stopwatch _timer;
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // bắt đầu đồng hồ để đo thời gian thực thi của action method
        _timer = Stopwatch.StartNew();
        Console.WriteLine($"[LogActionFilter] - Action {context.ActionDescriptor.DisplayName} is executing...");
    }
    // phương thức này sẽ được gọi sau khi action method được thực thi
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // dừng đồng hồ và ghi log thời gian thực thi của action method
        _timer.Stop();
        Console.WriteLine($"[LogActionFilter] - Action {context.ActionDescriptor.DisplayName} executed in {_timer.ElapsedMilliseconds} ms."); 
        // xử lý lưu record vào db  hoặc gửi log đến hệ thống log tập trung (vd: ELK, Seq, v.v.) ... 
    }
}
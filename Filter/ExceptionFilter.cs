using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        
        Console.WriteLine($"❌ - Lỗi: {context.Exception.Message}");
        context.Result = new JsonResult(new { Message = "Đã xảy ra lỗi trong quá trình xử lý yêu cầu." })
        {
            StatusCode = 500 // Internal Server Error
        };
    }
}
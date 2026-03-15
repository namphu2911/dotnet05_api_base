public class BlockIpMiddleware : IMiddleware
{
    static List<string> blockedIps = new List<string>(); // Danh sách các IP bị chặn
    static Dictionary<string, int> requestCounts = new Dictionary<string, int>(); // Đếm số lần request của mỗi IP
    public BlockIpMiddleware()
    {
        // Constructor
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();

        if (blockedIps.Contains(clientIp))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return context.Response.WriteAsync("Your IP has been blocked due to too many requests.");
        }
        // chưa bị block thì 
        // key Ip, value là số lần request
        // cần ip , số lần requess và thời gian cuối cùng request để reset lại số lần request sau 1 khoảng thời gian nhất định
        if(requestCounts.ContainsKey(clientIp))
        {
            requestCounts[clientIp]++;
        }
        else
        {
            requestCounts[clientIp] = 1;
        }
        // nếu số lần request vượt quá 5 lần thì block ip đó
        if(requestCounts[clientIp] > 5)
        {
            blockedIps.Add(clientIp);
            requestCounts.Remove(clientIp); // Xóa khỏi requestCounts để tiết kiệm bộ nhớ
        }

        return next(context);
    }
}
using Microsoft.AspNetCore.Mvc.Filters;

public class AuthorFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
       // nêu như có role ADMIN  cho truy cập không thì trả 403
       var user   = context.HttpContext.User;
       if(user == null || !user.Identity.IsAuthenticated || !user.IsInRole("ADMIN"))
       {
            context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();// trả 403
       }
       
    }
}
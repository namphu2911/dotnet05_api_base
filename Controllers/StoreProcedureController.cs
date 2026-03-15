using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet05_api_base.DTO;
using dotnet05_api_base.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
//using dotnet05_api_base.Models;

namespace dotnet05_api_base.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreProcedureController : ControllerBase
    {
        private readonly CybersoftMarketplaceContext _context ;
        public StoreProcedureController(CybersoftMarketplaceContext context)
        {
            _context = context;
        }

        [HttpGet("getProductByCategory/{categoryId}")]
        public async Task<ActionResult> getProductByCategory([FromRoute] int categoryId)
        {
            
            try
            {
                await _context.Database.BeginTransactionAsync();
                SqlParameter paramCategoryId = new SqlParameter("@CategoryID", System.Data.SqlDbType.Int) { Value = categoryId };
                string sqlRaw = @$"EXEC GetProductByCategoryId @CategoryID";

                var res = await _context.Database.SqlQueryRaw<ProductByProcedureDTO>(sqlRaw, paramCategoryId).ToListAsync();
                await _context.Database.CommitTransactionAsync();
                return StatusCode(200, new { message = "Thành công", data = res });


            }catch (Exception ex)
            {
                await _context.Database.RollbackTransactionAsync();
                return StatusCode(500, new { message = "Lỗi khi thực hiện stored procedure", error = ex.Message });
            }
     
        }

    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using dotnet05_api_base.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
//using dotnet05_api_base.Models;

namespace dotnet05_api_base.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoStoreProcedureController : ControllerBase
    {
        private readonly CybersoftMarketplaceContext _context;
        public DemoStoreProcedureController(CybersoftMarketplaceContext context)
        {
            _context = context;
        }
        [HttpGet("layDanhSachSanPhamTheoDanhMuc/{categoryId}")]
        public async Task<IActionResult> LayDanhSachSanPhamTheoDanhMuc(int categoryId)
        {
            try
            {
                var res = _context.Database.SqlQueryRaw<ProductDTOStore>(@$"EXEC GetProductByCategory @CategoryID = ${categoryId}").ToList();
                return StatusCode(200, new { message = "Thành công", data = res });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thực hiện stored procedure", error = ex.Message });
            }
        }
        [HttpPost("AddUserDynamicStoreProcedure")]
        public async Task<IActionResult> AddUserDynamicStoreProcedure([FromBody] UserCreateStoreDTO userCreateStoreDTO)
        {

            string res = InsertStoreProcedure<UserCreateStoreDTO>(userCreateStoreDTO);

            return Ok(res);
        }


        private string InsertStoreProcedure<T>(T entity)
        {
            string tableName = typeof(T).GetCustomAttribute<TableAttribute>().Name;
            //Lấy danh sách thuộc tính của class
            string columnNames = "";
            var properties = typeof(T).GetProperties();

            foreach (PropertyInfo prop in properties)
            {
                if (columnNames != "")
                {
                    columnNames += ", ";
                }
                columnNames += prop.Name;
            }

            //List giá trị tương ứng
            List<string> columnValues = new List<string>();

            foreach (PropertyInfo prop in properties)
            {

                var value = prop.GetValue(entity);
                if (value is string || value is DateTime)
                {
                    columnValues.Add($"{value}"); // Thêm N để hỗ trợ Unicode
                }
                else
                {
                    columnValues.Add($"{value}");
                }
            }

            //Gọi stored procedure insert
            // EXEC InsertDynamicData_JSON @TableName, @Columns, @ValuesJSON;
            SqlParameter tableNameParam = new SqlParameter("@TableName", System.Data.SqlDbType.NVarChar, 128)
            {
                Value = tableName
            };

            SqlParameter columnsParam = new SqlParameter("@Columns", System.Data.SqlDbType.NVarChar, 200)
            {
                Value = columnNames
            };

            SqlParameter columnsValuesParam = new SqlParameter("@ValuesJSON", System.Data.SqlDbType.NVarChar, 200)
            {
                Value = JsonSerializer.Serialize(columnValues)
            };

            var sql = $"EXEC InsertDynamicData_JSON @TableName, @Columns, @ValuesJSON";
            int res = _context.Database.ExecuteSqlRaw(sql, tableNameParam, columnsParam, columnsValuesParam);
            Console.WriteLine($@"{tableName} - {columnNames} - {columnValues}");
            string kq = $@"{tableName} - {columnNames} - {JsonSerializer.Serialize(columnValues)}";
            return kq;
        }



        //Kết nối view 
        [HttpGet("getProductById/{idProduct}")]
        public IActionResult getProductById([FromRoute] int idProduct)
        {
            var res = _context.VGetAllProductsDetails.Where(item => item.ProductId == idProduct).ToList();
            return Ok(new { message = "Thành công", data = res });
        }


        [HttpGet("getAllUserFromFunctionSQL")]
        public IActionResult getAllUserFromFunctionSQL()
        {
            //sql gọi function: select * from dbo.[function_name]()
            string sqlRaw = "SELECT * FROM dbo.GetAllUserWithEmailFullNamePhone()";
            List<UserFunctionDTO> res = _context.Database.SqlQueryRaw<UserFunctionDTO>(sqlRaw).ToList();
            return Ok(new { message = "Thành công", data = res });
        }
    }
    class UserFunctionDTO
    {
        public string email { get; set; }
        public string fullName { get; set; }
        public string phone { get; set; }
    }

}



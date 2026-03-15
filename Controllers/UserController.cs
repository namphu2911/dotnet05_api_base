using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet05_api_base.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
//using dotnet05_api_base.Models;

namespace dotnet05_api_base.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly CybersoftMarketplaceContext _context;
        public UserController(CybersoftMarketplaceContext context)
        {
            _context = context;
        }


        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _context.Users.Where(item => item.Deleted != true).OrderBy(item => item.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(item => new
            {
                id = item.Id,
                fullName = item.FullName,
                email = item.Email,
                phoneNumber = item.Phone,
                address = item.Address,
                avatar = item.Avatar
            }).ToListAsync();
            return Ok(users);
        }
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDTO userDTO)
        {

            User newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = userDTO.Username,
                FullName = userDTO.FullName,
                Email = userDTO.Email,
                Phone = userDTO.Phone,
                Avatar = userDTO.Avatar,
                PasswordHash = "123",
                // Address = userDTO.Address,
                CreatedAt = DateTime.UtcNow,
                Deleted = false
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created successfully", userId = newUser.Id });
        }
        [HttpDelete("DeleteUserRaw/{id}")]
        public async Task<IActionResult> DeleteUserRaw([FromRoute] Guid id)
        {
            //Check id có tồn tại trong csdl
            var user = await _context.Users.SingleOrDefaultAsync(n => n.Id == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            try
            {

                await _context.Database.BeginTransactionAsync();

                //Sql parameter để tránh lỗi sql injection
                var sqlParameter = new SqlParameter("@id", System.Data.SqlDbType.UniqueIdentifier) { Value = id };
                //Sử dụng câu lệnh sql raw để xóa
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users WHERE Id = @id", sqlParameter);

                await _context.Database.CommitTransactionAsync();

            }
            catch (Exception ex)
            {
                await _context.Database.RollbackTransactionAsync();
                return StatusCode(500, new { message = "An error occurred while deleting the user.", error = ex.Message });
            }

            return Ok(new { message = "User deleted successfully" });
        }
        [HttpDelete("DeleteUserLinQ/{id}")]
        public async Task<IActionResult> DeleteUserLinQ([FromRoute] Guid id)
        {
            var user = await _context.Users.SingleOrDefaultAsync(n => n.Id == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync(); //Lệnh tương tự Commit

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user.", error = ex.Message });
            }

            return Ok(new { message = "User deleted successfully" });
        }

        [HttpGet("DeleteUpdate/{id}")]
        // [Authorize(Roles ="Admin")]
        public async Task<IActionResult> DeleteUpdate([FromRoute] Guid id)
        {

            var user = await _context.Users.SingleOrDefaultAsync(n => n.Id == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            try
            {
                user.Deleted = true;
                await _context.SaveChangesAsync();
                return Ok(new { message = "User deleted successfully" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user.", error = ex.Message });
            }
        }

        [HttpPut("UpdateUserInfo/{id}")]
        public async Task<IActionResult> UpdateUserInfo([FromRoute] Guid id, [FromBody] UserUpdateInfoDTO updateDTO)
        {
            //Kiểm tra coi user có tồn tại trong CSDL không
            User user = await _context.Users.SingleOrDefaultAsync(n => n.Id == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            try
            {
                user.Phone = updateDTO.Phone;
                user.Address = updateDTO.Address;
                await _context.SaveChangesAsync();
                return Ok(new { message = "User information updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user information.", error = ex.Message });
            }
        }

        [HttpPut("SQLUpdateUserInfo/{id}")]
        public async Task<IActionResult> SQLUpdateUserInfo([FromRoute] Guid id, [FromBody] UserUpdateInfoDTO updateDTO)
        {
            //Kiểm tra coi user có tồn tại trong CSDL không
            var user = await _context.Users.SingleOrDefaultAsync(n => n.Id == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            try
            {
                var sqlParameters = new[]
                {
                    new SqlParameter("@id", System.Data.SqlDbType.UniqueIdentifier) { Value = id },
                    new SqlParameter("@phone", System.Data.SqlDbType.NVarChar, 15) { Value = updateDTO.Phone },
                    new SqlParameter("@address", System.Data.SqlDbType.NVarChar, 50) { Value = updateDTO.Address }
                };

                await _context.Database.ExecuteSqlRawAsync("UPDATE Users SET Phone = @phone, Address = @address WHERE Id = @id", sqlParameters);

                return Ok(new { message = "User information updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user information.", error = ex.Message });
            }
        }
 
    }
}


//api-controller-async
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet05_api_base.Models;
using Microsoft.AspNetCore.Mvc;
//using dotnet05_api_base.Models;

namespace dotnet05_api_base.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        public static List<SanPham> dsSanPham = new List<SanPham>()
        {
            new SanPham(){Id=1,TenSanPham="Iphone 14 Pro Max",GiaBan=2000,MoTa="Điện thoại Iphone 14 Pro Max 128",HinhAnh="https://dummyimage.com/600x400/000/fff?text=Iphone14ProMax"},
            new SanPham(){Id=2,TenSanPham="Samsung S23 Ultra",GiaBan=1800,MoTa="Điện thoại Samsung S23 Ultra 128",HinhAnh="https://dummyimage.com/600x400/000/fff?text=SamsungS23Ultra"}
        };
        
        public SanPhamController()
        {
        }
        [HttpGet("laydanhsachsanpham")]
        public async Task<IActionResult> LayDanhSachSanPham()
        {
            return Ok(dsSanPham);
        }

        [HttpGet("customlaydanhsachsanpham")]
        public async Task<IActionResult> CustomLayDanhSachSanPham()
        {
            return StatusCode(200,new {message="Thành công",data=dsSanPham});
        }
        //resful api: chuẩn api
        //resapi: không chuẩn api

        [HttpPost("themsanpham")]
        public async Task<IActionResult> ThemSanPham([FromBody] SanPham sanPham)
        {
            dsSanPham.Add(sanPham);
            return Created(nameof(LayDanhSachSanPham), sanPham);
        }

        //Giã sử accessToken phải là cybersoft@111
        [HttpGet("laythongtindangnhap")]
        public async Task<IActionResult> layThongTinDangNhap([FromHeader] string accessToken)
        {   
            if(accessToken != "cybersoft@111")
            {
                return Unauthorized(new {message="Không có quyền truy cập !"});
            }
            var ttdn = new
            {
                HoTen = "Nguyễn Văn A",
                Email = "nguyenvana@gmail.com",
            };
            return Ok(ttdn);    
        }

        [HttpGet("layThongTinSanPham/{id}")]
        public IActionResult layThongTinSanPham([FromRoute] int id)
        {
            var sp = dsSanPham.SingleOrDefault(item => item.Id == id);
            if(sp == null)
            {
                return BadRequest("Mã sản phẩm không tồn tại !");
            }
            return Ok (sp);
        }


        [HttpGet("timKiemSanPham")]
        public IActionResult timKiemSanPham([FromQuery] string ten="",[FromQuery] double gia=double.MaxValue)
        {
            var kq = dsSanPham.Where(item => item.TenSanPham.Contains(ten) && item.GiaBan <= gia).ToList();
            if(kq.Count == 0)
            {
                return NotFound("Không tìm thấy sản phẩm phù hợp !");
            }
            return Ok(kq);
        }
        

        [HttpPut("capNhatSanPham/{id}")]
        public async Task<IActionResult> capNhatSanPham([FromRoute] int id, [FromBody] SanPham sanPhamEdit)
        {
            var spUpdate = dsSanPham.SingleOrDefault(item =>item .Id == id);
            if (spUpdate != null) //Tìm thấy sp
            {
                spUpdate.TenSanPham = sanPhamEdit.TenSanPham;
                spUpdate.GiaBan = sanPhamEdit.GiaBan;
                spUpdate.MoTa = sanPhamEdit.MoTa;
                spUpdate.HinhAnh = sanPhamEdit.HinhAnh;
                return StatusCode(200,new {message="Cập nhật thành công", data=dsSanPham});
            }
            return BadRequest("Mã sản phẩm không tồn tại !");
        }

        [HttpDelete("xoaSanPham/{id}")] 
        public async Task<IActionResult> xoaSanPham([FromRoute] int id)
        {
            var spDelete = dsSanPham.SingleOrDefault(item => item.Id == id);
            if (spDelete != null) //Tìm thấy sp
            {
                dsSanPham.Remove(spDelete);
                return StatusCode(200,new {message="Xoá thành công", data=dsSanPham});
            }
            return BadRequest("Mã sản phẩm không tồn tại !");
        }


       
    }
}
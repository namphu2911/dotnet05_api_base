namespace dotnet05_api_base.Controllers
{
    using System.Text.Json;
    using AutoMapper;
    using dotnet05_api_base.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Distributed;

    [Route("api/[controller]")]
    [ApiController]
    public class RedisController(CybersoftMarketplaceContext _context, IDistributedCache _cache, RedisHelper _redis, IMapper _mapper) : ControllerBase
    {
        [HttpGet("products")]
        public async Task<IActionResult> Get()
        {
            var cacheKey = "product_list_100";
            // kiểm tra cache
            var cacheData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cacheData)) // nếu có cache - HIT  -> return
            {
                var dspro = JsonSerializer.Deserialize<List<Product>>(cacheData);
                return Ok(dspro);
            }
            var res = await _context.Products.OrderByDescending(x => x.Id).Take(500).ToListAsync();
            // lưu vào cache
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(res), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // cache trong 5 phút
            });

            return Ok(res);
        }
        // api thêm mới product -> xóa cache product_list_100 đi để lần sau truy cập sẽ lấy dữ liệu mới nhất
        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDTO proDTO)
        {
            // Product model = new Product();
            // model.Name = proDTO.Name;
            // model.Description = proDTO.Description;
            // model.Image = proDTO.Image;
            // model.DisplayPrice = proDTO.DisplayPrice;
            // model.ShopId = proDTO.ShopId;
            // model.CategoryId = proDTO.CategoryId;
            // model.Alias = proDTO.Alias;
            // dùng AutoMapper để map dữ liệu từ DTO sang Entity
            var model  = _mapper.Map<Product>(proDTO);

            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            // sau khi thêm product mới thành công, xóa cache của danh sách product để lần sau truy cập sẽ lấy dữ liệu mới nhất
            await _cache.RemoveAsync("product_list_100");

            return Ok("đã thêm product mới thành công");
        }


        [HttpGet("products_v2")]
        public async Task<IActionResult> GetV2()
        {
            var cacheKey = "product_list_100_v2";
            // kiểm tra cache
            var cacheData = await _redis.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cacheData)) // nếu có cache - HIT  -> return
            {
                var dspro = JsonSerializer.Deserialize<List<Product>>(cacheData);
                return Ok(dspro);
            }
            var res = await _context.Products.OrderByDescending(x => x.Id).Take(500).ToListAsync();
            // lưu vào cache
            await _redis.SetStringAsync(cacheKey, JsonSerializer.Serialize(res), TimeSpan.FromMinutes(5));

            return Ok(res);
        }

    }
}
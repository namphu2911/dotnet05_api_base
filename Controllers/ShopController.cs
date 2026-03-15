namespace dotnet05_api_base.Controllers
{
    using dotnet05_api_base.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;

    [Route("api/[controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        CybersoftMarketplaceContext _context;
        IMemoryCache _cache;
        string cacheKey = "shop_list_50";

        public ShopController(CybersoftMarketplaceContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {


            // cache dự liệu
            // Lấy danh sách shops từ database
            // lấy ra 50 shop mới nhất (sắp xếp theo id giảm dần) và trả về cho client
            if (_cache.TryGetValue(cacheKey, out var cachedResult))
            {
                // có cache rồi : cache hit -> return
                return Ok(cachedResult);
            }
            // nếu không có thì truy vấn database để lấy dữ liệu và lưu vào cache
            // cache miss -> truy vấn database
            var res = await _context.Shops.OrderByDescending(x => x.Id).Take(50).ToListAsync();

            _cache.Set(cacheKey, res, TimeSpan.FromMinutes(5)); // Cache trong 5 phút

            return Ok(res);
        }

        // thêm shop mới 
        [HttpPost]
        public async Task<IActionResult> CreateShop([FromBody] ShopCreateDTO shopDTO)
        {
            Shop model = new Shop();
            model.ShopName = shopDTO.ShopName;
            model.Description = shopDTO.Description;
            model.Image = shopDTO.Image;
            model.Alias = shopDTO.Alias;
            model.OwnerId = new Guid("AF887A87-60C7-4792-8A38-02FD95B5BA20");

            _context.Shops.Add(model);
            await _context.SaveChangesAsync();

            // sau khi thêm shop mới thành công, xóa cache của danh sách shop để lần sau truy cập sẽ lấy dữ liệu mới nhất
            _cache.Remove(cacheKey);
            return Ok("đã thêm shop mới thành công");
        }

    }
}

/*
Cache = bộ nhớ tạm để lưu dữ liệu truy cập thường xuyên
HIT - MISS

 
CLIENT -> API -> CONTROLLER -> DB LẤY DỮU LIỆU TRẢ VỀ CHO CLIENT
                    |
                    CÓ CACHE THÌ -> CACHE  TRẢ VỀ CHO CLIENT

- redis - redis insight vào chung 1 network  : an toàn , bảo mật hơn 


*/
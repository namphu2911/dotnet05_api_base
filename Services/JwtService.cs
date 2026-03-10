using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dotnet05_api_base.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public interface IJwtAuthService
{
    Task<string> GenerateToken(User userLogin);
    string DecodePayloadToken(string token);
}
public class JwtAuthService : IJwtAuthService
{
    private readonly string? _key;
    private readonly string? _issuer;
    private readonly string? _audience;
    private readonly CybersoftMarketplaceContext _context;
    public JwtAuthService(IConfiguration Configuration, CybersoftMarketplaceContext db)
    {
        _key = Configuration["Jwt:Serect-Key"];
        _issuer = Configuration["Jwt:Issuer"];
        _audience = Configuration["Jwt:Audience"];
        _context = db;
    }
    
    public async Task<string> GenerateToken(User userLogin)
    {
        // Khóa bí mật để ký token
        var key = Encoding.ASCII.GetBytes(_key);
        // Tạo danh sách các claims cho token
        var claims = new List<Claim>
        {
            new Claim("UserName", userLogin.Username),               // Claim mặc định cho username
            // new Claim(ClaimTypes.Role, userLogin.Role),                   // Claim mặc định cho Role
            new Claim(JwtRegisteredClaimNames.Sub, userLogin.Username),   // Subject của token
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique ID của token
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()) // Thời gian tạo token
        };
        // Tạo khóa bí mật để ký token
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature
        );


        // lấy ra user role
        var userRole = await  _context.UserRoles.Where(ur=> ur.UserId==userLogin.Id).Include(ur => ur.Role).FirstOrDefaultAsync();
        if (userRole != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Id.ToString()));
        }
        // Thiết lập thông tin cho token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(2), // Token hết hạn sau 1 giờ
            SigningCredentials = credentials,
            Issuer = _issuer,                 // Thêm Issuer vào token
            Audience = _audience,              // Thêm Audience vào token
        };
        
        // Tạo token bằng JwtSecurityTokenHandler
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        // Trả về chuỗi token đã mã hóa
        return tokenHandler.WriteToken(token);
    }

    public string DecodePayloadToken(string token)
    {
        try
        {
            // Kiểm tra token có null hoặc rỗng không
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token không được để trống", nameof(token));
            }

            // Tạo handler và đọc token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Lấy username từ claims (thường nằm trong claim "sub" hoặc "name")
            var usernameClaim = jwtToken.Claims.FirstOrDefault(x =>x.Type == "UserName"); // Common in some identity providers

            if (usernameClaim == null)
            {
                throw new InvalidOperationException("Không tìm thấy username trong payload");
            }

            return usernameClaim.Value;
        }
        catch (Exception ex)
        {
            // Xử lý lỗi (có thể log lỗi ở đây)
            throw new InvalidOperationException($"Lỗi khi decode token: {ex.Message}", ex);
        }
    }

}
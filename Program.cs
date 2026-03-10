using System.Security.Claims;
using System.Text;
using System.Text.Json;
using dotnet05_api_base.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//DI Services swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // 🔥 Thêm hỗ trợ Authorization header tất cả api
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token vào ô bên dưới theo định dạng: Bearer {token}"
    });

    // 🔥 Định nghĩa yêu cầu sử dụng Authorization trên từng api
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
//DI Service EF-context
//connection string : Mỗi hệ quản trị csdl khác nhau sẽ có connectionstring khác nhau

builder.Services.AddDbContext<QuanLySanPhamContext>();



//DI Service entity framework core CybersoftMarketplaceContext
builder.Services.AddDbContext<CybersoftMarketplaceContext>();

// DI service JwtAuthService
builder.Services.AddScoped<IJwtAuthService, JwtAuthService>();


//DI service controller 
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// DI service CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() // Cho phép tất cả domain gửi request đến server
              .AllowAnyMethod() // Cho phép tất cả method (GET, POST, PUT, DELETE, ...)
              .AllowAnyHeader(); // Cho phép tất cả header
    });
});

// DI thêm 1 service cors khác  AllowFe
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFe", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://127.0.0.1:5501", "https://127.0.0.1:5500") // Chỉ cho phép domain frontend gửi request đến server
              .AllowAnyMethod() // Cho phép tất cả method (GET, POST, PUT, DELETE, ...)
              .AllowAnyHeader(); // Cho phép tất cả header
    });
});


// AUTHEN, AUTHO : Xác thực và phân quyền người dùng 
//Thêm middleware authentication
var privateKey = builder.Configuration["jwt:Serect-Key"];
var Issuer = builder.Configuration["jwt:Issuer"];
var Audience = builder.Configuration["jwt:Audience"];
// Thêm dịch vụ Authentication vào ứng dụng, sử dụng JWT Bearer làm phương thức xác thực
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    // Thiết lập các tham số xác thực token
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        // Kiểm tra và xác nhận Issuer (nguồn phát hành token)
        ValidateIssuer = true,
        ValidIssuer = Issuer, // Biến `Issuer` chứa giá trị của Issuer hợp lệ
                              // Kiểm tra và xác nhận Audience (đối tượng nhận token)
        ValidateAudience = true,
        ValidAudience = Audience, // Biến `Audience` chứa giá trị của Audience hợp lệ
                                  // Kiểm tra và xác nhận khóa bí mật được sử dụng để ký token
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey)),
        // Sử dụng khóa bí mật (`privateKey`) để tạo SymmetricSecurityKey nhằm xác thực chữ ký của token
        // Giảm độ trễ (skew time) của token xuống 0, đảm bảo token hết hạn chính xác
        ClockSkew = TimeSpan.Zero,
        // Xác định claim chứa vai trò của user (để phân quyền)
        RoleClaimType = ClaimTypes.Role,
        // Xác định claim chứa tên của user
        NameClaimType = ClaimTypes.Name,
        // Kiểm tra thời gian hết hạn của token, không cho phép sử dụng token hết hạn
        ValidateLifetime = true
    };
});
// Thêm dịch vụ Authorization để hỗ trợ phân quyền người dùng
builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware : Xử lý request và response
// Exception Handling Middleware - Xử lý lỗi toàn cục
// app.UseExceptionHandler("/error"); // middleware có sẵn
// kiểm soát việc trả lỗi về client theo format chuẩn
// exception : tình huống bất thường xảy ra trong quá trình xử lý request (vd: lỗi kết nối csdl, lỗi chia cho 0, v.v.)

app.UseExceptionHandler(er =>
{
    // Middleware xử lý lỗi tùy chỉnh
    er.Run(async context =>
    {
        // can thiệp vào response trả về lỗi
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        // trả về lỗi theo format chuẩn

        var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = errorFeature?.Error;


        var errorResponse = new
        {
            Message = "Đã có lỗi xảy ra. Vui lòng thử lại sau.",
            Details = exception?.Message
        };
        var jsonRes = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(jsonRes);
    });
});

//CORS MIDDLEWARE ---> (THIẾT LẬP CHÍNH SÁCH CORS)
// CORS (Cross-Origin Resource Sharing) : Cơ chế bảo mật của trình duyệt, ngăn chặn các request từ domain khác (origin khác) trừ khi server cho phép

// Origin
/*
http://127.0.0.1:5500  #  https://127.0.0.1:5500

- http   : protocol
- 127.0.0.1 : host
- 5500 : port


*/

app.UseCors("AllowFe"); // Cho phép tất cả các domain gửi request đến server (không nên dùng trong production)



// static file middleware : cho phép truy cập vào thư mục wwwroot để lấy file tĩnh (html, css, js, hình ảnh, ...)
//app.UseStaticFiles();  // mặc định sẽ trỏ đến wwwroot, nếu muốn đổi tên thư mục thì phải cấu hình thêm
// khi trình duyệt gửi request đến server để lấy file ví dụ là http://localhost:5244/black-car.jpg , thì middleware này sẽ tìm trong thư mục wwwroot có file black-car.jpg hay không, nếu có thì trả về cho trình duyệt, nếu không có thì trả về lỗi 404

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles") // đổi thành folder StaticFiles
                                                                     // khi truy cập vào url có chứa /static thì sẽ trỏ đến folder StaticFiles
    ),
    RequestPath = "/files", // khi truy cập vào url có chứa /files thì sẽ trỏ đến folder StaticFiles
    OnPrepareResponse = ctx =>
    {
        // can thiệp loại file được trả về, vd chỉ cho phép trả về file hình ảnh
        var path = ctx.File.PhysicalPath;
        if (path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".jpeg") || path.EndsWith(".pdf"))
        {
            // cho phép trả về file hình ảnh
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600"); // cache file trong 10 phút
        }
        else
        {
            // không cho phép trả về file khác
            ctx.Context.Response.StatusCode = 403; // forbidden
            ctx.Context.Response.ContentLength = 0;
            ctx.Context.Response.Body = Stream.Null; // không trả về nội dung
        }
    }
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();

app.UseHttpsRedirection();
//Sử dụng middleware map controller
app.MapControllers();



app.Run();

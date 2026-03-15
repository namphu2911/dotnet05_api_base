// Cách 1 tạo 1 file dùng chung cho tất cả các controller để map dữ liệu từ entity sang DTO và ngược lại, tránh việc phải viết code map thủ công nhiều lần trong controller
using AutoMapper;
using dotnet05_api_base.Models;

public class EntityMapper : Profile
{
    public EntityMapper()
    {
        //  PRODUCT
        // CreateMap<Product, ProductCreateDTO>(); // map từ Product sang ProductCreateDTO
        // CreateMap<ProductCreateDTO, Product>(); // map từ ProductCreateDTO sang Product
        CreateMap<Product, ProductCreateDTO>().ReverseMap(); // map 2 chiều giữa Product và ProductCreateDTO
        // map khác tên field
        CreateMap<Product, ProductDTO>()
            .ForMember(x => x.ProductName, opt => opt.MapFrom(x => x.Name))
            .ForMember(x => x.Image, otp=> otp.MapFrom(x => x.Image ?? "https://via.placeholder.com/150")) // nếu field Image trong Product có giá trị null thì trả về url ảnh mặc định, còn nếu có giá trị thì trả về giá trị đó
            .ForMember(x => x.Deleted, otp => otp.Ignore()); // bỏ qua field Id khi map từ Product sang ProductDTO vì ProductDTO không có field Id, nếu không bỏ qua thì khi map sẽ bị lỗi vì không tìm thấy field Id trong ProductDTO
            // x => x.ProductName : tên field trong ProductDTO
            // opt.MapFrom(x => x.Name) : lấy dữ liệu từ field Name trong Product

        // SHOP
        CreateMap<Shop, ShopCreateDTO>().ReverseMap(); // map 2 chiều giữa Shop và ShopCreateDTO



        // 
        //User
            CreateMap<User, UserCreateDTO>().ReverseMap(); // map 2 chiều giữa User và UserCreateDTO
    }
}
// ProductMapper
// ShopMapper
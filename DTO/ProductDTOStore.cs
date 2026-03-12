// Id,ShopId,CategoryId,Name,Description,Image,DisplayPrice
public class ProductDTOStore
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Image { get; set; } = "";
    public decimal DisplayPrice { get; set; }
    public ProductDTOStore()    {
    }
}
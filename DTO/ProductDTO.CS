public class ProductDTO
{
    public int Id { get; set; }

    public int ShopId { get; set; }

    public int CategoryId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Alias { get; set; }

    public string? AdditionalData { get; set; }

    public bool? Deleted { get; set; }

    public string? Image { get; set; }

    public decimal DisplayPrice { get; set; }

}
public class ProductCreateDTO
{
    public int Id { get; set; }

    public int ShopId { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Alias { get; set; }

    public string? AdditionalData { get; set; }

    public string? Image { get; set; }

    public decimal DisplayPrice { get; set; }
}
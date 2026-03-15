namespace dotnet05_api_base.DTO
{
    public class ProductByProcedureDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal DisplayPrice { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
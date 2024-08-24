namespace StockInformationSystem.Core
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
    }
}

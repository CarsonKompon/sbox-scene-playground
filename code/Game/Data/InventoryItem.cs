namespace Home.Data
{
    public class InventoryItem
    {
        // Database values
        public string? Id { get; set; }
        public int Quantity { get; set; }

        // Runtime values
        public string Name => Id ?? "Unknown";
        public int AvailableQuantity => Quantity;
    }
}

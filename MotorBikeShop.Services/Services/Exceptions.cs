namespace MotorBikeShop.Services
{

    [Serializable]
    public class ShopException : Exception
    {
        public ShopException() { }
        public ShopException(string message) : base(message) { }
        public ShopException(string message, Exception inner) : base(message, inner) { }

    }

    public class OutOfStockException : ShopException
    {
        public OutOfStockException() 
            : base("Out of stock")
        {
            
        }

        public OutOfStockException(string message)
            : base(message)
        {

        }
    }

    public class InventoryNotFoundException : ShopException
    {
        public InventoryNotFoundException() 
            : base("Inventory not found")
        {
            
        }
    }
}

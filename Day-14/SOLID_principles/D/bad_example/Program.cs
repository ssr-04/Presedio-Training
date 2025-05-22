// Low-level module (implementation)
public class VisaGateway
{
    public void MakePayment(decimal amount)
    {
        Console.WriteLine($"PayPal: Processing payment of {amount}...");
        // Simulated
    }
}

// High-level module (core business logic)
public class OrderProcessor
{
    private VisaGateway _visaGateway; // Direct dependent on low-level concrete class

    public OrderProcessor()
    {
        _visaGateway = new VisaGateway(); // High-level module creates its own low-level dependency
    }

    public void Checkout(decimal totalAmount)
    {
        Console.WriteLine("OrderProcessor: Starting checkout process...");
        _visaGateway.MakePayment(totalAmount); // High-level calls low-level
        Console.WriteLine("OrderProcessor: Checkout complete.");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        OrderProcessor processor = new OrderProcessor();
        processor.Checkout(9900.99m);

        // Problems:
        // 1. If we want to use Mastercard, we have to change OrderProcessor.
        // 2. Hard to test OrderProcessor without hitting Visa logic.
    }
}
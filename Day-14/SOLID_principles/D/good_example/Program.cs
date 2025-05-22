// 1. Abstraction (Interface)
public interface IPaymentGateway
{
    void MakePayment(decimal amount);
}

// 2. Low-level (Concrete Details) depend on Abstraction
public class VisaGateway : IPaymentGateway // Concrete implementation
{
    public void MakePayment(decimal amount)
    {
        Console.WriteLine($"VisaGateway: Processing payment of {amount}...");
        // Simulated
    }
}

public class MastercardGateway : IPaymentGateway // Another concrete implementation
{
    public void MakePayment(decimal amount)
    {
        Console.WriteLine($"MastercardGateway: Processing payment of {amount}...");
        // Simulated
    }
}

// 3. High-level module depends on Abstraction (through Dependency Injection)
public class OrderProcessor
{
    private readonly IPaymentGateway _paymentGateway; // Dependency on Abstraction

    // Constructor Injection: The dependency is "injected" from outside
    public OrderProcessor(IPaymentGateway paymentGateway)
    {
        // Now, OrderProcessor doesn't know or care whether it's Visa, Mastercard, etc.
        // It only knows it's something that implements IPaymentGateway.
        _paymentGateway = paymentGateway;
    }

    public void Checkout(decimal totalAmount)
    {
        Console.WriteLine("OrderProcessor: Starting checkout process...");
        _paymentGateway.MakePayment(totalAmount);
        Console.WriteLine("OrderProcessor: Checkout complete.");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Using Visa ---");
        
        IPaymentGateway visaImpl = new VisaGateway();
        OrderProcessor visaProcessor = new OrderProcessor(visaImpl); // Inject dependency
        visaProcessor.Checkout(99.99m);

        Console.WriteLine("\n--- Using Mastercard ---");
        IPaymentGateway masterCardImpl = new MastercardGateway();
        OrderProcessor masterCardProcessor = new OrderProcessor(masterCardImpl); // Inject dependency
        masterCardProcessor.Checkout(150.00m);
    }
}
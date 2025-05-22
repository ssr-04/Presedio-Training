// 1. Defining Abstraction with CalculateBonus method
public abstract class EmployeeBase 
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Salary { get; set; }

    // This is extension point:
    // Every Employee type has its own bonus calculation logic.
    public abstract double CalculateBonus();
}

// 2. Concrete Implementations for each employee type
public class FullTimeEmployee : EmployeeBase
{
    public override double CalculateBonus()
    {
        return Salary * 0.10; // 10%
    }
}

public class PartTimeEmployee : EmployeeBase
{
    public override double CalculateBonus()
    {
        return Salary * 0.05; // 5%
    }
}

public class Contractor : EmployeeBase
{
    public override double CalculateBonus()
    {
        return 0; // No bonus 
    }
}

// 3. The Calculation remains 'closed for modification'
public class BonusProcessor
{
    // operates on the abstraction, not concrete types.
    public double ProcessBonus(EmployeeBase employee)
    {
        // It simply leaves the calculation to the specific employee object.
        return employee.CalculateBonus();
    }
}

// let's add a new employee type: Intern
// We simply create a new class without modifying existing ones
public class InternEmployee : EmployeeBase
{
    public override double CalculateBonus()
    {
        return 1000; // Fixed bonus
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        // Creating instances of employee types
        var fullTimeEmployee = new FullTimeEmployee { Id = 1, Name = "Dummeldore", Salary = 50000 };
        var partTimeEmployee = new PartTimeEmployee { Id = 2, Name = "Hagrid", Salary = 20000 };
        var contractor = new Contractor { Id = 3, Name = "Voldemort", Salary = 40000 };

        var processor = new BonusProcessor();

        Console.WriteLine($"Dummeldore's Bonus: {processor.ProcessBonus(fullTimeEmployee)}");
        Console.WriteLine($"Hagrid's Bonus: {processor.ProcessBonus(partTimeEmployee)}");
        Console.WriteLine($"Voldemort's Bonus: {processor.ProcessBonus(contractor)}");


        Console.WriteLine("\n--- Adding a new employee type: Intern ---");


        var intern = new InternEmployee { Id = 4, Name = "Harry Potter", Salary = 15000 };
        Console.WriteLine($"Harry Potter's Bonus: {processor.ProcessBonus(intern)}"); 
    }
}

/*
Why good?

- Reduced introduction of bugs
- Improved maintainability
- SCalability and extensability
*/
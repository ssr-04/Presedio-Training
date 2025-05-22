public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Salary { get; set; }
    public EmployeeType Type { get; set; } //enum
}

public enum EmployeeType
{
    FullTime,
    PartTime,
    Contractor
}

public class BonusCalculator
{
    public double CalculateBonus(Employee employee)
    {
        double bonus = 0;

        switch (employee.Type)
        {
            case EmployeeType.FullTime:
                bonus = employee.Salary * 0.10; // 10%
                break;
            case EmployeeType.PartTime:
                bonus = employee.Salary * 0.05; // 5%
                break;
            case EmployeeType.Contractor:
                bonus = 0; // No bonus
                break;
            default:
                throw new Exception("Unknown employee type.");
        }
        return bonus;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var fullTimeEmployee = new Employee { Id = 1, Name = "Dummeldore", Salary = 50000, Type = EmployeeType.FullTime };
        var partTimeEmployee = new Employee { Id = 2, Name = "Hagrid", Salary = 20000, Type = EmployeeType.PartTime };
        var contractor = new Employee { Id = 3, Name = "Voldemort", Salary = 40000, Type = EmployeeType.Contractor };

        var calculator = new BonusCalculator();

        Console.WriteLine($"Dummeldore's Bonus: {calculator.CalculateBonus(fullTimeEmployee)}");
        Console.WriteLine($"Hagrid's Bonus: {calculator.CalculateBonus(partTimeEmployee)}");
        Console.WriteLine($"Voldemort's Bonus: {calculator.CalculateBonus(contractor)}");
    }
}

/*
Why bad?

- What if we have a employee of type intern - Harry Potter?
- We need to modify the existing employee types and also the switch case for calculating the bonus so it violates the closedness of the OCP
*/
using System;
using System.Collections.Generic;
using System.Linq;

class Helper
{
    public static int GetValidEmployeeID()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Please enter the employee ID: ");
        Console.ResetColor();
        int id;
        while (!int.TryParse(Console.ReadLine(), out id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid input. Please enter a valid integer for ID:");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Please enter the employee ID: ");
            Console.ResetColor();
        }
        return id;
    }

    public static string GetValidEmployeeName()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Please enter the employee name: ");
        Console.ResetColor();
        string? name = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(name))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Name cannot be empty.");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Please enter the employee name: ");
            Console.ResetColor();
            name = Console.ReadLine();
        }
        return name;
    }

    public static int GetValidEmployeeAge()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Please enter the employee age: ");
        Console.ResetColor();
        int age;
        while (!int.TryParse(Console.ReadLine(), out age) || age <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid input. Please enter a valid positive integer for age:");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Please enter the employee age: ");
            Console.ResetColor();
        }
        return age;
    }

    public static double GetValidEmployeeSalary()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Please enter the employee salary: ");
        Console.ResetColor();
        double salary;
        while (!double.TryParse(Console.ReadLine(), out salary) || salary < 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid input. Please enter a valid non-negative number for salary:");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Please enter the employee salary: ");
            Console.ResetColor();
        }
        return salary;
    }
}

public class Employee : IComparable<Employee>
{
    public int Id { get; set; }
    public int Age { get; set; }
    public string Name { get; set; }
    public double Salary { get; set; }

    public Employee() { }

    public Employee(int id, int age, string name, double salary)
    {
        Id = id;
        Age = age;
        Name = name;
        Salary = salary;
    }

    public void TakeEmployeeDetailsFromUser()
    {
        Id = Helper.GetValidEmployeeID();
        if (Id == 0) return; // Special case for stopping
        Name = Helper.GetValidEmployeeName();
        Age = Helper.GetValidEmployeeAge();
        Salary = Helper.GetValidEmployeeSalary();
    }

    public override string ToString()
    {
        return $"Employee ID : {Id}\nName : {Name}\nAge : {Age}\nSalary : {Salary}";
    }

    public int CompareTo(Employee other)
    {
        if (other == null) return 1;
        return Salary.CompareTo(other.Salary);
    }
}

public class EmployeeManager
{
    private Dictionary<int, Employee> employeeDictionary = new();

    public void AddEmployee(Employee employee)
    {
        if (employeeDictionary.ContainsKey(employee.Id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Employee with ID {employee.Id} already exists.");
            Console.ResetColor();
        }
        else
        {
            employeeDictionary[employee.Id] = employee;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Employee with ID {employee.Id} added successfully.");
            Console.ResetColor();
        }
    }

    public void PopulateEmployeesFromUser()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Enter Employee Details (Enter 0 for ID to stop) ---");
        Console.ResetColor();

        while (true)
        {
            Employee newEmployee = new Employee();
            newEmployee.TakeEmployeeDetailsFromUser();

            if (newEmployee.Id == 0)
                break;

            AddEmployee(newEmployee);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("--- Finished Entering Employee Details ---");
        Console.ResetColor();
    }

    public List<Employee> GetEmployeeList() => employeeDictionary.Values.ToList();

    public void SortEmployeesBySalary(List<Employee> employees) => employees.Sort();

    public Employee FindEmployeeById(int id) =>
        employeeDictionary.Values.FirstOrDefault(emp => emp.Id == id);

    public List<Employee> FindEmployeesByName(string name) =>
        employeeDictionary.Values
            .Where(emp => emp.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

    public List<Employee> FindEmployeesOlderThan(Employee employee) =>
        employee == null ? new List<Employee>() :
        employeeDictionary.Values.Where(emp => emp.Age > employee.Age).ToList();

    public void PrintEmployeeList(List<Employee> employees, string title)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n--- {title} ---");
        Console.ResetColor();

        if (employees == null || employees.Count == 0)
        {
            Console.WriteLine("No employees to display.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Blue;
        foreach (var emp in employees)
        {
            Console.WriteLine(emp);
            Console.WriteLine("-----------");
        }
        Console.ResetColor();
    }
}

class Program
{
    public static void Main(string[] args)
    {
        EmployeeManager manager = new();

         // 1. Populate the dictionary with employee details
        manager.PopulateEmployeesFromUser();

        // 2. Get the list from the dictionary
        var allEmployees = manager.GetEmployeeList();

        // 2a. Sort the list by salary and print
        manager.SortEmployeesBySalary(allEmployees);
        manager.PrintEmployeeList(allEmployees, "Employees Sorted by Salary");

        // 2b. Find an employee by ID
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nFinding Employee by ID:");
        Console.ResetColor();
        int searchId = Helper.GetValidEmployeeID();
        var foundEmployee = manager.FindEmployeeById(searchId);
        if (foundEmployee != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nEmployee found with ID {searchId}:");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(foundEmployee);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Employee with ID {searchId} not found.");
        }
        Console.ResetColor();

        // 3. Find all employees with a given name
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nFinding Employee with Name:");
        Console.ResetColor();
        string searchName = Helper.GetValidEmployeeName();
        var employeesByName = manager.FindEmployeesByName(searchName);
        manager.PrintEmployeeList(employeesByName, $"Employees with name '{searchName}'");

        // 4. Find all employees older than a given employee
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nFinding employees older than given EmpID:");
        Console.ResetColor();
        int compareId = Helper.GetValidEmployeeID();
        var compareEmployee = manager.FindEmployeeById(compareId);

        if (compareEmployee != null)
        {
            var olderEmployees = manager.FindEmployeesOlderThan(compareEmployee);
            manager.PrintEmployeeList(olderEmployees, $"Employees Older Than {compareEmployee.Name} (ID: {compareEmployee.Id})");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Employee with ID {compareId} not found. Cannot compare.");
            Console.ResetColor();
        }

        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
    }
}

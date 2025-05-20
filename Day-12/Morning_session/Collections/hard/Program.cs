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

    public bool AddEmployee(Employee employee)
    {
        if (employeeDictionary.ContainsKey(employee.Id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Employee with ID {employee.Id} already exists.");
            Console.ResetColor();
            return false;
        }
        else
        {
            employeeDictionary[employee.Id] = employee;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Employee with ID {employee.Id} added successfully.");
            Console.ResetColor();
            return true;
        }
    }

    public void PopulateEmployeesFromUser()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Enter Employee Details (Enter 0 for ID to stop) ---");
        Console.ResetColor();

        while (true)
        {
            int id = Helper.GetValidEmployeeID();
            if (id == 0)
                break;
            
            if (employeeDictionary.ContainsKey(id))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Employee with ID {id} already exists. Cannot add.");
                Console.ResetColor();
                continue;
            }

            Employee newEmployee = new Employee { Id = id };
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Enter details for Employee ID {id}:");
            Console.ResetColor();
            newEmployee.TakeEmployeeDetailsFromUser();

            AddEmployee(newEmployee);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("--- Finished Entering Employee Details ---");
        Console.ResetColor();
    }

    public void ModifyEmployee(int id)
    {
        if (employeeDictionary.TryGetValue(id, out Employee? employeeToModify))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nEnter new details for Employee ID {id}:");
            Console.ResetColor();

            employeeToModify.TakeEmployeeDetailsFromUser();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Employee with ID {id} modified successfully.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Employee with ID {id} not found. Cannot modify.");
            Console.ResetColor();
        }
    }

    public void DeleteEmployee(int id)
    {
        if (employeeDictionary.Remove(id))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Employee with ID {id} deleted successfully.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Employee with ID {id} not found. Cannot delete.");
            Console.ResetColor();
        }
    }

    public List<Employee> GetEmployeeList() => employeeDictionary.Values.ToList();

    public void PrintAllEmployees()
    {
        PrintEmployeeList(GetEmployeeList(), "All Employee Details");
    }

    public void SortEmployeesBySalary(List<Employee> employees) => employees.Sort();

    public Employee FindEmployeeById(int id)
    {
         // Using dictionary for efficiency (better than Linq)
        employeeDictionary.TryGetValue(id, out Employee? employee);
        return employee;
    }

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
    public static void ShowMenu()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n--- Employee Management Menu ---");
        Console.WriteLine("1. Add New Employee");
        Console.WriteLine("2. Print All Employee Details");
        Console.WriteLine("3. Find Employee by ID");
        Console.WriteLine("4. Modify Employee Details");
        Console.WriteLine("5. Delete Employee by ID");
        Console.WriteLine("6. Sort and Print Employees by Salary");
        Console.WriteLine("7. Find Employees by Name");
        Console.WriteLine("8. Find Employees Older Than a Given Employee");
        Console.WriteLine("9. Exit");
        Console.WriteLine("------------------------------");
        Console.ResetColor();
    }

    public static int GetMenuOption()
    {
        int choice;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Enter your choice: ");
        Console.ResetColor();
        while (!int.TryParse(Console.ReadLine(), out choice))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid input. Please enter a valid number.");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Enter your choice: ");
            Console.ResetColor();
        }
        return choice;
    }


    public static void Main(string[] args)
    {
        EmployeeManager manager = new();

        // Populate data intially?
        //manager.PopulateEmployeesFromUser();

        while (true)
        {
            ShowMenu();
            int choice = GetMenuOption();

            switch (choice)
            {
                case 1: // add new employee
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n--- Add New Employee ---");
                    Console.ResetColor();
                    Employee newEmp = new Employee();
                    int newId = Helper.GetValidEmployeeID();
                    if (newId == 0) 
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Employee ID cannot be 0. Please try again.");
                        Console.ResetColor();
                        break;
                    }
                    newEmp.Id = newId;
                    // Checking for duplicate ID
                    if (manager.FindEmployeeById(newId) != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: Employee with ID {newId} already exists. Cannot add.");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Enter details for Employee ID {newId}:");
                        Console.ResetColor();
                        newEmp.TakeEmployeeDetailsFromUser(); // Gets name, age, salary
                        manager.AddEmployee(newEmp);
                    }
                    break;

                case 2: // print all employee details
                    manager.PrintAllEmployees();
                    break;

                case 3: // find employee by id
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n--- Find Employee by ID ---");
                    Console.ResetColor();
                    int searchId = Helper.GetValidEmployeeID();
                    Employee? foundEmployee = manager.FindEmployeeById(searchId);
                    if (foundEmployee != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nEmployee found with ID {searchId}:");
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(foundEmployee.ToString());
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Employee with ID {searchId} not found.");
                        Console.ResetColor();
                    }
                    break;

                case 4: // modify employee details
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n--- Modify Employee Details ---");
                    Console.ResetColor();
                    int modifyId = Helper.GetValidEmployeeID();
                    manager.ModifyEmployee(modifyId);
                    break;

                case 5: // delete employee by id
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n--- Delete Employee by ID ---");
                    Console.ResetColor();
                    int deleteId = Helper.GetValidEmployeeID();
                    manager.DeleteEmployee(deleteId);
                    break;

                case 6: // sort and print employees by salary
                    List<Employee> allEmployeesForSort = manager.GetEmployeeList();
                    manager.SortEmployeesBySalary(allEmployeesForSort);
                    manager.PrintEmployeeList(allEmployeesForSort, "Employees Sorted by Salary");
                    break;

                case 7: // find employees by name
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n--- Find Employees by Name ---");
                    Console.ResetColor();
                    string searchName = Helper.GetValidEmployeeName();
                    List<Employee> employeesByName = manager.FindEmployeesByName(searchName);
                    manager.PrintEmployeeList(employeesByName, $"Employees with name '{searchName}'");
                    break;

                case 8: // find employees older than a given employee
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n--- Find Employees Older Than ---");
                    Console.WriteLine("Enter the ID of the employee to find others older than:");
                    Console.ResetColor();
                    int compareId = Helper.GetValidEmployeeID();
                    Employee? compareEmployee = manager.FindEmployeeById(compareId);

                    if (compareEmployee != null)
                    {
                        List<Employee> olderEmployees = manager.FindEmployeesOlderThan(compareEmployee);
                        manager.PrintEmployeeList(olderEmployees, $"Employees Older Than {compareEmployee.Name} (ID: {compareEmployee.Id})");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Employee with ID {compareId} not found. Cannot compare.");
                        Console.ResetColor();
                    }
                    break;

                case 9: // exit
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Exiting program. Thank you!");
                    Console.ResetColor();
                    return; // exiting main method

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid choice. Please try again.");
                    Console.ResetColor();
                    break;
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}

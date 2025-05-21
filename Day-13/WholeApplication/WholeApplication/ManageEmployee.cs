using System;
using System.Collections.Generic;
using System.Linq;
using WholeApplication.Exceptions;
using WholeApplication.Interfaces;
using WholeApplication.Models;
using WholeApplication.Repositories;
using WholeApplication.Services;

namespace WholeApplication
{
    public class ManageEmployee
    {
        private readonly IEmployeeService _employeeService;

        public ManageEmployee(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public void Run()
        {
            Console.WriteLine("Welcome to Employee Management System");

            bool exit = false;
            while (!exit)
            {
                DisplayMainMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddEmployee();
                        break;
                    case "2":
                        SearchEmployees();
                        break;
                    case "3":
                        Console.WriteLine("Exiting application. Bye!");
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear(); 
            }
        }

        private void DisplayMainMenu()
        {
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("1. Add New Employee");
            Console.WriteLine("2. Search Employees");
            Console.WriteLine("3. Exit");
            Console.Write("Enter your choice: ");
        }

        private void AddEmployee()
        {
            Console.WriteLine("\n--- Add New Employee ---");
            Employee newEmployee = new Employee();

            Console.Write("Enter Employee Name: ");
            string name = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(name) && name.Length <3 && !name.All(Char.IsLetter))
            {
                Console.Write("Invalid name. Please enter a valid name: ");
                name = Console.ReadLine();
            }
            newEmployee.Name = name;

            Console.Write("Enter Employee Age: ");
            int age;
            while (!int.TryParse(Console.ReadLine(), out age) || age < 18)
            {
                Console.Write("Invalid age. Please enter a positive integer (>=18): ");
            }
            newEmployee.Age = age;

            Console.Write("Enter Employee Salary: ");
            double salary;
            while (!double.TryParse(Console.ReadLine(), out salary) || salary <= 0)
            {
                Console.Write("Invalid salary. Please enter a positive number: ");
            }
            newEmployee.Salary = salary;

            int newId = _employeeService.AddEmployee(newEmployee);

            if (newId != -1)
            {
                Console.WriteLine($"Employee added successfully! Employee ID: {newId}");
            }
            else
            {
                Console.WriteLine("Failed to add employee. Please check for duplicate or other issues.");
            }
        }

        private void SearchEmployees()
        {
            Console.WriteLine("\n--- Search Employees ---");
            SearchModel searchModel = new SearchModel();

            Console.WriteLine("Enter search criteria (leave blank for no filter):");

            Console.Write("Search by ID (integer): ");
            string idInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(idInput) && int.TryParse(idInput, out int id))
            {
                searchModel.Id = id;
            }

            Console.Write("Search by Name: ");
            string nameInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nameInput))
            {
                searchModel.Name = nameInput;
            }

            Console.WriteLine("Search by Age Range:");
            Console.Write("  Minimum Age (integer): ");
            string minAgeInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(minAgeInput) && int.TryParse(minAgeInput, out int minAge))
            {
                if (searchModel.Age == null) searchModel.Age = new Range<int>();
                searchModel.Age.MinVal = minAge;
            }

            Console.Write("  Maximum Age (integer): ");
            string maxAgeInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(maxAgeInput) && int.TryParse(maxAgeInput, out int maxAge))
            {
                if (searchModel.Age == null) searchModel.Age = new Range<int>();
                searchModel.Age.MaxVal = maxAge;
            }

            Console.WriteLine("Search by Salary Range:");
            Console.Write("  Minimum Salary (decimal): ");
            string minSalaryInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(minSalaryInput) && double.TryParse(minSalaryInput, out double minSalary))
            {
                if (searchModel.Salary == null) searchModel.Salary = new Range<double>();
                searchModel.Salary.MinVal = minSalary;
            }

            Console.Write("  Maximum Salary (decimal): ");
            string maxSalaryInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(maxSalaryInput) && double.TryParse(maxSalaryInput, out double maxSalary))
            {
                if (searchModel.Salary == null) searchModel.Salary = new Range<double>();
                searchModel.Salary.MaxVal = maxSalary;
            }

            List<Employee>? searchResults = _employeeService.SearchEmployee(searchModel);

            if (searchResults != null && searchResults.Any())
            {
                Console.WriteLine("\n--- Search Results ---");
                Console.WriteLine("{0,-5} {1,-20} {2,-5} {3,-10}", "ID", "Name", "Age", "Salary");
                Console.WriteLine("-------------------------------------------------");
                foreach (var employee in searchResults)
                {
                    Console.WriteLine("{0,-5} {1,-20} {2,-5} {3,-10:C}", employee.Id, employee.Name, employee.Age, employee.Salary);
                }
            }
            else
            {
                Console.WriteLine("No employees found matching the criteria.");
            }
        }
    }
}
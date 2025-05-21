// Program.cs
using WholeApplication.Interfaces;
using WholeApplication.Models;
using WholeApplication.Repositories;
using WholeApplication.Services;

namespace WholeApplication
{
    public class Program
    {
        static void Main(string[] args)
        {
            IRepository<int, Employee> employeeRepository = new EmployeeRepository();

            IEmployeeService employeeService = new EmployeeService(employeeRepository);

            ManageEmployee employeeUI = new ManageEmployee(employeeService);

            employeeUI.Run();
        }
    }
}
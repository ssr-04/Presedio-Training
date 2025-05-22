namespace example2
{
    internal class Program
    {
        //public delegate void MyDelegate<T>(T num1, T num2); -- generic delegate using <T>
        //public delegate void MyFDelegate(float num1, float num2);
        public void Add(int n1, int n2)
        {
            int sum = n1 + n2;
            Console.WriteLine($"The sum of {n1} and {n2} is {sum}");
        }
        public void Product(int n1, int n2)
        {
            int prod = n1 * n2;
            Console.WriteLine($"The sum of {n1} and {n2} is {prod}");
        }
        Program()
        {
            //MyDelegate<int> del = new MyDelegate<int>(Add); // Usual way to define delegate
            Action<int, int> del = Add; // Using action to create a delegate
            del += Product; // First way, adding a usual function
            //del += delegate (int n1, int n2) // Second way of nameless
            //{
            //    Console.WriteLine($"The division result of {n1} and {n2} is {n1 / n2}");
            //};
            del += (int n1, int n2) => Console.WriteLine($"The division result of {n1} and {n2} is {n1 / n2}"); // Using Lambda expression
            del(100, 20);
        }
        static void Main(string[] args)
        {
            //IRepositor<int, Employee> employeeRepository = new EmployeeRepository();
            //IEmployeeService employeeService = new EmployeeService(employeeRepository);
            //ManageEmployee manageEmployee = new ManageEmployee(employeeService);
            //manageEmployee.Start();
            //new Program();
            Program program = new();
        }
    }
}
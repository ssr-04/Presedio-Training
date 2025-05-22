namespace example3
{
    public class Employee
    {
        public int Id { get; set; }
        public int Age { get; set; }
        public string Name { get; set; }
        public double Salary { get; set; }

        public Employee() 
        {
            Name = string.Empty;
        }

        public Employee(int id, int age, string name, double salary)
        {
            Id = id;
            Age = age;
            Name = name;
            Salary = salary;
        }

        public override string ToString()
        {
            return "Employee ID : " + Id + "\nName : " + Name + "\nAge : " + Age + "\nSalary : " + Salary;
        }
    }
    internal class Program
    {
        List<Employee> employees = new List<Employee>()
        {
            new Employee(101,30, "John Doe",  50000),
            new Employee(102, 25,"Jane Smith",  60000),
            new Employee(103,35, "Sam Brown",  70000)
        };
        
        
        void FindEmployee()
        {
            int empId = 102;
            Predicate<Employee> predicate = e => e.Id == empId;
            Employee? emp = employees.Find(predicate);
            Console.WriteLine(emp.ToString()??"No such employee");
        }
        void SortEmployee()
        {
            var sortedEmployees = employees.OrderBy(e => e.Name);
            foreach (var emp in sortedEmployees)
            {
                Console.WriteLine(emp.ToString());
            }
        }
            static void Main(string[] args)
        {
            Program program = new();
            program.FindEmployee();
            program.SortEmployee();

        }
    }
}

/*
The Predicate<T> Delegate
The key part here is Predicate<Employee> predicate = e => e.Id == empId;.

Predicate<T> is a built-in delegate type in C#. It's defined in the System namespace.
Its signature is very specific: It takes one input parameter of type T and returns a bool.
Think of it like this: public delegate bool Predicate<T>(T obj);
In this example, T is Employee. So, Predicate<Employee> represents any method that takes an Employee object as input and returns true or false.

2. The Lambda Expression e => e.Id == empId
This is where C# becomes very concise and powerful.

e => e.Id == empId is a lambda expression. It's a shorthand way to write an anonymous method (a method without a name).

It does exactly what a Predicate<Employee> needs:

It takes an Employee object (represented by the parameter e).
It returns a bool value (true if e.Id is equal to empId, false otherwise).
Relationship to Delegates: The C# compiler sees this lambda expression and automatically converts it into an instance of the Predicate<Employee> delegate. It's as if you wrote a separate method and then assigned it to the delegate:

List<T>.Find() Method
The line Employee? emp = employees.Find(predicate); is crucial.

The List<T>.Find() method expects a Predicate<T> delegate as an argument.
Its purpose is to iterate through the list and find the first element that satisfies the condition defined by the Predicate.
How it Works (Behind the Scenes):

When you call employees.Find(predicate);, the Find method:

Starts looping through each Employee object in the employees list.
For each Employee object (let's call it currentEmployee), it invokes the predicate delegate, passing currentEmployee as the argument: predicate(currentEmployee).
If predicate(currentEmployee) returns true, it means the condition is met, and Find returns currentEmployee.
If it iterates through the whole list and no element satisfies the condition, it returns null

OrderBy and Func<TSource, TKey>

This is another fantastic example of delegates at play, specifically the Func<TInput, TOutput> built-in delegate.

OrderBy is a LINQ (Language Integrated Query) extension method. LINQ heavily relies on delegates (and expression trees for database queries, but for in-memory collections, it's delegates).
The OrderBy method takes a Func<Employee, string> delegate as an argument.
Func<TInput, TOutput> represents a method that takes an input of type TInput and returns an output of type TOutput.
In this case, TInput is Employee, and TOutput is string (the Name property).
The lambda expression e => e.Name perfectly matches this signature. It takes an Employee (e) and returns a string (e.Name).
How it Works: OrderBy uses this Func delegate to extract the "key" (in this case, the Name) by which to sort the elements. It invokes the delegate for each Employee to get its name and then performs the sorting operation.
Relationship to Delegates Explained
The examples Predicate<Employee> in FindEmployee() and OrderBy in SortEmployee() are fundamentally about passing behavior (methods/functions) as arguments to other methods. This is the core concept of delegates.

Predicate<T>: Allows you to pass a "test" or "condition" method to a generic algorithm (like List<T>.Find).
Func<TInput, TOutput>: Allows you to pass a "transformation" or "key extraction" method to a generic algorithm (like Enumerable.OrderBy, Select, etc.).
Why is this powerful?

Flexibility and Reusability: The List<T>.Find() method doesn't need to know how you want to find an employee (by ID, by name, by salary range, etc.). You provide the specific logic at runtime using the Predicate delegate. The Find method remains generic and reusable for any Predicate you supply.
Decoupling: The List class is decoupled from the specific search logic. It just knows it needs some method that can evaluate an item and return true/false.
Conciseness (with Lambdas): Lambda expressions make creating these "on-the-fly" methods incredibly compact and readable, especially for simple operations like checking an ID or getting a name.
*/

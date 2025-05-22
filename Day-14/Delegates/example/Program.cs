namespace Example
{
    internal class Program
    {
        public delegate void MyDelegate(int num1, int num2);

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
            MyDelegate del = new MyDelegate(Add);
            del += Product;
            del(10, 20);
        }
        static void Main(string[] args)
        {
            new Program();
        }
    }
}

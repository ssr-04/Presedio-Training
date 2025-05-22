// a common interface for shapes that have an area
public interface IShapeWithArea
{
    double GetArea();
}

public class Rectangle : IShapeWithArea
{
    public double Width { get; set; }
    public double Height { get; set; }

    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public double GetArea() => Width * Height;
}

public class Square : IShapeWithArea
{
    public double Side {get; set;}

    public Square(double side)
    {
        Side = side;
    }

    public double GetArea() => Side * Side;
}

public class Program
{
    // methods now depends on the IShapeWithArea interface
    public static void PrintArea(IShapeWithArea shape)
    {
        Console.WriteLine($"Shape Area: {shape.GetArea()}");
    }

    public static void Main(string[] args)
    {
        Rectangle myRectangle = new Rectangle(5, 4);
        Square mySquare = new Square(5);

        PrintArea(myRectangle);
        PrintArea(mySquare);  
    }
}

/*
Why good?

- We are implementing square and rectangle from a interface IShapeWithArea that doesn't have the predefined getters and setters which introduced the conflict in bad example when trying to enlarge the shape.
- So better to always inherit from a interface that has only the absolute common things defined in them.

*/
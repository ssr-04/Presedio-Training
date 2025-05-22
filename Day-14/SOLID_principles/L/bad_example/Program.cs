public class Rectangle
{
    public virtual double Width { get; set; }
    public virtual double Height { get; set; }

    public double Area => Width * Height;

    public override string ToString()
    {
        return $"The height is {Height} and width is {Width}";
    }
}

public class Square : Rectangle
{
    // square have equal sides.
    // To enforce this, override the setters.
    public override double Width
    {
        get => base.Width;
        set { base.Width = value; base.Height = value; } // Setting width also resets height
    }

    public override double Height
    {
        get => base.Height;
        set { base.Height = value; base.Width = value; } // Setting height also resets width
    }
}

public class Program
{
    // method that expects a Rectangle and operates on it.
    public static void EnlargeRectangle(Rectangle rect, double newWidth, double newHeight)
    {
        rect.Width = newWidth;
        rect.Height = newHeight;
        Console.WriteLine($"Rectangle resized to Width: {rect.Width}, Height: {rect.Height}. Area: {rect.Area}");
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("--- Using Rectangle ---");
        Rectangle myRectangle = new Rectangle { Width = 5, Height = 4 };
        Console.WriteLine("\n--- Before enlarging Rectangle ---");
        Console.WriteLine(myRectangle);
        EnlargeRectangle(myRectangle, 6, 8); // Expected: Width 6, Height 8, Area 48

        Console.WriteLine("\n--- Using Square (LSP Violation) ---");
        Square mySquare = new Square { Width = 5 }; // Initializes both width and height to 5
        Console.WriteLine("--- Before enlarging Sqaure ---");
        Console.WriteLine(mySquare);
        // substituting Square where Rectangle is expected:
        EnlargeRectangle(mySquare, 6, 8); // Expected: Width 6, Height 8, Area 48
                                         // Actual:   Width 8, Height 8, Area 64
                                         
    }
}

/*
Why bad ?

 - The EnlargeRectangle method expects a Rectangle. It sets the Width and Height independently and expects them to remain that way.
 - When a Square object is substituted, its overridden setters introduce unexpected behavior: setting Width also changes Height, and vice-versa.
 - The contract of Rectangle (that Width and Height can be set independently) is violated by the Square subtype.
 - The client (EnlargeRectangle) no longer behaves correctly when given a Square. It makes an assumption about how Width and Height work based on Rectangle, which Square subtly breaks.
 - So an assumption that square is same as the rectangle and inherting from it causes the issue
*/
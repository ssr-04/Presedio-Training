// Define interfaces for a family of related UI components (Abstract products)
public interface IButton
{
    void Paint();
}

public interface ICheckbox
{
    void Paint();
}

public interface ITextBox
{
    void Paint();
}

//==============================================================================================================
// Concrete products-1 (Windows)
public class WindowsButton : IButton
{
    public void Paint() => Console.WriteLine("Rendering a Windows-style Button.");
}

public class WindowsCheckbox : ICheckbox
{
    public void Paint() => Console.WriteLine("Rendering a Windows-style Checkbox.");
}

public class WindowsTextBox : ITextBox
{
    public void Paint() => Console.WriteLine("Rendering a Windows-style Textbox.");
}
// Concrete products-2 (Mac)
public class MacOSButton : IButton
{
    public void Paint() => Console.WriteLine("Rendering a macOS-style Button.");
}

public class MacOSCheckbox : ICheckbox
{
    public void Paint() => Console.WriteLine("Rendering a macOS-style Checkbox.");
}

public class MacOSTextBox : ITextBox
{
    public void Paint() => Console.WriteLine("Rendering a macOS-style Textbox.");
}
// ---------------------------------------------------------------------------------------------
// Abstract factory
public interface IUIFactory
{
    IButton CreateButton();
    ICheckbox CreateCheckbox();
    ITextBox CreateTextBox();
}
// ---------------------------------------------------------------------------------------------
// Concrete factory
public class WindowsUIFactory : IUIFactory
{
    public IButton CreateButton() => new WindowsButton();
    public ICheckbox CreateCheckbox() => new WindowsCheckbox();
    public ITextBox CreateTextBox() => new WindowsTextBox();
}

public class MacOSUIFactory : IUIFactory
{
    public IButton CreateButton() => new MacOSButton();
    public ICheckbox CreateCheckbox() => new MacOSCheckbox();
    public ITextBox CreateTextBox() => new MacOSTextBox();
}
// -----------------------------------------------------------------------------------------------------
// Client

public class Application
{
    private readonly IButton _button;
    private readonly ICheckbox _checkbox;
    private readonly ITextBox _textBox;

    // client consumes the Abstract Factory via DI
    public Application(IUIFactory factory)
    {
        Console.WriteLine($"\nApplication initializing with {factory.GetType().Name}...");
        _button = factory.CreateButton();
        _checkbox = factory.CreateCheckbox();
        _textBox = factory.CreateTextBox();
    }

    public void Run()
    {
        Console.WriteLine("Application running, painting UI elements:");
        _button.Paint();
        _checkbox.Paint();
        _textBox.Paint();
        Console.WriteLine("UI elements painted.");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Abstract Factory Pattern Example ---");


        // Windows UI
        Console.WriteLine("\n--- Running with Windows UI ---");
        IUIFactory windowsFactory = new WindowsUIFactory();
        Application windowsApp = new Application(windowsFactory);
        windowsApp.Run();

        // macOS UI
        Console.WriteLine("\n--- Running with macOS UI ---");
        IUIFactory macOSFactory = new MacOSUIFactory();
        Application macOSApp = new Application(macOSFactory);
        macOSApp.Run();

        // How OCP ?
        // If we need a new UI style (like Linux UI),
        // we just add a new set of Linux-specific IButton, ICheckbox, ITextBox implementations,
        // and a new LinuxUIFactory.
        // We DO NOT need to modify the existing ones
    }
}
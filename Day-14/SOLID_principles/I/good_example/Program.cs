// Human worker (basic stuff)
public interface IHumanWorker
{
    void Eat();
    void Sleep();
}

// something for human workers
public interface IWorkable
{
    void Work();
    void TakeBreak();
}

// Specific responsibilities
public interface ICodeCapable
{
    void Code();
}

public interface IProjectManageable
{
    void ManageProjects();
}

public interface IAutomatedWorker
{
    void PerformAutomatedTasks();
    void ChargeBattery();
}

//concrete classes now implement only the interfaces they truly need
public class Developer : IHumanWorker, IWorkable, ICodeCapable
{
    public void Eat()
    {
        Console.WriteLine("Developer: Eating lunch.");
    }

    public void Sleep()
    {
        Console.WriteLine("Developer: Sleeping.");
    }

    public void Work()
    {
        Code();
    }

    public void TakeBreak()
    {
        Console.WriteLine("Developer: Taking a coffee break.");
    }

    public void Code()
    {
        Console.WriteLine("Developer: Writing code.");
    }
}

public class Manager : IHumanWorker, IWorkable, IProjectManageable
{
    public void Eat()
    {
        Console.WriteLine("Manager: Eating lunch.");
    }

    public void Sleep()
    {
        Console.WriteLine("Manager: Sleeping.");
    }

    public void Work()
    {
        ManageProjects();
    }

    public void TakeBreak()
    {
        Console.WriteLine("Manager: Taking a quick break.");
    }

    public void ManageProjects()
    {
        Console.WriteLine("Manager: Planning tasks.");
    }
}

public class RobotWorker : IAutomatedWorker, IWorkable
{
    public void PerformAutomatedTasks()
    {
        Console.WriteLine("Robot: Performing automated tasks.");
    }

    public void ChargeBattery()
    {
        Console.WriteLine("Robot: Charging battery.");
    }

    public void Work() 
    {
        PerformAutomatedTasks();
    }

    public void TakeBreak() 
    {
        ChargeBattery();
    }
}


public class Program
{
    public static void Main(string[] args)
    {
        Developer dev = new Developer();
        Manager mgr = new Manager();
        RobotWorker robot = new RobotWorker();

        Console.WriteLine("--- Developer Actions ---");
        dev.Work();
        dev.Code();
        dev.Eat();

        Console.WriteLine("\n--- Manager Actions ---");
        mgr.Work();
        mgr.ManageProjects();
        mgr.Sleep();

        Console.WriteLine("\n--- Robot Actions ---");
        robot.Work();
        robot.ChargeBattery();

    }
}
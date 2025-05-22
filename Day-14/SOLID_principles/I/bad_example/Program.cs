// "fat" interface, trying to more stuff
public interface IWorker
{
    void Work(); // Yep, all does
    void Eat();  // Yep, all does
    void Sleep(); // Yep, all does
    void ManageProjects(); // Only managers manage projects
    void Code(); // Only developers
    void TakeBreak(); // maybe or depends
}

public class Developer : IWorker
{
    public void Work()
    {
        Code();
    }

    public void Eat()
    {
        Console.WriteLine("Developer: Eating lunch.");
    }

    public void Sleep()
    {
        Console.WriteLine("Developer: Sleeping.");
    }

    public void ManageProjects()
    {
        Console.WriteLine("Developer: I don't manage projects."); 
    }

    public void Code()
    {
        Console.WriteLine("Developer: Writing C# code.");
    }

    public void TakeBreak()
    {
        Console.WriteLine("Developer: Taking a coffee break.");
    }
}

public class Manager : IWorker
{
    public void Work()
    {
        ManageProjects(); // A manager's work is managing
    }

    public void Eat()
    {
        Console.WriteLine("Manager: Eating lunch.");
    }

    public void Sleep()
    {
        Console.WriteLine("Manager: Sleeping.");
    }

    public void ManageProjects()
    {
        Console.WriteLine("Manager: Planning tasks.");
    }

    public void Code()
    {
        Console.WriteLine("Manager: I don't code anymore."); 
    }

    public void TakeBreak()
    {
        Console.WriteLine("Manager: Taking a quick break.");
    }
}

public class RobotWorker : IWorker
{
    public void Work()
    {
        Console.WriteLine("Robot: Performing automated tasks.");
    }

    public void Eat()
    {
        // Robots don't eat
        Console.WriteLine("Robot: I don't eat, Charging battery."); 
    }

    public void Sleep()
    {
        // Robots don't sleep
        Console.WriteLine("Robot: I don't sleep, Entering low-power mode.");
    }

    public void ManageProjects()
    {
        Console.WriteLine("Robot: Cannot manage projects.");
    }

    public void Code()
    {
        Console.WriteLine("Robot: Executing pre-programmed instructions (Not anymore -).");
    }

    public void TakeBreak()
    {
        // Robots don't take.
        Console.WriteLine("Robot: I don't take breaks humans.. so Performing self-maintenance.");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        IWorker dev = new Developer();
        IWorker mgr = new Manager();
        IWorker robot = new RobotWorker();

        dev.Work();
        mgr.Work();
        robot.Work();

        dev.Eat();
        robot.Eat();
    }
}

/*
Why bad?

- The interface is fat with too many methods so we are forced to implement even if it doesn't makes sense.
- Maybe can lead to NotImplemented error

*/
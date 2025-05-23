using System.Threading; // For sleep

// Defines the interface for both the Real Image and the Proxy
public interface IImage
{
    void Display();
    void GetImageInfo();
}

// The real image loading class, which is resource-intensive
public class RealImage : IImage
{
    private string _fileName;

    public RealImage(string fileName)
    {
        _fileName = fileName;
        LoadImageFromDisk(); // Simulate io operation
    }

    private void LoadImageFromDisk()
    {
        Console.WriteLine($"[RealImage]: Loading '{_fileName}' from disk... (This takes time)");
        Thread.Sleep(2000); // Simulating delay
        Console.WriteLine($"[RealImage]: '{_fileName}' loaded.");
    }

    public void Display()
    {
        Console.WriteLine($"[RealImage]: Displaying '{_fileName}'.");
    }

    public void GetImageInfo()
    {
        Console.WriteLine($"[RealImage]: Info for '{_fileName}': Resolution 1920x1080, Size 12.5MB.");
    }
}

// The Proxy class, which controls access to RealImage & adds lazy loading
public class ImageProxy : IImage
{
    private string _fileName;
    private RealImage? _realImage; // Reference to the Real image

    public ImageProxy(string fileName)
    {
        _fileName = fileName;
        Console.WriteLine($"[ImageProxy]: Proxy created for '{_fileName}'. (Real one not yet loaded)");
    }

    public void Display()
    {
        // Lazy Init: RealImage is created only when display() is called for the first time
        if (_realImage == null)
        {
            _realImage = new RealImage(_fileName);
        }
        _realImage.Display();
    }

    public void GetImageInfo()
    {
        // This method might provide information without loading real image
        Console.WriteLine($"[ImageProxy]: Getting info for '{_fileName}' (without loading real image).");
        Console.WriteLine($"[ImageProxy Info]: Placeholder info for '{_fileName}'.");

        // If more info required then loading:
        // if (_realImage == null)
        // {
        //     _realImage = new RealImage(_fileName);
        // }
        // _realImage.GetImageInfo();
    }
}

public class Program
{
    public static void Main(string[] args)
    {

        // 1 Using the Proxy for lazy loading
        Console.WriteLine("\n--- 1: Lazy Loading Image ---");
        IImage image1 = new ImageProxy("some_large_image.jpg"); 

        Console.WriteLine("user not yet on image focus...");
        Thread.Sleep(1000); // Simulate delay

        // when user reaches image viewport, will trigger the real image loading.
        Console.WriteLine("\nrequest to DISPLAY image1:");
        image1.Display(); // real constructor call

        // Get info without displaying with proxy
        Console.WriteLine("\nrequest to GET INFO for image1:");
        image1.GetImageInfo(); // This can be handled by proxy without loading

        // 2: Directly using RealImage (without proxy)
        Console.WriteLine("\n--- 2: Direct Loading Real Image ---");
        Console.WriteLine("Creating RealImage directly:");
        IImage image2 = new RealImage("small_thumbnail.png"); 
        image2.Display(); 
        image2.GetImageInfo();

        Console.WriteLine("\nApplication Finished.");
    }
}
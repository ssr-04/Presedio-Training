// Defines the interface for objects the factory method creates (Product)
public interface IDocument
{
    void Open();
    void Save();
    void Close();
    void Print();
}

// --------------------------------------------------------------------------------------
// Concrete implementation of the IDocument interface (Concrete Product)
public class WordDocument : IDocument
{
    public void Open() => Console.WriteLine("Opening Word Document.");
    public void Save() => Console.WriteLine("Saving Word Document.");
    public void Close() => Console.WriteLine("Closing Word Document.");
    public void Print() => Console.WriteLine("Printing Word Document.");
}

public class PdfDocument : IDocument
{
    public void Open() => Console.WriteLine("Opening PDF Document.");
    public void Save() => Console.WriteLine("Saving PDF Document.");
    public void Close() => Console.WriteLine("Closing PDF Document.");
    public void Print() => Console.WriteLine("Printing PDF Document.");
}

// --------------------------------------------------------------------------------
// Declares the factory method, which returns an object of the Product type.(Creator/Factory method)
public abstract class DocumentCreator
{
    // Subclasses will override to specify which concrete document to create.
    protected abstract IDocument CreateDocument();

    public IDocument NewDocument()
    {
        IDocument document = CreateDocument();
        Console.WriteLine("--- Creating New Document ---");
        document.Open();
        Console.WriteLine("Document created and opened.");
        return document;
    }
}
// ---------------------------------------------------------------------------------
// Concrete Creators override the factory method to return a specific Concrete Product.
public class WordDocumentCreator : DocumentCreator
{
    protected override IDocument CreateDocument()
    {
        return new WordDocument();
    }
}

public class PdfDocumentCreator : DocumentCreator
{
    protected override IDocument CreateDocument()
    {
        return new PdfDocument();
    }
}
// ---------------------------------------------------------------------------------------
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Document Application...\n");

        // Create a Word document (DIP)
        DocumentCreator wordCreator = new WordDocumentCreator();
        IDocument myWordDoc = wordCreator.NewDocument();
        myWordDoc.Save();
        myWordDoc.Print();
        myWordDoc.Close();

        Console.WriteLine("\n-----------------------------------\n");

        // Create a PDF document
        DocumentCreator pdfCreator = new PdfDocumentCreator();
        IDocument myPdfDoc = pdfCreator.NewDocument();
        myPdfDoc.Save();
        myPdfDoc.Print();
        myPdfDoc.Close();

        Console.WriteLine("\n-----------------------------------\n");

        Console.WriteLine("\nDocument Application Finished.");

        // How OCP ?
        // If we need a new document type (e.g., PresentationDocument),
        // we just add a new PresentationDocument and PresentationDocumentCreator class.
        // We DO NOT need to modify the existing IDocument, WordDocument, PdfDocument,
        // This is "open for extension, closed for modification."
    }
}
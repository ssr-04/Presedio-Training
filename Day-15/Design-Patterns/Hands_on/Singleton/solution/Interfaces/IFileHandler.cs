namespace solution
{
    public interface IFileHandler : IDisposable
    {
        void WriteLine(string content);
        string ReadAllContent();
        void EnsureFileClosed();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solution
{
    public sealed class FileHandler : IFileHandler
    {
        // Eager Initialization: The instance is created immediately when the class is loaded.
        private static readonly FileHandler _instance = new FileHandler();

        // The single instance of the FileHandler.
        public static FileHandler Instance = _instance;

        private StreamWriter? _writer;
        private readonly string _filePath;
        private readonly SemaphoreSlim _fileAccessLock = new SemaphoreSlim(1, 1);

        // Private constructor to prevent external instantiation.
        private FileHandler()
        {
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_data.txt");

            Console.WriteLine($"[AppFileHandler]: Initializing.... File path: {_filePath}");
            EnsureFileOpened();
        }

        private void EnsureFileOpened()
        {
            try
            {
                _writer = new StreamWriter(new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read));
                _writer.AutoFlush = true; // writes are immediately flushed to file

                Console.WriteLine($"[AppFileHandler]: File '{_filePath}' opened for writing.");
            }
  
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[AppFileHandler Error]: Access to file '{_filePath}' failed: {ex.Message}");
            }
        }

        public void WriteLine(string content)
        {
            _fileAccessLock.Wait();
            try
            {
                if (_writer == null)
                {
                    throw new Exception("File writer is not initialized.");
                }
                _writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {content}");
                Console.WriteLine($"[AppFileHandler]: Wrote: '{content}'");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[AppFileHandler Error]: Failed to write to file: {ex.Message}");
            }
            finally
            {
                _fileAccessLock.Release();
            }
        }

        public string ReadAllContent()
        {
            _fileAccessLock.Wait();
            try
            {
                if (!File.Exists(_filePath))
                {
                    Console.WriteLine($"[AppFileHandler]: File '{_filePath}' does not exist for reading.");
                    return string.Empty;
                }

                using (var reader = new StreamReader(new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    string content = reader.ReadToEnd();
                    Console.WriteLine($"[AppFileHandler]: Read all content.");
                    return content;
                }
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"[AppFileHandler Error]: Failed to read file: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                _fileAccessLock.Release();
            }
        }

        public void EnsureFileClosed()
        {
            _fileAccessLock.Wait();
            try
            {
                if (_writer != null)
                {
                    _writer.Close();
                    _writer.Dispose();
                    _writer = null;
                    Console.WriteLine($"[AppFileHandler]: File '{_filePath}' writer closed.");
                }
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"[AppFileHandler Error]: Failed to close file: {ex.Message}");
            }
            finally
            {
                _fileAccessLock.Release();
                _fileAccessLock.Dispose();
                Console.WriteLine($"[AppFileHandler]: Semaphore disposed.");
            }
        }

        public void Dispose()
        {
            EnsureFileClosed();
        }

        ~FileHandler()
        {
            Console.WriteLine("[AppFileHandler]: Destructor running. Ensuring file is closed.");
            EnsureFileClosed();
        }
    }

}

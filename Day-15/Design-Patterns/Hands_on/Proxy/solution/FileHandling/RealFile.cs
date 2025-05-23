using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using solution.Interfaces;

namespace solution.FileHandling
{
    public class RealFile : IFile
    {
        private readonly string _filePath;

        public RealFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.");
            }
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified file was not found: '{filePath}'");
            }

            _filePath = filePath;
            Console.WriteLine($"[RealFile]: '{Path.GetFileName(_filePath)}' linked. Ready to read content.");
        }

        public string Read()
        {
            Console.WriteLine($"[RealFile]: Accessing and reading ALL content from '{Path.GetFileName(_filePath)}'...");
            try
            {
                return File.ReadAllText(_filePath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[RealFile Error]: Failed to read file '{_filePath}': {ex.Message}");
                return $"[Error Reading File]: {ex.Message}";
            }
        }

        // Helper method to get file name (for ProxyFile)
        public string GetFileName() => Path.GetFileName(_filePath);
    }

}

// File.ReadAllText(_filePath) is a static helper method from System.IO.File. 
// This method handles the opening and closing of the file stream internally and automatically.
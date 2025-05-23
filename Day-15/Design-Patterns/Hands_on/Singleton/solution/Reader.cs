using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solution
{
    public class ReportReaderService
    {
        private readonly IFileHandler _fileHandler;

        public ReportReaderService(IFileHandler fileHandler)
        {
            _fileHandler = fileHandler;
        }

        public void PrintAllLogs()
        {
            Console.WriteLine("\n--- All Log Entries ---");
            string content = _fileHandler.ReadAllContent();
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("(No log entries found)");
            }
            else
            {
                Console.WriteLine(content);
            }
            Console.WriteLine("-----------------------");
        }

    }
}

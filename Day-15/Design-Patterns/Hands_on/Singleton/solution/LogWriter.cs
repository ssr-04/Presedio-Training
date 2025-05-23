using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solution
{
    public class LogWriterService
    {
        private readonly IFileHandler _fileHandler;

        public LogWriterService(IFileHandler fileHandler)
        {
            _fileHandler = fileHandler;
        }

        public void LogUserAction(string userName, string action)
        {
            _fileHandler.WriteLine($"User '{userName}' performed action: '{action}'.");
        }


    }
}
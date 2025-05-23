using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using solution.Interfaces;
using solution.Models;

namespace solution.FileHandling
{
    public class ProxyFile : IFile
    {
        private readonly RealFile _realFile; //real file refernce
        private readonly User _currentUser;  //user attempting to access the file

        public ProxyFile(RealFile realFile, User user)
        {
            _realFile = realFile ?? throw new ArgumentNullException(nameof(realFile));
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            Console.WriteLine($"[ProxyFile]: Created for '{realFile.GetFileName()}' by '{user.Username}'.");
        }

        public string Read()
        {
            Console.WriteLine($"\n--- Access Attempt by {_currentUser.Username} (Role: {_currentUser.Role}) ---");

            switch (_currentUser.Role)
            {
                case UserRole.Admin:
                    Console.WriteLine("[Access Granted] Admin: Full access to sensitive content.");
                    return _realFile.Read();
                case UserRole.User:
                    Console.WriteLine("[Access Limited] User: Sensitive content (lines starting with '[secret]') will be filtered.");
                    string fullContent = _realFile.Read();
                    List<string> filteredLines = new List<string>();
                    foreach (string line in fullContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!line.TrimStart().StartsWith("[secret]", StringComparison.OrdinalIgnoreCase))
                        {
                            filteredLines.Add(line);
                        }
                    }
                    return string.Join("\n", filteredLines);
                case UserRole.Guest:
                    Console.WriteLine("[Access Denied] Guest: You do not have permission to read this file.");
                    return "[Access Denied]";
                default:
                    Console.WriteLine("[Access Denied] Unknown Role: Access denied due to unrecognized role.");
                    return "[Access Denied]";
            }
        }

    }
}

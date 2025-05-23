using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solution.Models
{
    public enum UserRole
    {
        Admin,
        User,
        Guest,
        Unknown
    }
    public class User
    {
        public string Username { get; } = string.Empty;
        public UserRole Role { get; }

        public User(string username, UserRole role)
        {
            // validation
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty.");
            }
            Username = username;
            Role = role;
        }

        public override string ToString()
        {
            return $"User: {Username} | Role: {Role}";
        }

    }

}

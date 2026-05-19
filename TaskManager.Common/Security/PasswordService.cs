using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TaskManager.Common.Interfaces;

namespace TaskManager.Common.Security
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<object> _hasher = new();

        /// <summary>
        /// Hash password
        /// </summary>
        public string Hash(string password)
        {
            return _hasher.HashPassword(null, password);
        }

        /// <summary>
        /// Verify password against hash
        /// </summary>
        public bool Verify(string password, string hash)
        {
            var result = _hasher.VerifyHashedPassword(null, hash, password);
            
            return result == PasswordVerificationResult.Success;
        }
    }
}

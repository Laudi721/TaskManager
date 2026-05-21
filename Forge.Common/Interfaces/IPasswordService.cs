using System;
using System.Collections.Generic;
using System.Text;

namespace Forge.Common.Interfaces
{
    public interface IPasswordService
    {
        /// <summary>
        /// Hash password
        /// </summary>
        public string Hash(string password);

        /// <summary>
        /// Verify password against hash
        /// </summary>
        public bool Verify(string password, string hash);
    }
}

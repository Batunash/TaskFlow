using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.Interfaces
{
    public interface IPasswordHash
    {
        string Hash(string password);
        bool Verify(string password, string hash);
 
    }
}

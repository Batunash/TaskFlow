using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Entities
{
    internal class Organization
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        private Organization() { }
        public Organization(string name)
        {
            Name = name;
        }
    }
}

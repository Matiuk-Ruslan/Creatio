using System;

namespace RMQ.Files.cs.Contracts
{
    public class ContactContract
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
    }
}
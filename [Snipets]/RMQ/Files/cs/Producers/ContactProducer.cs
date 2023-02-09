using System.Threading.Tasks;
using RMQ.Files.cs.Contracts;
using MassTransit;
using NLog;

namespace RMQ.Files.cs.Producers
{
    public class ContactProducer
    {
        private readonly IBus _bus;
        private readonly ILogger _log;

        public ContactProducer(IBus bus, ILogger log) 
        {
            _bus = bus;
            _log = log;
        }
        
        public async Task ContactPublish(ContactContract contact)
        {
            await _bus.Publish(contact);

            //TODO: Удалить лог
            _log.Info("ContactProducer.ContactPublish");
        }
    }
}
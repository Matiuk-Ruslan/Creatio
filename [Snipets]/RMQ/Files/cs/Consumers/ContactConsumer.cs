using System.Threading.Tasks;
using RMQ.Files.cs.Contracts;
using MassTransit;
using NLog;

namespace RMQ.Files.cs.Consumers
{
    public class ContactConsumer : IConsumer<ContactContract>
    {
        public async Task Consume(ConsumeContext<ContactContract> context)
        {
            ILogger log = LogManager.GetLogger("MaibBaseLogger");
            log.Info("Получил Id: {0} из очереди", context.Message.Id);
            log.Info("Записал Id: {0} в базу данных", context.Message.Id);
        }
    }
}
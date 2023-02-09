using System;
using Terrasoft.Core.Configuration;
using Terrasoft.Core;
using RMQ.Files.cs.Consumers;
using MassTransit;
using NLog;

namespace RMQ.Files.cs.Extensions
{
    public static class MassTransitExtension
    {
        private static IBusControl _busControl;
        private static ILogger _logger = LogManager.GetLogger("MaibBaseLogger");

        public static void Start(UserConnection userConnection)
        {
            try
            {
                _busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(Convert.ToString(SysSettings.GetValue(userConnection, "UsrRabbitMQConnectionString")));

                    cfg.ReceiveEndpoint("entity-exchange", e =>
                    {
                        e.Consumer<ContactConsumer>();
                    });
                });

                _busControl.StartAsync();
            }
            catch (Exception ex) { _logger.Error(ex.Message); }

            //TODO: Удалить лог
            _logger.Info("MassTransitExtension.Start");
        }

        public static IBusControl BusControl()
        {
            return _busControl;
        }

        public static void Stop()
        {
            try { _busControl.StopAsync().GetAwaiter().GetResult(); }
            catch (Exception ex) { _logger.Info(ex.Message); }

            //TODO: Удалить лог
            _logger.Info("MassTransitExtension.Stop");
        }
    }
}
using System;
using Terrasoft.Core.Factories;
using RMQ.Files.cs.Extensions;
using NLog;
using RMQ.Api;

namespace MaibBase.Files.cs
{
    [DefaultBinding(typeof(IStartup))]
    public class Startup : IStartup
    {
        private readonly ILogger _logger;

        public Startup()
        {
            _logger = LogManager.GetLogger("MaibBaseLogger");
        }

        public void AppStart(object userConnection)
        {
            try
            {
                //TODO: Включить MassTransitExtension.Start
                //MassTransitExtension.Start(userConnection as UserConnection);

                //TODO: Удалить лог
                _logger.Info("Startup.AppStart.MassTransitExtension.Start");
            }
            catch (Exception ex) { _logger.Error(ex.Message); }
        }

        public void AppEnd()
        {
            try
            {
                //TODO: Включить MassTransitExtension.Stop
                //MassTransitExtension.Stop() ;

                //TODO: Удалить лог
                _logger.Info("Startup.AppEnd.MassTransitExtension.Stop");
            }
            catch (Exception ex) { _logger.Error(ex.Message); }
        }
    }
}
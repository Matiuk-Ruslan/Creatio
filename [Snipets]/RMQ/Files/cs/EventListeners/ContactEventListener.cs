//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Terrasoft.Core;
//using Terrasoft.Core.Entities;
//using Terrasoft.Core.Entities.Events;
//using RMQ.Files.cs.Contracts;
//using RMQ.Files.cs.Extensions;
//using RMQ.Files.cs.Producers;
//using MassTransit;
//using NLog;

//namespace RMQ.Files.cs.EventListeners
//{
//    [EntityEventListener(SchemaName = "Contact")]
//    public class ContactEventListener : BaseEntityEventListener
//    {
//        private readonly ILogger _log;
//        private readonly IBusControl _busControl;

//        public ContactEventListener ()
//        {
//            _log = LogManager.GetLogger("MaibBaseLogger");
//            _busControl = MassTransitExtension.BusControl();
//        }

//        public override void OnUpdated(object sender, EntityAfterEventArgs e)
//        {
//            base.OnUpdated(sender, e);

//            Entity entity = (Entity)sender;
//            UserConnection userConnection = entity.UserConnection;

//            Guid contactId = e.PrimaryColumnValue;
//            bool isNameModified = e.ModifiedColumnValues.Any(column => column.Name == "Name");

//            if(isNameModified) 
//            {
//                ContactContract contact = new ContactContract();
//                contact.Id = contactId;
//                contact.Name = entity.GetTypedColumnValue<string>("Name");
//                contact.MobilePhone = entity.GetTypedColumnValue<string>("MobilePhone");
//                contact.Email = entity.GetTypedColumnValue<string>("Email");

//                //TODO: Удалить лог
//                _log.Info("ContactEventListener.OnUpdated.Id: {contactId}");

//                ContactProducer contactProducer = new ContactProducer(_busControl, _log);
//                Task.Run(async () => { await contactProducer.ContactPublish(contact); }).Wait();
//            }
//        }
//    }
//}
//TODO: Включить [EntityEventListener(SchemaName = "Contact")]
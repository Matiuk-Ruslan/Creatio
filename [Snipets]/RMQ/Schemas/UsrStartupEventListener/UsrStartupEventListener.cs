namespace Terrasoft.Configuration
{
	using Terrasoft.Web.Common;
    using MaibBase.Api;
    using Terrasoft.Core.Factories;
    using Terrasoft.Core;

    public class UsrStartupEventListener : AppEventListenerBase
    {
        protected UserConnection UserConnection { get; set; }

        protected UserConnection GetUserConnection(AppEventContext context)
        {
            AppConnection appConnection = context.Application["AppConnection"] as AppConnection;
            return appConnection.SystemUserConnection;
        }

        public override void OnAppStart(AppEventContext context)
        {
            base.OnAppStart(context);
            UserConnection = GetUserConnection(context);

            IStartup startup = ClassFactory.Get<IStartup>();
            startup.AppStart(UserConnection as object);
        }

        public override void OnAppEnd(AppEventContext context)
        {
            base.OnAppEnd(context);

            IStartup startup = ClassFactory.Get<IStartup>();
            startup.AppEnd();
        }
    }
}
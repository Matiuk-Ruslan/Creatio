namespace RMQ.Api
{
    public interface IStartup
    {
        void AppStart(object userConnection);
        void AppEnd();
    }
}
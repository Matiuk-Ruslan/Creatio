using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth
{
    class Program
    {
        static void Main(string[] args)
        {
            Login login = new Login("https://eva.terrasoft.ua/", "Supervisor2", "Supervisor2new");
            login.TryLogin();
            login.GetWTLCallCdr();
            //Delay
            Console.ReadLine();
        }
    }
}

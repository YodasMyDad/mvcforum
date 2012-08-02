using Ninject;
using n3oWhiteSite.Domain.Interfaces.Services;
using n3oWhiteSite.IOC;

namespace n3oWhiteSite.Desktop
{
    class Program
    {
        static void Main(string[] args)
        {
            NinjectDT.Start();

            

            System.Console.Read();

            NinjectDT.Stop();
        }
    }
}

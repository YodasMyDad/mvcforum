using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCForum.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine(1550000.KiloFormat());

            

            Console.Read();
        }

        
    }

    public static class testing
    {
        public static string KiloFormat(this int num)
        {
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.#") + "M";

            if (num >= 10000)
                return (num / 1000D).ToString("#,0K");

            if (num >= 1000)
                return (num / 1000D).ToString("0.#") + "K";

            return num.ToString(CultureInfo.InvariantCulture);

        } 
    }
}

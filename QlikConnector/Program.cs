using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QlikConnector
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            if (args != null && args.Length >= 2)
            {
                new QlikConnectorServer().Run(args[0], args[1]);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocationClientGUI
{
    class ConsoleOut
    {
        public static MainWindow form = null;

        public static void messageout(string message, bool debugBoolean)
        {
            if (!debugBoolean)
            {
                return;
            }
             
            if(form == null)
            {
                Console.WriteLine(message);
            }
            else
            {
                form.print(message);
            }
        }
    }
}

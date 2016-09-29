using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentDemo {
    class Program {
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            //MySql.Program.Retrive_via_primaryKey_medium_scale();
            MySql.Program.Save_entity_new_and_exists();
            
            MySql.Program.Insert_with_dapper(1000);
            MySql.Program.Insert_with_nhibernate(1000);

            //Mongo.Program.Retrive_via_primaryKey_medium_scale();
            //Mongo.Program.Save_entity_new_and_exists();

            if (Debugger.IsAttached) {
                Console.WriteLine("Press <ENTER> to exit");
                Console.ReadLine();
            }
        }
    }
}

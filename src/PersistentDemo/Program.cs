using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chuye.Persistent.NHibernate;
using PersistentDemo.MySql;

namespace PersistentDemo {
    class Program {
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            //MysqlProgram.Retrive_via_primaryKey_medium_scale();
            MysqlProgram.Save_entity_new_and_exists();
            //MongoProgram.Save_entity();
        }
    }
}

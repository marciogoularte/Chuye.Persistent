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
            Retrive_via_primaryKey_medium_scale();
        }
            
        static void Retrive_via_primaryKey_medium_scale() {
            using (var context = new PubsContext()) {
                var repo = new NHibernateRepository<Roysched>(context);
                var allKeys = repo.All.Select(x => x.Title_id).ToArray();
                Console.WriteLine("Keys.Length={0}", allKeys.Length);
                var allItems = repo.Retrive(x => x.Title_id, allKeys).ToArray();
                Console.WriteLine("Items.Length={0}", allItems.Length);
                allItems = repo.All.Where(r => allKeys.Contains(r.Title_id)).ToArray();
                Console.WriteLine("Items.Length={0}", allItems.Length);
            }
        }
    }
}

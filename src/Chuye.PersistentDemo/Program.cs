using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chuye.Persistent.NHibernate;

namespace Chuye.PersistentDemo {
    class Program {
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            using (var context = new PubsContext()) {
                var repo = new NHibernateRepository<Job>(context);
                foreach (var item in repo.All) {
                    Console.WriteLine("{0} {1} {2} {3}",
                        item.Job_id, item.Job_desc, item.Min_lvl, item.Max_lvl);
                }

                var theFirstOne = repo.Retrive((Int16)1);
                var theSpecials = repo.Retrive("Max_lvl", (Byte)10);
                theSpecials = repo.Retrive(j => j.Max_lvl, (Byte)10);

                var query = repo.All.Where(r => r.Max_lvl == 100);
                theSpecials = repo.Fetch(_ => query);
                foreach (var item in theSpecials) {
                    Console.WriteLine("{0} {1} {2} {3}",
                        item.Job_id, item.Job_desc, item.Min_lvl, item.Max_lvl);
                }
            }
        }
    }
}

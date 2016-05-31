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
            
            Retrive_via_primaryKey_medium_scale();
        }

        static void NewMethod() {
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

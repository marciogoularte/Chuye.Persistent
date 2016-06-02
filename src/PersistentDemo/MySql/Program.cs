using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chuye.Persistent.NHibernate;

namespace PersistentDemo.MySql {
    class Program {
        internal static void Retrive_via_primaryKey_medium_scale() {
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

        internal static void Save_entity_new_and_exists() {
            var theOne = new Job {
                Job_id = (short)Guid.NewGuid().GetHashCode(),
                Job_desc = Guid.NewGuid().ToString(),
            };

            using (var context = new PubsContext()) {
                var repo = new NHibernateRepository<Job>(context);
                repo.Save(theOne);

                theOne.Min_lvl++;
                repo.Update(theOne);
                //context.Flush();
            }

            using (var context = new PubsContext()) {
                var repo = new NHibernateRepository<Job>(context);
                theOne.Max_lvl--;
                repo.Save(theOne);
                //context.Flush();
            }
        }
    }
}

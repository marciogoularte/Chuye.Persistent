using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chuye.Persistent.Mongo;
using PersistentDemo.Mongo;

namespace PersistentDemo {
    class MongoProgram {
        internal static void Save_entity() {
            using (var context = new PubsContext()) {
                var repo = new MongoRepository<Roysched, String>(context);
                var theFirstOne = new Roysched {
                    Id = null,
                    Hirange = Guid.NewGuid().GetHashCode(),
                    Lorange = Guid.NewGuid().GetHashCode(),
                    Royalty = Guid.NewGuid().GetHashCode(),
                };
                repo.Save(theFirstOne);

                var theSecendOne = new Roysched {
                    Id = Guid.NewGuid().ToString("n"),
                    Hirange = 1,
                    Lorange = 10,
                    Royalty = Guid.NewGuid().GetHashCode(),
                };
                repo.Save(theSecendOne);

                theSecendOne.Hirange++;
                repo.Update(theSecendOne);

                theSecendOne.Lorange++;
                repo.Save(theSecendOne);
            }
        }
    }
}

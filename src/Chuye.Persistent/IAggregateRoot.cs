using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent {
    public interface IAggregateRoot {
        TEntry RetriveByKey<TEntry>(Object key);
        void Save<TEntry>(TEntry entry);
        void Delete<TEntry>(TEntry entry);
    }
}

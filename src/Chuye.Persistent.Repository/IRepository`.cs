using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.Repository {
    public interface IRepository<TEntry> : IRepository<TEntry, TEntry> {

    }
}

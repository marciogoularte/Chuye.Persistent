using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentDemo.Model {
    class Owner {
        public virtual Int32 Id { get; set; }
        //public virtual IList<RootItem> Items { get; set; }
        public virtual FooItem FooItem { get; set; }
        public virtual BarItem BarItem { get; set; }
    }

    class RootItem {
        public virtual Int32 Id { get; set; }
        public virtual Owner Owner { get; set; }
    }

    class FooItem : RootItem {
        public virtual String Title { get; set; }
    }

    class BarItem : RootItem {
        public virtual String Remark { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentDemo.Model {
    class Node {
        public virtual int Id { get; set; }
        public virtual String Name { get; set; }
        public virtual Node Parent { get; set; }
    }
}

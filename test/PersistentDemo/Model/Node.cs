using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace PersistentDemo.Models {
    //class NodeMap : ClassMap<Node> {
    //    public NodeMap() {
    //        Id(x => x.Id);
    //        Map(x => x.Name);
    //        References(x => x.Parent, "ParentId")
    //            .NotFound.Ignore();
    //    }
    //}

    class Node {
        public virtual int Id { get; set; }
        public virtual String Name { get; set; }
        public virtual Node Parent { get; set; }
    }
}

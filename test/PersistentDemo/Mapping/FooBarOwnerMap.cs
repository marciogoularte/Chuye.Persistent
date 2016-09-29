using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using PersistentDemo.Model;

namespace PersistentDemo.Mapping {
    class RootItemMap : ClassMap<RootItem> {
        public RootItemMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
        }
    }

    class FooMap : ClassMap<FooItem> {
        public FooMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            References(x => x.Owner, "OwnerId")
                .NotFound.Ignore();
        }
    }

    class BarMap : ClassMap<BarItem> {
        public BarMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Remark);
            References(x => x.Owner, "OwnerId")
                .NotFound.Ignore();
        }
    }

    class OwnerMap : ClassMap<Owner> {
        public OwnerMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            References(x => x.BarItem, "BarId")
                .Cascade.All()
                .NotFound.Ignore();
            References(x => x.FooItem, "FooId")
                .Cascade.All()
                .NotFound.Ignore();
        }
    }
}

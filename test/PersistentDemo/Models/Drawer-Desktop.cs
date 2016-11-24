using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace PersistentDemo.Models {
    class Drawer {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
        public virtual Desktop Desktop { get; set; }
    }

    class Desktop {
        public virtual Int32 Id { get; set; }
        public virtual String Title { get; set; }
        public virtual Drawer Drawer { get; set; }
    }

    class DrawerMap : ClassMap<Drawer> {
        public DrawerMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Name);
            References(x => x.Desktop, "DesktopId")
                .Unique()
                .NotFound.Ignore();
        }
    }

    class DesktopMap : ClassMap<Desktop> {
        public DesktopMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            HasOne(x => x.Drawer).PropertyRef(x => x.Desktop)
                .Fetch.Select()
                .Cascade.All();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace PersistentDemo.Models {
    class Desktop  {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
    }

    class Drawer : Desktop {
        public virtual String Title { get; set; }
    }

    class DesktopMap : ClassMap <Desktop> {
        public DesktopMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Name);
        }
    }

    class DrawerMap : SubclassMap<Drawer> {
        public DrawerMap() {
            KeyColumn("Id");
            Map(x => x.Title);
        }
    }
}

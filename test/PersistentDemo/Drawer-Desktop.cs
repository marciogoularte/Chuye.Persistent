using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace PersistentDemo.Models {
    public class Drawer {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
        public virtual Desktop Desktop { get; set; }
    }

    public class Desktop {
        public virtual Int32 Id { get; set; }
        public virtual String Title { get; set; }
        public virtual IList<Drawer> Drawers { get; set; }

        public Desktop() {
            Drawers = new List<Drawer>();
        }
    }

    class DrawerMap : ClassMap<Drawer> {
        public DrawerMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Name);
            References(x => x.Desktop, "DesktopId")
                .LazyLoad(Laziness.NoProxy)
                .NotFound.Ignore();
        }
    }

    class DesktopMap : ClassMap<Desktop> {
        public DesktopMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
            HasMany(x => x.Drawers).KeyColumn("DesktopId")
                .Cascade.All();
        }
    }
}

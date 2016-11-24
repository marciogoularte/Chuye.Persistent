using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace PersistentDemo.Models {
    class Drawer {
        public virtual Desktop Desktop { get; set; }
        public virtual String Name { get; set; }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var child = obj as Drawer;

            if (child != null && child.Desktop != null) {
                return child.Desktop.Id == Desktop.Id;
            }

            return false;
        }

        public override int GetHashCode() {
            return Desktop.Id;
        }
    }

    class Desktop {
        public virtual Int32 Id { get; set; }
        public virtual String Title { get; set; }
    }

    class DrawerMap : ClassMap<Drawer> {
        public DrawerMap() {
            CompositeId().KeyReference(x => x.Desktop, "Id");
            Map(x => x.Name);
        }
    }

    class DesktopMap : ClassMap<Desktop> {
        public DesktopMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Title);
        }
    }
}

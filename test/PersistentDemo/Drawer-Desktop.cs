using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace PersistentDemo.Models {
    public class GeneralType : GeneralStatus {
        public static GeneralType Universal = new GeneralType(1, "Universal");
        public static GeneralType Classified = new GeneralType(3, "Classified");
        public static GeneralType Specific = new GeneralType(5, "Specific");

        protected GeneralType() {
        }

        protected GeneralType(Int32 symbol, String meaning)
            : base(symbol, meaning) {
        }
    }

    public class GeneralStatus : IEquatable<GeneralStatus> {
        public virtual Int32 Symbol { get; set; }
        public virtual String Meaning { get; set; }

        public GeneralStatus() {
        }

        public GeneralStatus(Int32 symbol, String meaning) {
            Symbol = symbol;
            Meaning = meaning;
        }

        public virtual Boolean Equals(GeneralStatus other) {
            if (other == null) {
                return false;
            }
            return Symbol == other.Symbol
                && Meaning.Equals(other.Meaning, StringComparison.Ordinal);
        }

        public override Boolean Equals(Object other) {
            var value = other as GeneralStatus;
            if (value == null) {
                return false;
            }
            if (other.GetType() != this.GetType()) {
                return false;
            }
            return Equals(value);
        }

        public override Int32 GetHashCode() {
            return 17 * Symbol ^ Meaning.GetHashCode();
        }

        public override String ToString() {
            return String.Format("{1}({0})", Symbol, Meaning);
        }

        public static Boolean operator ==(GeneralStatus v1, GeneralStatus v2) {
            //return (v1 == null && v2 == null) || (v1.Equals(v2));// == null 判断将形成递归, 
            return Object.Equals(v1, v2);
        }

        public static Boolean operator !=(GeneralStatus v1, GeneralStatus v2) {
            return !(Object.Equals(v1, v2));
        }
    }

    class GeneralStatusMap : ComponentMap<GeneralStatus> {
        public GeneralStatusMap() {
            Map(x => x.Symbol);
            Map(x => x.Meaning);
        }
    }

    public class Drawer {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
        public virtual Desktop Desktop { get; set; }
    }

    public class Desktop {
        public virtual Int32 Id { get; set; }
        public virtual String Title { get; set; }
        public virtual IList<Drawer> Drawers { get; set; }
        public virtual GeneralType Status { get; set; }

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
            Component(x => x.Status, c => {
                c.Map(x => x.Meaning);
                c.Map(x => x.Symbol);
            });
            HasMany(x => x.Drawers).KeyColumn("DesktopId")
                .Cascade.All();
        }
    }
}

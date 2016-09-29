using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using PersistentDemo.Model;

namespace PersistentDemo.Mapping {
    class UserMap : ClassMap<User> {
        public UserMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Name);
        }
    }

    class AccountMap : ClassMap<Account> {
        public AccountMap() {
            CompositeId().KeyReference(x => x.User, "Id");
            Map(x => x.Balance);
        }
    }

    class DrivingLicenseMap : ClassMap<DrivingLicense> {
        public DrivingLicenseMap() {
            Id(x => x.Id).GeneratedBy.Foreign("User");
            HasOne(x => x.User).Constrained().ForeignKey();
            Map(x => x.CreateAt);
        }
    }



    /*
    public class ParentMap : ClassMap<Parent> {
        public ParentMap() {
            //Table("StackOverflowExamples.dbo.Parent");

            Id(x => x.ParentId);
            Map(x => x.FirstName);
            Map(x => x.LastName);
        }
    }

    public class OnlyChildOfParentMap : ClassMap<OnlyChildOfParent> {
        public OnlyChildOfParentMap() {
            //Table("StackOverflowExamples.dbo.OnlyChildOfParent");

            CompositeId().KeyReference(x => x.Parent, "ParentId");
            Map(x => x.SomeStuff);
            Map(x => x.SomeOtherStuff);
        }
    }

    public class Parent {
        public virtual int ParentId { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
    }

    public class OnlyChildOfParent {
        public virtual Parent Parent { get; set; }
        public virtual string SomeStuff { get; set; }
        public virtual string SomeOtherStuff { get; set; }

        #region Overrides

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var child = obj as OnlyChildOfParent;

            if (child != null && child.Parent != null) {
                return child.Parent.ParentId == Parent.ParentId;
            }

            return false;
        }

        public override int GetHashCode() {
            return Parent.ParentId;
        }

        #endregion Overrides
    }
    */
}

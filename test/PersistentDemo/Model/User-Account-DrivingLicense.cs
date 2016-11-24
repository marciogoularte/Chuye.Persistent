using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace PersistentDemo.Models {
    //class UserMap : ClassMap<User> {
    //    public UserMap() {
    //        Id(x => x.Id).GeneratedBy.Assigned();
    //        Map(x => x.Name);
    //    }
    //}

    //class AccountMap : ClassMap<Account> {
    //    public AccountMap() {
    //        CompositeId().KeyReference(x => x.User, "Id");
    //        Map(x => x.Balance);
    //    }
    //}

    //class DrivingLicenseMap : ClassMap<DrivingLicense> {
    //    public DrivingLicenseMap() {
    //        Id(x => x.Id).GeneratedBy.Foreign("User");
    //        HasOne(x => x.User).Constrained().ForeignKey();
    //        Map(x => x.CreateAt);
    //    }
    //}

    class User {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
    }

    class Account {
        public virtual User User { get; set; }
        public virtual Int32 Balance { get; set; }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var child = obj as Account;

            if (child != null && child.User != null) {
                return child.User.Id == User.Id;
            }

            return false;
        }

        public override int GetHashCode() {
            return User.Id;
        }
    }

    class DrivingLicense {
        protected internal virtual Int32 Id { get; set; }
        public virtual User User { get; set; }
        public virtual DateTime CreateAt { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentDemo.Model {
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

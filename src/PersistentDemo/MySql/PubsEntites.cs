using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace PersistentDemo.MySql {
    public class Job {
        public virtual Int16 Job_id { get; set; }
        public virtual String Job_desc { get; set; }
        public virtual Byte Min_lvl { get; set; }
        public virtual Byte Max_lvl { get; set; }
    }

    public class Roysched {
        public virtual String Title_id { get; set; }
        public virtual Int32? Lorange { get; set; }
        public virtual Int32? Hirange { get; set; }
        public virtual Int32? Royalty { get; set; }
    }

    public class Person {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
        public virtual DateTime Birth { get; set; }
        public virtual String Address { get; set; }
        public virtual Int32 Job_id { get; set; }
    }
}

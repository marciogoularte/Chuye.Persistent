﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace Chuye.Persistent.Tests.MySql {
    public class Job {
        public virtual Int16 Job_id { get; set; }
        public virtual String Job_desc { get; set; }
        public virtual Byte Min_lvl { get; set; }
        public virtual Byte Max_lvl { get; set; }
    }

    public class Roysched {
        public virtual String Id { get; set; }
        public virtual Int32? Lorange { get; set; }
        public virtual Int32? Hirange { get; set; }
        public virtual Int32? Royalty { get; set; }
    }

    public class JobMap : ClassMap<Job> {
        public JobMap() {
            Table("jobs");
            Id(x => x.Job_id);
            Map(x => x.Job_desc).Not.Nullable().Length(255);
            Map(x => x.Min_lvl).Not.Nullable();
            Map(x => x.Max_lvl).Not.Nullable();
        }
    }

    public class RoyschedMap : ClassMap<Roysched> {
        public RoyschedMap() {
            Id(x => x.Id).Column("Title_id").GeneratedBy.Assigned();
            Map(x => x.Lorange);
            Map(x => x.Hirange);
            Map(x => x.Royalty);
        }
    }

}

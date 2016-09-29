using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using PersistentDemo.Model;

namespace PersistentDemo.Mapping {
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
            Id(x => x.Title_id).Column("Title_id").GeneratedBy.Assigned();
            Map(x => x.Lorange);
            Map(x => x.Hirange);
            Map(x => x.Royalty);
        }
    }

    public class PersonMap : ClassMap<Person> {
        public PersonMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Name);
            Map(x => x.Birth);
            Map(x => x.Address);
            Map(x => x.Job_id);
        }
    }
}

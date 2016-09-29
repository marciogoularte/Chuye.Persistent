﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using PersistentDemo.Model;

namespace PersistentDemo.Mapping {
    class ParentMap : ClassMap<Node> {
        public ParentMap() {
            Id(x => x.Id);
            Map(x => x.Name);
            References(x => x.Parent, "ParentId")
                .NotFound.Ignore();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.NHibernate {
    public interface INHibernateRepository<TEntry> : INHibernateRepository<TEntry, TEntry> {
    }
}

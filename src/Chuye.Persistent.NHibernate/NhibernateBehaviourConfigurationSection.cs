using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Chuye.Persistent.NHibernate {
    public class NhibernateBehaviourConfigurationSection : ConfigurationSection {
        private const String alwaysCommit = "alwaysCommit";

        [ConfigurationProperty(alwaysCommit, IsRequired = false)]
        public Boolean AlwaysCommit {
            get { return (Boolean)this[alwaysCommit]; }
            set { this[alwaysCommit] = value; }
        }
    }
}

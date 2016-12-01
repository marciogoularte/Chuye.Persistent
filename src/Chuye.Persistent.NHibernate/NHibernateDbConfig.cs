using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.NHibernate {
    public struct NHibernateDbConfig {
        public TransactionStragety Stragety;
        public Boolean SaveUncommitted;

        public static readonly NHibernateDbConfig Default;

        static NHibernateDbConfig() {
            Default = new NHibernateDbConfig {
                Stragety = new TransactionStragety {
                    Require = TransactionRequire.Manual,
                    Time = TransactionTime.Lazy,
                },
                SaveUncommitted = false
            };
        }

        public static NHibernateDbConfig FromConfig(String key) {
            //Manual,SessionStarted,SaveUncommitted
            var config = ConfigurationManager.AppSettings.Get(key);
            if (config == null) {
                return Default;
            }

            var values = config.Split(',');
            if (values.Length < 2 || values.Length > 3) {
                throw new ArgumentOutOfRangeException("config");
            }

            var require = Default.Stragety.Require;
            Enum.TryParse(values[0], true, out require);

            var time = Default.Stragety.Time;
            Enum.TryParse(values[1], true, out time);

            var saveUncommitted = values.Length > 2
                && values[2].Equals("SaveUncommitted", StringComparison.OrdinalIgnoreCase);
            return new NHibernateDbConfig {
                Stragety = new TransactionStragety {
                    Require = require,
                    Time = time,
                },
                SaveUncommitted = saveUncommitted
            };
        }
    }

    public struct TransactionStragety {
        public TransactionRequire Require;
        public TransactionTime Time;
    }

    public enum TransactionRequire {
        Manual, Essential
    }

    public enum TransactionTime {
        Immediately, Lazy
    }
}

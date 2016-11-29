using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.NHibernate {
    public struct NHibernateDbConfig {
        public TransactionDemand TransactionDemand;
        public TransactionTime TransactionTime;
        public ModificationStragety ModificationStragety;

        public static readonly NHibernateDbConfig Default;

        static NHibernateDbConfig() {
            Default = new NHibernateDbConfig {
                TransactionDemand = TransactionDemand.Manual,
                TransactionTime = TransactionTime.SessionStarted,
                ModificationStragety = ModificationStragety.Discard,
            };
        }

        public static NHibernateDbConfig FromConfig(String key) {
            var config = ConfigurationManager.AppSettings.Get(key);
            if (config == null) {
                return Default;
            }

            var pairs = config.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries));

            var demand = Default.TransactionDemand;
            var modelPair = pairs.SingleOrDefault(x => x[0] == "model");
            if (modelPair != null) {
                if (!Enum.TryParse(modelPair[1], out demand)) {
                    throw new ArgumentOutOfRangeException("model");
                }
            }

            var time = Default.TransactionTime;
            var timePair = pairs.SingleOrDefault(x => x[0] == "time");
            if (timePair != null) {
                if (!Enum.TryParse(timePair[1], out time)) {
                    throw new ArgumentOutOfRangeException("time");
                }
            }

            var stragety = Default.ModificationStragety;
            var stragetyPair = pairs.SingleOrDefault(x => x[0] == "stragety");
            if (stragetyPair != null) {
                if (!Enum.TryParse(stragetyPair[1], out stragety)) {
                    throw new ArgumentOutOfRangeException("stragety");
                }
            }

            return new NHibernateDbConfig {
                TransactionDemand = demand,
                TransactionTime = time,
                ModificationStragety = stragety,
            };
        }
    }

    public enum TransactionDemand {
        Manual, Essential
    }

    public enum TransactionTime {
        SessionStarted, Immediately
    }

    public enum ModificationStragety {
        Discard, Submit
    }
}

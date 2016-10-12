using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaPoco;

namespace Chuye.Persistent.PetaPoco {
    public class PetaPocoDbContext {
        private String _connectionStringName;
        private String _connectionString;
        private String _providerName;

        public PetaPocoDbContext(String connectionStringName) {
            if (String.IsNullOrWhiteSpace(connectionStringName)) {
                throw new ArgumentOutOfRangeException("connectionStringName");
            }
            _connectionStringName = connectionStringName;
        }

        public PetaPocoDbContext(String connectionString, String providerName) {
            if (String.IsNullOrWhiteSpace(connectionString)) {
                throw new ArgumentOutOfRangeException("connectionString");
            }
            _connectionString = connectionString;
            _providerName = providerName;
        }

        public Database OpenDatabase() {
            if (_connectionStringName != null) {
                return new Database(_connectionStringName);
            }
            else {
                return new Database(_connectionString, _providerName);
            }
        }
    }
}

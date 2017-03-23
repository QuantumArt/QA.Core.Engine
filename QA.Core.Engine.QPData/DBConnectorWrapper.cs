using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quantumart.QPublishing.Database;

namespace QA.Core.Engine.QPData
{
    internal class DBConnectorWrapper : IDBConnector
    {
        private DBConnector _connector;
        #region IDBConnector Members
        public DBConnectorWrapper(DBConnector connector)
        {
            Throws.IfArgumentNull(connector, _ => connector);
            _connector = connector;
        }
        public string GetUrlForFileAttribute(int fieldId, bool asShortAsPossible, bool removeSchema)
        {
            return _connector.GetUrlForFileAttribute(fieldId, asShortAsPossible, removeSchema);
        }

        #endregion
    }
}

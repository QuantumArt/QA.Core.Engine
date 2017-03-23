using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Engine.QPData
{
    public interface IDBConnector
    {
        string GetUrlForFileAttribute(int fieldId, bool asShortAsPossible, bool removeSchema);
    }
}

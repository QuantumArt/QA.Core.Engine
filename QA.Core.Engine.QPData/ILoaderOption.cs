using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QPublishing.Database;

namespace QA.Core.Engine.QPData
{
    public interface ILoaderOption
    {
        Type Type { get; }
        string PropertyName { get; }
        void AttachTo(Type type, string propertyName);
        string Process(DBConnector cnn, QPContext ctx,  string value);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QA.Core.Data.Repository;

namespace QA.Core.Engine.QPData.Repository
{
    public class AbstractItemRepository : L2SqlRepositoryBase<QPAbstractItem, int>
    {
        public AbstractItemRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}

using QA.Core.Data.Repository;
#pragma warning disable 1591

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

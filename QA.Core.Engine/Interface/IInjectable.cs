
namespace QA.Core.Engine
{
    public interface IInjectable<T>
    {
        void Set(T dependency);
    }
}

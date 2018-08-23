#pragma warning disable 1591
namespace QA.Core.Engine
{
    public interface IInjectable<T>
    {
        void Set(T dependency);
    }
}

namespace ArdoqFluentModels.Search
{
    public interface IUnbuiltSearchSpecElement
    {
        ISearchSpecElement Build(object obj);
    }
}

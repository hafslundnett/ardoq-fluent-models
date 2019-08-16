namespace ModelMaintainer.Search
{
    public interface IUnbuiltSearchSpecElement
    {
        ISearchSpecElement Build(object obj);
    }
}

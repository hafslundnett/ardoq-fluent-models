namespace ModelMaintainer.Search
{
    public interface ISearchBuilder
    {
        SearchSpec BuildSearch(object obj);
    }
}

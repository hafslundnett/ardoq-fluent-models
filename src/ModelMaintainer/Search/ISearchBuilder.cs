namespace ArdoqFluentModels.Search
{
    public interface ISearchBuilder
    {
        SearchSpec BuildSearch(object obj);
    }
}

using ModelMaintainer.Samples.Azure.Model;

namespace ModelMaintainer.Samples.Azure
{
    public interface IAzureReader
    {
        Subscription ReadSubscription();
    }
}
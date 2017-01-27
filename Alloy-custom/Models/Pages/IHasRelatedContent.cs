using EPiServer.Core;

namespace Alloy_custom.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}

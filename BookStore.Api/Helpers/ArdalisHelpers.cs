using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BookStore.Helpers;

/// <summary>
/// Ardalis ApiEndpoints binds a single request object. When an endpoint needs to
/// combine values from multiple binding sources (e.g. a route id and a query value)
/// into one request record, decorate the request parameter with [FromMultiSource].
/// </summary>
public sealed class FromMultiSourceAttribute : Attribute, IBindingSourceMetadata
{
    public BindingSource BindingSource { get; } = CompositeBindingSource.Create(
        [BindingSource.Path, BindingSource.Query],
        nameof(FromMultiSourceAttribute));
}

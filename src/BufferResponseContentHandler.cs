namespace AutoIoc;

/// <summary>
///     Help preserve the original request message and buffer the response content, which is a known compatability issue with Polly.
/// </summary>
public class BufferResponseContentHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        
        // Preserve the original request message
        response.RequestMessage = request;

        // Buffer the content into memory so it survives Polly disposing the response
        await response.Content.LoadIntoBufferAsync();

        return response;
    }
}
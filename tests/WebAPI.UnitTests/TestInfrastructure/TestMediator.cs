#nullable enable
using Mediator;

namespace WebAPI.UnitTests.TestInfrastructure;

/// <summary>
/// A mock implementation of <see cref="IMediator"/> for unit testing purposes.
/// </summary>
public sealed class TestMediator : IMediator
{
    private readonly Func<object, Task<object>> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestMediator"/> class.
    /// </summary>
    /// <param name="handler">A function that handles requests and returns a task of the result.</param>
    public TestMediator(Func<object, Task<object>> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Gets the last request that was sent through the mediator.
    /// </summary>
    public object? LastRequest { get; private set; }

    /// <summary>
    /// Sends a request to the mediator and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        var result = await _handler(request);
        return (TResponse)result;
    }

    /// <summary>
    /// Publishes a notification. This is a no-op in the test implementation.
    /// </summary>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task Publish(INotification notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

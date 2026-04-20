using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Mediator;

namespace Application.Reminders.Commands.DeleteReminder;

/// <summary>
/// Command to delete a reminder by its identifier.
/// </summary>
public class DeleteReminderCommand : IRequest<BaseResponse<string>>
{
    /// <summary>
    /// Gets or sets the identifier of the reminder to delete.
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// Handles the deletion of a reminder by its identifier.
/// </summary>
public class DeleteReminderCommandHandler : IRequestHandler<DeleteReminderCommand, BaseResponse<string>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteReminderCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public DeleteReminderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the deletion of a reminder.
    /// </summary>
    /// <param name="request">The command containing the reminder identifier to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response indicating the result of the delete operation.</returns>
    public async Task<BaseResponse<string>> Handle(DeleteReminderCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Reminders.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Reminder with id {request.Id} was not found.");
        }

        _context.Reminders.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<string>.Ok($"Reminder with id {request.Id} deleted successfully.");
    }
}

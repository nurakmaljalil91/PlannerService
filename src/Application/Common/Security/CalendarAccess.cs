#nullable enable
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Security;

/// <summary>
/// Ownership rules for planner data. A calendar is readable by its owner and, when public, by any user;
/// it is writable only by its owner. Events, planner tasks and reminders inherit access from their calendar.
/// </summary>
public static class CalendarAccess
{
    /// <summary>
    /// Filters calendars to those the specified user may read: calendars they own and all public calendars.
    /// </summary>
    /// <param name="calendars">The calendars to filter.</param>
    /// <param name="userId">The current user's identifier, or <c>null</c> when unknown.</param>
    /// <returns>The readable calendars.</returns>
    public static IQueryable<Calendar> ReadableBy(this IQueryable<Calendar> calendars, Guid? userId)
    {
        if (userId is not { } id)
        {
            return calendars.Where(c => c.IsPublic);
        }

        return calendars.Where(c => c.UserId == id || c.IsPublic);
    }

    /// <summary>
    /// Filters calendars to those owned by the specified user. An unknown user owns nothing.
    /// </summary>
    /// <param name="calendars">The calendars to filter.</param>
    /// <param name="userId">The current user's identifier, or <c>null</c> when unknown.</param>
    /// <returns>The owned calendars.</returns>
    public static IQueryable<Calendar> OwnedBy(this IQueryable<Calendar> calendars, Guid? userId)
    {
        if (userId is not { } id)
        {
            return calendars.Where(c => false);
        }

        return calendars.Where(c => c.UserId == id);
    }

    /// <summary>
    /// Gets the identifiers of the calendars a list endpoint should draw from. With a calendar filter this is that
    /// calendar if it is readable; without one it is the readable calendars the user owns or is subscribed to.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="userId">The current user's identifier, or <c>null</c> when unknown.</param>
    /// <param name="calendarId">The optional calendar filter.</param>
    /// <returns>A sub-query of calendar identifiers.</returns>
    public static IQueryable<long> ListedCalendarIds(this IApplicationDbContext context, Guid? userId, long? calendarId)
    {
        var readable = context.Calendars.ReadableBy(userId);

        if (calendarId is { } filterId)
        {
            return readable.Where(c => c.Id == filterId).Select(c => c.Id);
        }

        if (userId is not { } id)
        {
            return readable.Where(c => false).Select(c => c.Id);
        }

        var subscribedIds = context.CalendarSubscriptions
            .Where(s => s.UserId == id)
            .Select(s => s.CalendarId);

        return readable
            .Where(c => c.UserId == id || subscribedIds.Contains(c.Id))
            .Select(c => c.Id);
    }

    /// <summary>
    /// Ensures the specified calendar exists and is readable by the user.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="calendarId">The calendar identifier.</param>
    /// <param name="userId">The current user's identifier, or <c>null</c> when unknown.</param>
    /// <param name="notFoundMessage">The message used when the calendar is missing or not readable.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="NotFoundException">The calendar is missing or not readable.</exception>
    public static async Task EnsureCalendarReadableAsync(
        this IApplicationDbContext context,
        long calendarId,
        Guid? userId,
        string notFoundMessage,
        CancellationToken cancellationToken)
    {
        var readable = await context.Calendars
            .ReadableBy(userId)
            .AnyAsync(c => c.Id == calendarId, cancellationToken);

        if (!readable)
        {
            throw new NotFoundException(notFoundMessage);
        }
    }

    /// <summary>
    /// Ensures the specified calendar exists and is owned by the user.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="calendarId">The calendar identifier.</param>
    /// <param name="userId">The current user's identifier, or <c>null</c> when unknown.</param>
    /// <param name="notFoundMessage">The message used when the calendar is missing or not readable.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="NotFoundException">The calendar is missing or not readable.</exception>
    /// <exception cref="ForbiddenAccessException">The calendar is readable (public) but not owned by the user.</exception>
    public static async Task EnsureCalendarWritableAsync(
        this IApplicationDbContext context,
        long calendarId,
        Guid? userId,
        string notFoundMessage,
        CancellationToken cancellationToken)
    {
        var calendar = await context.Calendars
            .ReadableBy(userId)
            .Where(c => c.Id == calendarId)
            .Select(c => new { c.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (calendar is null)
        {
            throw new NotFoundException(notFoundMessage);
        }

        if (calendar.UserId != userId)
        {
            throw new ForbiddenAccessException();
        }
    }

    /// <summary>
    /// Ensures the specified event exists and its calendar is readable by the user.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="userId">The current user's identifier, or <c>null</c> when unknown.</param>
    /// <param name="notFoundMessage">The message used when the event is missing or not readable.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="NotFoundException">The event is missing or not readable.</exception>
    public static async Task EnsureEventReadableAsync(
        this IApplicationDbContext context,
        long eventId,
        Guid? userId,
        string notFoundMessage,
        CancellationToken cancellationToken)
    {
        var calendarId = await GetEventCalendarIdAsync(context, eventId, notFoundMessage, cancellationToken);
        await context.EnsureCalendarReadableAsync(calendarId, userId, notFoundMessage, cancellationToken);
    }

    /// <summary>
    /// Ensures the specified event exists and its calendar is owned by the user.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="userId">The current user's identifier, or <c>null</c> when unknown.</param>
    /// <param name="notFoundMessage">The message used when the event is missing or not readable.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="NotFoundException">The event is missing or not readable.</exception>
    /// <exception cref="ForbiddenAccessException">The event is readable (public calendar) but not owned by the user.</exception>
    public static async Task EnsureEventWritableAsync(
        this IApplicationDbContext context,
        long eventId,
        Guid? userId,
        string notFoundMessage,
        CancellationToken cancellationToken)
    {
        var calendarId = await GetEventCalendarIdAsync(context, eventId, notFoundMessage, cancellationToken);
        await context.EnsureCalendarWritableAsync(calendarId, userId, notFoundMessage, cancellationToken);
    }

    private static async Task<long> GetEventCalendarIdAsync(
        IApplicationDbContext context,
        long eventId,
        string notFoundMessage,
        CancellationToken cancellationToken)
    {
        var calendarId = await context.Events
            .Where(e => e.Id == eventId)
            .Select(e => (long?)e.CalendarId)
            .FirstOrDefaultAsync(cancellationToken);

        return calendarId ?? throw new NotFoundException(notFoundMessage);
    }
}

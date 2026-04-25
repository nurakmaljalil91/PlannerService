using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Application.Calendars.Commands.CreateCalendar;
using Application.Calendars.Commands.CreatePublicCalendar;
using Application.Calendars.Commands.DeleteCalendar;
using Application.Calendars.Commands.SubscribeToCalendar;
using Application.Calendars.Commands.ToggleCalendarVisibility;
using Application.Calendars.Commands.ToggleSubscriptionVisibility;
using Application.Calendars.Commands.UnsubscribeFromCalendar;
using Application.Calendars.Commands.UpdateCalendar;
using Application.Calendars.Dtos;
using Application.Calendars.Queries.GetCalendarById;
using Application.Calendars.Queries.GetCalendars;
using Application.Calendars.Queries.GetPublicCalendars;
using Application.Calendars.Queries.GetUserSubscriptions;
using Application.Common.Behaviours;
using Application.Common.Models;
using Application.Events.Commands.CreateEvent;
using Application.Events.Commands.DeleteEvent;
using Application.Events.Commands.UpdateEvent;
using Application.Events.Dtos;
using Application.Events.Queries.GetEventById;
using Application.Events.Queries.GetEvents;
using Application.PlannerTasks.Commands.CompletePlannerTask;
using Application.PlannerTasks.Commands.CreatePlannerTask;
using Application.PlannerTasks.Commands.DeletePlannerTask;
using Application.PlannerTasks.Commands.UpdatePlannerTask;
using Application.PlannerTasks.Dtos;
using Application.PlannerTasks.Queries.GetPlannerTasks;
using Application.Reminders.Commands.CreateReminder;
using Application.Reminders.Commands.DeleteReminder;
using Application.Reminders.Commands.UpdateReminder;
using Application.Reminders.Dtos;
using Application.Reminders.Queries.GetReminderById;
using Application.Reminders.Queries.GetReminders;
using Domain.Common;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Provides extension methods for registering application services in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application-specific services to the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddScoped<IMediator, Mediator.Mediator>();

        // Calendar handlers
        services.AddScoped<IRequestHandler<GetCalendarsQuery, BaseResponse<PaginatedEnumerable<CalendarDto>>>,
            GetCalendarsQueryHandler>();
        services.AddScoped<IRequestHandler<GetCalendarByIdQuery, BaseResponse<CalendarDto>>, GetCalendarByIdQueryHandler>();
        services.AddScoped<IRequestHandler<CreateCalendarCommand, BaseResponse<CalendarDto>>, CreateCalendarCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateCalendarCommand, BaseResponse<CalendarDto>>, UpdateCalendarCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteCalendarCommand, BaseResponse<string>>, DeleteCalendarCommandHandler>();
        services.AddScoped<IRequestHandler<GetPublicCalendarsQuery, BaseResponse<PaginatedEnumerable<CalendarDto>>>,
            GetPublicCalendarsQueryHandler>();
        services.AddScoped<IRequestHandler<CreatePublicCalendarCommand, BaseResponse<CalendarDto>>, CreatePublicCalendarCommandHandler>();
        services.AddScoped<IRequestHandler<SubscribeToCalendarCommand, BaseResponse<CalendarSubscriptionDto>>, SubscribeToCalendarCommandHandler>();
        services.AddScoped<IRequestHandler<UnsubscribeFromCalendarCommand, BaseResponse<string>>, UnsubscribeFromCalendarCommandHandler>();
        services.AddScoped<IRequestHandler<GetUserSubscriptionsQuery, BaseResponse<IEnumerable<CalendarSubscriptionDto>>>, GetUserSubscriptionsQueryHandler>();
        services.AddScoped<IRequestHandler<ToggleCalendarVisibilityCommand, BaseResponse<CalendarDto>>, ToggleCalendarVisibilityCommandHandler>();
        services.AddScoped<IRequestHandler<ToggleSubscriptionVisibilityCommand, BaseResponse<CalendarSubscriptionDto>>, ToggleSubscriptionVisibilityCommandHandler>();

        // Event handlers
        services.AddScoped<IRequestHandler<GetEventsQuery, BaseResponse<PaginatedEnumerable<EventDto>>>,
            GetEventsQueryHandler>();
        services.AddScoped<IRequestHandler<GetEventByIdQuery, BaseResponse<EventDto>>, GetEventByIdQueryHandler>();
        services.AddScoped<IRequestHandler<CreateEventCommand, BaseResponse<EventDto>>, CreateEventCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateEventCommand, BaseResponse<EventDto>>, UpdateEventCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteEventCommand, BaseResponse<string>>, DeleteEventCommandHandler>();

        // PlannerTask handlers
        services.AddScoped<IRequestHandler<GetPlannerTasksQuery, BaseResponse<PaginatedEnumerable<PlannerTaskDto>>>,
            GetPlannerTasksQueryHandler>();
        services.AddScoped<IRequestHandler<CreatePlannerTaskCommand, BaseResponse<PlannerTaskDto>>, CreatePlannerTaskCommandHandler>();
        services.AddScoped<IRequestHandler<UpdatePlannerTaskCommand, BaseResponse<PlannerTaskDto>>, UpdatePlannerTaskCommandHandler>();
        services.AddScoped<IRequestHandler<DeletePlannerTaskCommand, BaseResponse<string>>, DeletePlannerTaskCommandHandler>();
        services.AddScoped<IRequestHandler<CompletePlannerTaskCommand, BaseResponse<PlannerTaskDto>>, CompletePlannerTaskCommandHandler>();

        // Reminder handlers
        services.AddScoped<IRequestHandler<GetRemindersQuery, BaseResponse<PaginatedEnumerable<ReminderDto>>>,
            GetRemindersQueryHandler>();
        services.AddScoped<IRequestHandler<GetReminderByIdQuery, BaseResponse<ReminderDto>>, GetReminderByIdQueryHandler>();
        services.AddScoped<IRequestHandler<CreateReminderCommand, BaseResponse<ReminderDto>>, CreateReminderCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateReminderCommand, BaseResponse<ReminderDto>>, UpdateReminderCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteReminderCommand, BaseResponse<string>>, DeleteReminderCommandHandler>();

        return services;
    }
}

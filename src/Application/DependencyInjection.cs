using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Application.Calendars.Commands.CreateCalendar;
using Application.Calendars.Commands.DeleteCalendar;
using Application.Calendars.Commands.UpdateCalendar;
using Application.Calendars.Dtos;
using Application.Calendars.Queries.GetCalendarById;
using Application.Calendars.Queries.GetCalendars;
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
using Application.TodoItems.Commands;
using Application.TodoItems.Models;
using Application.TodoItems.Queries;
using Application.TodoLists.Commands;
using Application.TodoLists.Models;
using Application.TodoLists.Queries;
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

        // TodoItem handlers
        services.AddScoped<IRequestHandler<GetTodoItemsQuery, BaseResponse<PaginatedEnumerable<TodoItemDto>>>,
            GetTodoItemsQueryHandler>();
        services.AddScoped<IRequestHandler<CreateTodoItemCommand, BaseResponse<TodoItemDto>>, CreateTodoItemCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateTodoItemComand, BaseResponse<TodoItemDto>>, UpdateTodoItemCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteTodoItemCommand, BaseResponse<object>>, DeleteTodoItemCommandHandler>();

        // TodoList handlers
        services.AddScoped<IRequestHandler<GetTodoListsQuery, BaseResponse<PaginatedEnumerable<TodoListDto>>>,
            GetTodoListsQueryHandler>();
        services.AddScoped<IRequestHandler<CreateTodoListCommand, BaseResponse<TodoListDto>>, CreateTodoListCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateTodoListCommand, BaseResponse<TodoListDto>>, UpdateTodoListCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteTodoListCommand, BaseResponse<string>>, DeleteTodoListCommandHandler>();

        // Calendar handlers
        services.AddScoped<IRequestHandler<GetCalendarsQuery, BaseResponse<PaginatedEnumerable<CalendarDto>>>,
            GetCalendarsQueryHandler>();
        services.AddScoped<IRequestHandler<GetCalendarByIdQuery, BaseResponse<CalendarDto>>, GetCalendarByIdQueryHandler>();
        services.AddScoped<IRequestHandler<CreateCalendarCommand, BaseResponse<CalendarDto>>, CreateCalendarCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateCalendarCommand, BaseResponse<CalendarDto>>, UpdateCalendarCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteCalendarCommand, BaseResponse<string>>, DeleteCalendarCommandHandler>();

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

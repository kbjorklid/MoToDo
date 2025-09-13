using Base.Contracts;

namespace ToDoLists.Contracts;

public record ToDoAddedIntegrationEvent(DateTime OccurredOn, string ToDoListId, string ToDoId, string UserId) : IntegrationEvent(OccurredOn);

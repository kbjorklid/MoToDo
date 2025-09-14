using System.Collections.Concurrent;
using AiItemSuggestions.Application.Ports;
using AiItemSuggestions.Domain;
using Base.Domain.Result;
using Microsoft.Extensions.Logging;
using ToDoLists.Contracts;
using Wolverine;

namespace AiItemSuggestions.Application;

/// <summary>
/// Handles ToDoAddedIntegrationEvent to automatically generate AI suggestions when appropriate conditions are met.
/// </summary>
public static class ToDoAddedIntegrationEventHandler
{
    private const int SuggestionTriggerItemCount = 3;
    private static readonly ConcurrentDictionary<ToDoListId, SemaphoreSlim> _locks = new();
    public static async Task Handle(
        ToDoAddedIntegrationEvent integrationEvent,
        IToDoListDataPort toDoListDataPort,
        IItemSuggestionsPort itemSuggestionsPort,
        IToDoListSuggestionsRepository suggestionsRepository,
        IMessageBus messageBus,
        ILogger logger)
    {
        Result<ToDoListId> toDoListIdResult = ToDoListId.FromString(integrationEvent.ToDoListId);
        if (toDoListIdResult.IsFailure)
            return;
        ToDoListId toDoListId = toDoListIdResult.Value;
        logger.LogDebug("Processing ToDoAddedIntegrationEvent for ToDo {ToDoId} in ToDoList {ToDoListId}",
            integrationEvent.ToDoId, integrationEvent.ToDoListId);
        await HandleInternal(integrationEvent, toDoListDataPort, itemSuggestionsPort, suggestionsRepository, messageBus, toDoListId, logger);
    }

    private static async Task HandleInternal(ToDoAddedIntegrationEvent integrationEvent, IToDoListDataPort toDoListDataPort,
        IItemSuggestionsPort itemSuggestionsPort, IToDoListSuggestionsRepository suggestionsRepository,
        IMessageBus messageBus, ToDoListId toDoListId, ILogger logger)
    {
        // Get or create a semaphore for this specific ToDoListId to prevent race conditions
        SemaphoreSlim semaphore = _locks.GetOrAdd(toDoListId, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            bool shouldAddSuggestions = await ShouldAddSuggestions(toDoListId, integrationEvent.UserId, toDoListDataPort, suggestionsRepository, logger);
            if (!shouldAddSuggestions)
                return;

            await AddAiSuggestions(toDoListId, integrationEvent.UserId, toDoListDataPort, itemSuggestionsPort, suggestionsRepository, messageBus);
        }
        finally
        {
            semaphore.Release();

            // Clean up the semaphore if it's no longer in use to prevent memory leaks
            if (semaphore.CurrentCount == 1 && _locks.TryRemove(toDoListId, out SemaphoreSlim? removedSemaphore))
            {
                removedSemaphore?.Dispose();
            }
        }
    }


    private static async Task<bool> ShouldAddSuggestions(
        ToDoListId toDoListId,
        string userId,
        IToDoListDataPort toDoListDataPort,
        IToDoListSuggestionsRepository suggestionsRepository,
        ILogger logger)
    {
        bool hasExistingSuggestions = await suggestionsRepository.ExistsForToDoListAsync(toDoListId);
        if (hasExistingSuggestions)
        {
            logger.LogDebug("AI suggestions already exist for ToDoList {ToDoListId}", toDoListId);
            return false;
        }

        ToDoListSnapshot? snapshot = await toDoListDataPort.GetToDoListSnapshotAsync(toDoListId, userId);
        if (snapshot == null)
        {
            logger.LogDebug("Could not retrieve ToDoList snapshot for ToDoList {ToDoListId}", toDoListId);
            return false;
        }

        logger.LogDebug("ToDoList {ToDoListId} has {ItemCount} items", toDoListId, snapshot.Items.Count);
        return ShouldTriggerAiSuggestions(snapshot.Items.Count);
    }

    private static async Task AddAiSuggestions(
        ToDoListId toDoListId,
        string userId,
        IToDoListDataPort toDoListDataPort,
        IItemSuggestionsPort itemSuggestionsPort,
        IToDoListSuggestionsRepository suggestionsRepository,
        IMessageBus messageBus)
    {
        ToDoListSnapshot? snapshot = await toDoListDataPort.GetToDoListSnapshotAsync(toDoListId, userId);
        if (snapshot == null)
            return;

        Result<IReadOnlyList<SuggestedItemTitle>> suggestionsResult = await itemSuggestionsPort.GenerateSuggestionsAsync(snapshot, SuggestionTriggerItemCount);
        if (suggestionsResult.IsFailure)
            return;

        var suggestions = ToDoListSuggestions.Create(
            ToDoListSuggestionsId.New(),
            toDoListId,
            DateTime.UtcNow);

        foreach (SuggestedItemTitle suggestionTitle in suggestionsResult.Value)
        {
            AddToDoCommand command = new()
            {
                ToDoListId = toDoListId.Value.ToString(),
                Title = suggestionTitle.Value
            };

            Result<AddToDoResult> addResult = await messageBus.InvokeAsync<Result<AddToDoResult>>(command);
            if (addResult.IsSuccess)
            {
                Result<ToDoId> correspondingToDoId = ToDoId.FromGuid(addResult.Value.Id);
                if (correspondingToDoId.IsSuccess)
                {
                    suggestions.AddSuggestedItem(suggestionTitle, correspondingToDoId.Value, DateTime.UtcNow);
                }
            }
        }

        if (suggestions.SuggestedItemCount > 0)
        {
            try
            {
                await suggestionsRepository.AddAsync(suggestions);
                await suggestionsRepository.SaveChangesAsync();
            }
            catch (Exception)
            {
                // If saving fails (likely unique constraint violation), it means suggestions already exist
                // This can happen due to race conditions - multiple events processed concurrently
                // The goal (AI suggestions exist for this list) is already achieved, so we can safely continue
                return;
            }
        }
    }

    private static bool ShouldTriggerAiSuggestions(int itemCount) => itemCount == SuggestionTriggerItemCount;
}

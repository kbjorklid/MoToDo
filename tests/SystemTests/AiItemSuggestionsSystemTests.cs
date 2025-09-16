using System.Net;
using AiItemSuggestions.Domain;
using Base.Domain.Result;
using NSubstitute;
using SystemTests.ToDoLists;
using ToDoLists.Contracts;

namespace SystemTests;

/// <summary>
/// System tests for AI Item Suggestions functionality that spans across ToDoLists and AiItemSuggestions modules.
/// </summary>
public class AiItemSuggestionsSystemTests : BaseSystemTest
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    public AiItemSuggestionsSystemTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task PostTodos_WhenAddingThirdItemToList_AutomaticallyGeneratesThreeAiSuggestions()
    {
        // Arrange
        ConfigureMockToReturnSuggestions("AI Suggestion 1", "AI Suggestion 2", "AI Suggestion 3");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Shopping List");
        Guid listId = todoList.ToDoListId;

        // Add first two items
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Buy milk");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Buy bread");

        // Act - Add the third item which should trigger AI suggestions
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Buy eggs");

        // Wait for Wolverine to process domain events and generate AI suggestions
        await CreatePollingHelper(listId, todoList.UserId)
            .WaitUntilTodoCountAtAsync(6);

        // Assert - Verify that the list now contains 6 items (3 original + 3 AI suggestions)
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(6, result.TodoCount);
        Assert.Equal(6, result.Todos.Length);

        Assert.Contains(result.Todos, t => t.Title == "Buy milk");
        Assert.Contains(result.Todos, t => t.Title == "Buy bread");
        Assert.Contains(result.Todos, t => t.Title == "Buy eggs");
        Assert.Contains(result.Todos, t => t.Title == "AI Suggestion 1");
        Assert.Contains(result.Todos, t => t.Title == "AI Suggestion 2");
        Assert.Contains(result.Todos, t => t.Title == "AI Suggestion 3");

        IEnumerable<ToDoApiDto> aiSuggestions = result.Todos.Where(t => t.Title.StartsWith("AI Suggestion "));
        foreach (ToDoApiDto suggestion in aiSuggestions)
        {
            Assert.False(suggestion.IsCompleted);
            Assert.NotEqual(Guid.Empty.ToString(), suggestion.Id);
        }
    }

    [Fact]
    public async Task PostTodos_WhenListHasTwoItems_DoesNotGenerateAiSuggestions()
    {
        // Arrange
        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Task List");
        Guid listId = todoList.ToDoListId;

        // Act - Add only 2 items (should not trigger AI suggestions)
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task 1");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task 2");

        // Verify no background processing occurs - count should remain at 2
        ToDoListPollingHelper pollingHelper = CreatePollingHelper(listId, todoList.UserId);
        await pollingHelper.WaitUntilTodoCountAtAsync(2);
        await pollingHelper.EnsureTodoCountDoesNotChangeAsync();

        // Assert - Verify that the list contains exactly 2 items (no AI suggestions added)
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(2, result.TodoCount);
        Assert.Equal(2, result.Todos.Length);

        // Verify only the original items are present
        Assert.Contains(result.Todos, t => t.Title == "Task 1");
        Assert.Contains(result.Todos, t => t.Title == "Task 2");
    }

    [Fact]
    public async Task PostTodos_WhenListReachesFourItemsViaAiSuggestions_DoesNotGenerateMoreSuggestions()
    {
        // Arrange
        ConfigureMockToReturnSuggestions("AI Task A", "AI Task B", "AI Task C");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Project Tasks");
        Guid listId = todoList.ToDoListId;

        // Add 3 items to trigger initial AI suggestions (should result in 6 total: 3 original + 3 AI)
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Analysis");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Design");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Implementation");

        // Wait for initial AI suggestions to be processed
        await CreatePollingHelper(listId, todoList.UserId).WaitUntilTodoCountAtAsync(6);

        // Act - Add the fourth original item (should result in 7 total items)
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Testing");

        // Verify no additional AI suggestions are generated after the fourth item
        ToDoListPollingHelper helper = CreatePollingHelper(listId, todoList.UserId);
        await helper.WaitUntilTodoCountAtAsync(7);
        await helper.EnsureTodoCountDoesNotChangeAsync();

        // Assert - Verify that the list contains exactly 7 items (4 original + 3 AI suggestions, no additional AI suggestions)
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(7, result.TodoCount);
        Assert.Equal(7, result.Todos.Length);

        // Verify all original items are present
        Assert.Contains(result.Todos, t => t.Title == "Analysis");
        Assert.Contains(result.Todos, t => t.Title == "Design");
        Assert.Contains(result.Todos, t => t.Title == "Implementation");
        Assert.Contains(result.Todos, t => t.Title == "Testing");

        // Verify AI suggestions are present
        Assert.Contains(result.Todos, t => t.Title == "AI Task A");
        Assert.Contains(result.Todos, t => t.Title == "AI Task B");
        Assert.Contains(result.Todos, t => t.Title == "AI Task C");
    }

    [Fact]
    public async Task PostTodos_WhenListAlreadyHasAiSuggestions_DoesNotGenerateMoreSuggestions()
    {
        // Arrange
        ConfigureMockToReturnSuggestions("Smart suggestion A", "Smart suggestion B", "Smart suggestion C");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Work Tasks");
        Guid listId = todoList.ToDoListId;

        // Add 3 items to trigger initial AI suggestions
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Meeting prep");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Code review");
        AddToDoResult thirdItem = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Update docs");

        // Wait for Wolverine to process domain events and generate AI suggestions
        await CreatePollingHelper(listId, todoList.UserId).WaitUntilTodoCountAtAsync(6);

        // Verify initial AI suggestions were generated (should have 6 total items)
        HttpResponseMessage initialResponse = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        ToDoListDetailApiResponse initialResult = await FromJsonAsync<ToDoListDetailApiResponse>(initialResponse);
        Assert.Equal(6, initialResult.TodoCount);

        // Act - Add more items after AI suggestions have already been generated
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Deploy to staging");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "User testing");

        // Verify no additional AI suggestions are generated
        ToDoListPollingHelper helper = CreatePollingHelper(listId, todoList.UserId);
        await helper.WaitUntilTodoCountAtAsync(8);
        await helper.EnsureTodoCountDoesNotChangeAsync();

        // Assert - Verify that no additional AI suggestions were generated
        HttpResponseMessage finalResponse = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        Assert.Equal(HttpStatusCode.OK, finalResponse.StatusCode);

        ToDoListDetailApiResponse finalResult = await FromJsonAsync<ToDoListDetailApiResponse>(finalResponse);
        Assert.Equal(8, finalResult.TodoCount); // 6 from before + 2 new items, no additional AI suggestions

        // Verify the new items are present
        Assert.Contains(finalResult.Todos, t => t.Title == "Deploy to staging");
        Assert.Contains(finalResult.Todos, t => t.Title == "User testing");
    }

    [Fact]
    public async Task PostTodos_AiSuggestionsFlow_VerifiesCompleteDataConsistency()
    {
        // Arrange
        ConfigureMockToReturnSuggestions("Generated Task 1", "Generated Task 2", "Generated Task 3");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Complete Test");
        Guid listId = todoList.ToDoListId;
        Guid userId = todoList.UserId;

        // Act - Add exactly 3 items to trigger AI suggestions
        AddToDoResult[] originalTodos =
        [
            await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original task 1"),
            await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original task 2"),
            await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original task 3")
        ];

        await CreatePollingHelper(listId, userId).WaitUntilTodoCountAtAsync(6);

        // Assert - Comprehensive verification of the complete state
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={userId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);

        // Verify total count
        Assert.Equal(6, result.TodoCount);
        Assert.Equal(6, result.Todos.Length);

        // Verify all original todos are present with correct data
        foreach (AddToDoResult? originalTodo in originalTodos)
        {
            ToDoApiDto? foundTodo = result.Todos.SingleOrDefault(t => t.Id == originalTodo.Id.ToString());
            Assert.NotNull(foundTodo);
            Assert.Equal(originalTodo.Title, foundTodo.Title);
            Assert.Equal(originalTodo.IsCompleted, foundTodo.IsCompleted);
            Assert.Equal(originalTodo.CreatedAt, foundTodo.CreatedAt);
            Assert.Equal(originalTodo.CompletedAt, foundTodo.CompletedAt);
        }

        // Verify AI suggestions are present and valid
        var originalTodoIds = originalTodos.Select(t => t.Id.ToString()).ToHashSet();
        var aiSuggestions = result.Todos.Where(t => !originalTodoIds.Contains(t.Id)).ToList();

        Assert.Equal(3, aiSuggestions.Count);

        // Verify the specific mock AI suggestions are present
        Assert.Contains(result.Todos, t => t.Title == "Generated Task 1");
        Assert.Contains(result.Todos, t => t.Title == "Generated Task 2");
        Assert.Contains(result.Todos, t => t.Title == "Generated Task 3");

        foreach (ToDoApiDto? aiSuggestion in aiSuggestions)
        {
            // Verify AI suggestions have valid structure
            Assert.NotEqual(Guid.Empty.ToString(), aiSuggestion.Id);
            Assert.False(aiSuggestion.IsCompleted);
            Assert.Null(aiSuggestion.CompletedAt);

            // Verify AI suggestions have been created at test time (using FakeTimeProvider)
            Assert.Equal(FakeTimeProvider.GetUtcNow().UtcDateTime, aiSuggestion.CreatedAt);
        }

        // Verify no duplicate titles across all items
        var allTitles = result.Todos.Select(t => t.Title).ToList();
        var uniqueTitles = allTitles.Distinct().ToList();
        Assert.Equal(allTitles.Count, uniqueTitles.Count);
    }

    /// <summary>
    /// Configures the mock AI suggestions port to return specific suggestion titles.
    /// </summary>
    private static void ConfigureMockToReturnSuggestions(params string[] suggestionTitles)
    {
        ClearPreviousMockConfigurations();
        ConfigureMockToReturnSpecificSuggestions(suggestionTitles);
    }

    private static void ClearPreviousMockConfigurations()
    {
        DatabaseFixture.MockItemSuggestionsPort.ClearReceivedCalls();
    }

    private static void ConfigureMockToReturnSpecificSuggestions(string[] suggestionTitles)
    {
        DatabaseFixture.MockItemSuggestionsPort.GenerateSuggestionsAsync(Arg.Any<ToDoListSnapshot>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                int requestedCount = callInfo.ArgAt<int>(1);
                IReadOnlyList<SuggestedItemTitle> suggestions = CreateSuggestionsFromTitles(suggestionTitles, requestedCount);
                return Task.FromResult(Result<IReadOnlyList<SuggestedItemTitle>>.Success(suggestions));
            });
    }

    private static IReadOnlyList<SuggestedItemTitle> CreateSuggestionsFromTitles(string[] suggestionTitles, int requestedCount)
    {
        var suggestions = new List<SuggestedItemTitle>();
        int countToReturn = Math.Min(requestedCount, suggestionTitles.Length);

        for (int i = 0; i < countToReturn; i++)
        {
            Result<SuggestedItemTitle> titleResult = SuggestedItemTitle.Create(suggestionTitles[i]);
            if (titleResult.IsSuccess)
            {
                suggestions.Add(titleResult.Value);
            }
        }

        return suggestions.AsReadOnly();
    }

    /// <summary>
    /// Helper class for polling todo list state changes during system tests.
    /// </summary>
    private class ToDoListPollingHelper(
        HttpClient httpClient,
        Guid listId,
        Guid userId,
        Func<HttpResponseMessage, Task<ToDoListDetailApiResponse>> fromJsonAsync)
    {
        private int? _lastObservedCount;

        /// <summary>
        /// Waits until the todo list reaches the expected count.
        /// </summary>
        public async Task<ToDoListPollingHelper> WaitUntilTodoCountAtAsync(int expectedCount, TimeSpan? timeout = null)
        {
            timeout ??= DefaultTimeout;
            DateTime endTime = DateTime.UtcNow.Add(timeout.Value);

            while (DateTime.UtcNow < endTime)
            {
                HttpResponseMessage response = await httpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={userId}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    ToDoListDetailApiResponse result = await fromJsonAsync(response);

                    if (result.TodoCount == expectedCount)
                    {
                        _lastObservedCount = expectedCount; // Store the count for potential chaining
                        return this; // Success - found expected count
                    }
                }

                await Task.Delay(PollInterval);
            }

            // Get final state for error message
            HttpResponseMessage finalResponse = await httpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={userId}");
            if (finalResponse.StatusCode == HttpStatusCode.OK)
            {
                ToDoListDetailApiResponse finalResult = await fromJsonAsync(finalResponse);
                Assert.Fail($"Timeout waiting for todo count. Expected: {expectedCount}, Actual: {finalResult.TodoCount}");
            }
            else
            {
                Assert.Fail($"Timeout waiting for todo count. Expected: {expectedCount}, Response: {finalResponse.StatusCode}");
            }

            return this; // This line will never be reached due to Assert.Fail above
        }

        /// <summary>
        /// Ensures that the todo count does not change during the specified timeout period.
        /// Used for negative test cases where we expect no background processing to occur.
        /// If called after WaitUntilTodoCountAtAsync, uses the previously observed count as the baseline.
        /// </summary>
        public async Task<ToDoListPollingHelper> EnsureTodoCountDoesNotChangeAsync(TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromMilliseconds(500);

            int initialCount;
            if (_lastObservedCount.HasValue)
            {
                // Use the count from the previous WaitUntilTodoCountAtAsync call
                initialCount = _lastObservedCount.Value;
            }
            else
            {
                // Get current count if no previous count is stored
                HttpResponseMessage initialResponse = await httpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={userId}");
                Assert.Equal(HttpStatusCode.OK, initialResponse.StatusCode);
                ToDoListDetailApiResponse initialResult = await fromJsonAsync(initialResponse);
                initialCount = initialResult.TodoCount;
            }

            DateTime endTime = DateTime.UtcNow.Add(timeout.Value);

            while (DateTime.UtcNow < endTime)
            {
                HttpResponseMessage response = await httpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={userId}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    ToDoListDetailApiResponse result = await fromJsonAsync(response);

                    if (result.TodoCount != initialCount)
                    {
                        Assert.Fail($"Expected todo count to remain at {initialCount}, but it changed to {result.TodoCount}");
                    }
                }

                await Task.Delay(PollInterval);
            }

            return this; // Success - count remained stable
        }
    }

    /// <summary>
    /// Creates a polling helper for the specified todo list.
    /// </summary>
    private ToDoListPollingHelper CreatePollingHelper(Guid listId, Guid userId)
    {
        return new ToDoListPollingHelper(HttpClient, listId, userId, FromJsonAsync<ToDoListDetailApiResponse>);
    }

}

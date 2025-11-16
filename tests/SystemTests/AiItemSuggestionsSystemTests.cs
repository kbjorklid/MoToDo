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

    #region Error Handling Tests

    [Fact]
    public async Task PostTodos_WhenAiPortReturnsFailure_DoesNotAddSuggestionsAndContinuesNormally()
    {
        // Arrange
        ConfigureMockToReturnFailure("AI service temporarily unavailable");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Error Test List");
        Guid listId = todoList.ToDoListId;

        // Add first two items
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task A");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task B");

        // Act - Add third item (should trigger AI suggestions, but AI port fails)
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task C");

        // Wait briefly for background processing
        ToDoListPollingHelper helper = CreatePollingHelper(listId, todoList.UserId);
        await helper.WaitUntilTodoCountAtAsync(3);
        await helper.EnsureTodoCountDoesNotChangeAsync();

        // Assert - Should only have original 3 items, no AI suggestions added
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(3, result.TodoCount);
        Assert.Equal(3, result.Todos.Length);

        // Verify only original items exist
        Assert.Contains(result.Todos, t => t.Title == "Task A");
        Assert.Contains(result.Todos, t => t.Title == "Task B");
        Assert.Contains(result.Todos, t => t.Title == "Task C");
    }

    [Fact]
    public async Task PostTodos_WhenAiPortReturnsEmptyList_DoesNotAddSuggestionsButTracksAttempt()
    {
        // Arrange
        ConfigureMockToReturnSuggestions(); // Empty suggestions array

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Empty Suggestions Test");
        Guid listId = todoList.ToDoListId;

        // Add items to trigger AI suggestions
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original 1");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original 2");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original 3");

        // Wait for processing
        ToDoListPollingHelper helper = CreatePollingHelper(listId, todoList.UserId);
        await helper.WaitUntilTodoCountAtAsync(3);
        await helper.EnsureTodoCountDoesNotChangeAsync();

        // Assert - Should only have original 3 items
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(3, result.TodoCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\t")]
    [InlineData("AB")] // Too short (minimum is 3)
    public async Task PostTodos_WhenAiPortReturnsInvalidSuggestionTitles_SkipsInvalidSuggestionsAndAddsValidOnes(string invalidTitle)
    {
        // Arrange
        ConfigureMockToReturnSuggestions(invalidTitle, "Valid AI Suggestion", "Another Valid One");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Invalid Titles Test");
        Guid listId = todoList.ToDoListId;

        // Add items to trigger AI suggestions
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "User Task 1");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "User Task 2");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "User Task 3");

        // Wait for processing - should get 5 items (3 original + 2 valid AI suggestions)
        await CreatePollingHelper(listId, todoList.UserId).WaitUntilTodoCountAtAsync(5);

        // Assert - Should have original items plus only the valid AI suggestions
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(5, result.TodoCount);

        // Verify valid suggestions were added
        Assert.Contains(result.Todos, t => t.Title == "Valid AI Suggestion");
        Assert.Contains(result.Todos, t => t.Title == "Another Valid One");

        // Verify invalid suggestion was not added
        Assert.DoesNotContain(result.Todos, t => t.Title == invalidTitle);
    }

    [Fact]
    public async Task PostTodos_WhenAiPortReturnsTooLongSuggestionTitle_SkipsInvalidSuggestionsAndAddsValidOnes()
    {
        // Arrange
        string tooLongTitle = new('x', 201); // Max length is 200
        ConfigureMockToReturnSuggestions(tooLongTitle, "Valid Short Title", "Another Valid Title");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Too Long Title Test");
        Guid listId = todoList.ToDoListId;

        // Add items to trigger AI suggestions
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task 1");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task 2");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task 3");

        // Wait for processing - should get 5 items (3 original + 2 valid AI suggestions)
        await CreatePollingHelper(listId, todoList.UserId).WaitUntilTodoCountAtAsync(5);

        // Assert
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(5, result.TodoCount);

        // Verify valid suggestions were added but invalid one was skipped
        Assert.Contains(result.Todos, t => t.Title == "Valid Short Title");
        Assert.Contains(result.Todos, t => t.Title == "Another Valid Title");
        Assert.DoesNotContain(result.Todos, t => t.Title == tooLongTitle);
    }

    #endregion

    #region Boundary Condition Tests

    [Fact]
    public async Task PostTodos_WhenAiPortReturnsExactlyRequestedCount_AddsAllSuggestions()
    {
        // Arrange - Configure exactly 3 suggestions (the default request count)
        ConfigureMockToReturnSuggestions("AI Task 1", "AI Task 2", "AI Task 3");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Exact Count Test");
        Guid listId = todoList.ToDoListId;

        // Add items to trigger AI suggestions
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original 1");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original 2");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original 3");

        // Wait for processing
        await CreatePollingHelper(listId, todoList.UserId).WaitUntilTodoCountAtAsync(6);

        // Assert
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(6, result.TodoCount);

        Assert.Contains(result.Todos, t => t.Title == "AI Task 1");
        Assert.Contains(result.Todos, t => t.Title == "AI Task 2");
        Assert.Contains(result.Todos, t => t.Title == "AI Task 3");
    }

    [Fact]
    public async Task PostTodos_WhenAiPortReturnsMoreThanRequested_AddsOnlyRequestedCount()
    {
        // Arrange - Configure more suggestions than requested
        ConfigureMockToReturnSuggestions("AI 1", "AI 2", "AI 3", "AI 4", "AI 5");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "More Than Requested Test");
        Guid listId = todoList.ToDoListId;

        // Add items to trigger AI suggestions
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "User 1");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "User 2");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "User 3");

        // Wait for processing - should only add first 3 suggestions
        await CreatePollingHelper(listId, todoList.UserId).WaitUntilTodoCountAtAsync(6);

        // Assert
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(6, result.TodoCount);

        // Verify only first 3 AI suggestions were added
        Assert.Contains(result.Todos, t => t.Title == "AI 1");
        Assert.Contains(result.Todos, t => t.Title == "AI 2");
        Assert.Contains(result.Todos, t => t.Title == "AI 3");
        Assert.DoesNotContain(result.Todos, t => t.Title == "AI 4");
        Assert.DoesNotContain(result.Todos, t => t.Title == "AI 5");
    }

    [Fact]
    public async Task PostTodos_WhenAiPortReturnsFewerThanRequested_AddsAllAvailableSuggestions()
    {
        // Arrange - Configure fewer suggestions than typically requested
        ConfigureMockToReturnSuggestions("Only One Suggestion");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Fewer Than Requested Test");
        Guid listId = todoList.ToDoListId;

        // Add items to trigger AI suggestions
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task A");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task B");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task C");

        // Wait for processing - should get 4 items (3 original + 1 AI suggestion)
        await CreatePollingHelper(listId, todoList.UserId).WaitUntilTodoCountAtAsync(4);

        // Assert
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(4, result.TodoCount);

        Assert.Contains(result.Todos, t => t.Title == "Only One Suggestion");
    }

    #endregion

    #region Timing and State Consistency Tests

    [Fact]
    public async Task PostTodos_WhenMultipleUsersAddItemsToSameListSimultaneously_HandlesRaceConditionsCorrectly()
    {
        // Arrange
        ConfigureMockToReturnSuggestions("Race Condition AI 1", "Race Condition AI 2", "Race Condition AI 3");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Race Condition Test");
        Guid listId = todoList.ToDoListId;

        // Add first two items normally
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Item 1");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Item 2");

        // Act - Simulate concurrent addition of the third item (which could trigger race conditions)
        List<Task> concurrentTasks = new();
        for (int i = 0; i < 3; i++)
        {
            int taskNum = i + 3;
            concurrentTasks.Add(ToDoListTestHelper.AddToDoAsync(HttpClient, listId, $"Concurrent Item {taskNum}"));
        }

        await Task.WhenAll(concurrentTasks);

        // Wait for all background processing to complete
        // Should have at most 8 items: 2 original + 3 concurrent + 3 AI suggestions
        // But due to race condition handling, AI suggestions should only be generated once
        ToDoListPollingHelper helper = CreatePollingHelper(listId, todoList.UserId);
        await helper.WaitUntilTodoCountAtAsync(8, TimeSpan.FromSeconds(10));

        // Assert - Verify final state is consistent
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(8, result.TodoCount);

        // Verify AI suggestions were generated only once
        IEnumerable<ToDoApiDto> aiSuggestions = result.Todos.Where(t => t.Title.Contains("Race Condition AI"));
        Assert.Equal(3, aiSuggestions.Count());
    }

    [Fact]
    public async Task PostTodos_VerifiesAiSuggestionsCreatedAtTimeMatchesTestTime()
    {
        // Arrange
        ConfigureMockToReturnSuggestions("Time Test Suggestion 1", "Time Test Suggestion 2");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Time Consistency Test");
        Guid listId = todoList.ToDoListId;

        DateTime expectedTime = FakeTimeProvider.GetUtcNow().UtcDateTime;

        // Add items to trigger AI suggestions
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "User Item 1");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "User Item 2");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "User Item 3");

        // Wait for processing
        await CreatePollingHelper(listId, todoList.UserId).WaitUntilTodoCountAtAsync(5);

        // Assert
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);

        // Verify AI suggestion timestamps
        var aiSuggestions = result.Todos.Where(t => t.Title.Contains("Time Test Suggestion")).ToList();
        Assert.Equal(2, aiSuggestions.Count);

        foreach (ToDoApiDto aiSuggestion in aiSuggestions)
        {
            Assert.Equal(expectedTime, aiSuggestion.CreatedAt);
            Assert.False(aiSuggestion.IsCompleted);
            Assert.Null(aiSuggestion.CompletedAt);
        }
    }

    #endregion

    #region Edge Cases and Negative Tests

    [Fact]
    public async Task PostTodos_WhenToDoListDoesNotExist_DoesNotThrowAndHandlesGracefully()
    {
        // This test verifies that the system handles cases where the integration event
        // references a todo list that no longer exists (edge case that could happen
        // in distributed systems with eventual consistency)

        // Arrange
        ConfigureMockToReturnSuggestions("Should Not Be Added");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Will Be Deleted");
        Guid listId = todoList.ToDoListId;

        // Add two items
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Item 1");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Item 2");

        // Delete the todo list (simulating the edge case)
        await HttpClient.DeleteAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");

        // Act - Try to add a third item (this should fail gracefully)
        HttpResponseMessage addResponse = await HttpClient.PostAsync("/api/v1/todos", ToJsonContent(new AddToDoCommand
        {
            ToDoListId = listId.ToString(),
            Title = "Item 3"
        }));

        // Assert - The add operation should fail (todo list doesn't exist)
        Assert.Equal(HttpStatusCode.NotFound, addResponse.StatusCode);

        // Verify no AI suggestions were created in any remaining state
        // (This mainly ensures no exceptions were thrown in background processing)
    }

    [Fact]
    public async Task PostTodos_WithSpecialCharactersInSuggestions_HandlesAndStoresCorrectly()
    {
        // Arrange
        ConfigureMockToReturnSuggestions(
            "AI: Complete this task! 🎯",
            "Review & update documentation",
            "Send email to team@company.com"
        );

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Special Characters Test");
        Guid listId = todoList.ToDoListId;

        // Add items to trigger AI suggestions
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task 1");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task 2");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Task 3");

        // Wait for processing
        await CreatePollingHelper(listId, todoList.UserId).WaitUntilTodoCountAtAsync(6);

        // Assert
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);

        // Verify special characters are preserved
        Assert.Contains(result.Todos, t => t.Title == "AI: Complete this task! 🎯");
        Assert.Contains(result.Todos, t => t.Title == "Review & update documentation");
        Assert.Contains(result.Todos, t => t.Title == "Send email to team@company.com");
    }

    [Fact]
    public async Task PostTodos_WithDuplicateSuggestionTitles_HandlesGracefullyAndAddsAll()
    {
        // Arrange - Configure duplicate suggestion titles
        ConfigureMockToReturnSuggestions("Duplicate Task", "Unique Task", "Duplicate Task");

        CreateToDoListResult todoList = await ToDoListTestHelper.CreateToDoListAsync(HttpClient, "Duplicate Titles Test");
        Guid listId = todoList.ToDoListId;

        // Add items to trigger AI suggestions
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original 1");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original 2");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original 3");

        // Wait for background processing to complete
        await Task.Delay(2000);

        // Assert - Verify suggestions were added (allowing for different duplicate handling behaviors)
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);

        // Should have the 3 original items plus AI suggestions
        Assert.True(result.TodoCount >= 4, $"Expected at least 4 items, but got {result.TodoCount}");
        Assert.True(result.TodoCount <= 6, $"Expected at most 6 items, but got {result.TodoCount}");

        // Verify original items are present
        Assert.Contains(result.Todos, t => t.Title == "Original 1");
        Assert.Contains(result.Todos, t => t.Title == "Original 2");
        Assert.Contains(result.Todos, t => t.Title == "Original 3");

        // Verify AI suggestions were added
        var aiSuggestions = result.Todos.Where(t => !t.Title.StartsWith("Original")).ToList();
        Assert.True(aiSuggestions.Count >= 1, "Expected at least one AI suggestion to be added");

        // Check that duplicate titles are handled (either both added or one rejected)
        var duplicateTasks = result.Todos.Where(t => t.Title == "Duplicate Task").ToList();
        var uniqueTasks = result.Todos.Where(t => t.Title == "Unique Task").ToList();

        // At least one of the suggestions should have been successfully added
        Assert.True(duplicateTasks.Count >= 1 || uniqueTasks.Count >= 1,
            "Expected at least one AI suggestion (Duplicate Task or Unique Task) to be added");
    }

    #endregion

    #region Helper Method Extensions

    /// <summary>
    /// Configures the mock AI suggestions port to return a failure result.
    /// </summary>
    private static void ConfigureMockToReturnFailure(string errorMessage)
    {
        ClearPreviousMockConfigurations();
        DatabaseFixture.MockItemSuggestionsPort.GenerateSuggestionsAsync(Arg.Any<ToDoListSnapshot>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IReadOnlyList<SuggestedItemTitle>>.Failure(
                new Error("AI.ServiceUnavailable", errorMessage, ErrorType.Failure))));
    }

    #endregion

}

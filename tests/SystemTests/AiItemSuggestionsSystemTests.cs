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
    private const int WolverineProcessingDelayMs = 3000;
    private const int BackgroundProcessingDelayMs = 1000;

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
        AddToDoResult thirdItem = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Buy eggs");

        // Wait for Wolverine to process domain events in Solo mode
        await Task.Delay(WolverineProcessingDelayMs);

        // Assert - Verify that the list now contains 6 items (3 original + 3 AI suggestions)
        HttpResponseMessage response = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ToDoListDetailApiResponse result = await FromJsonAsync<ToDoListDetailApiResponse>(response);
        Assert.Equal(6, result.TodoCount);
        Assert.Equal(6, result.Todos.Length);

        // Verify the original 3 items are present
        Assert.Contains(result.Todos, t => t.Title == "Buy milk");
        Assert.Contains(result.Todos, t => t.Title == "Buy bread");
        Assert.Contains(result.Todos, t => t.Title == "Buy eggs");

        // Verify that 3 additional AI-suggested items were added
        string[] originalTitles = new[] { "Buy milk", "Buy bread", "Buy eggs" };
        var aiSuggestedItems = result.Todos.Where(t => !originalTitles.Contains(t.Title)).ToList();
        Assert.Equal(3, aiSuggestedItems.Count);

        // Verify the mock AI suggestions have the expected titles
        Assert.Contains(result.Todos, t => t.Title == "AI Suggestion 1");
        Assert.Contains(result.Todos, t => t.Title == "AI Suggestion 2");
        Assert.Contains(result.Todos, t => t.Title == "AI Suggestion 3");

        // Verify all AI suggestions are not completed
        foreach (ToDoApiDto? suggestion in aiSuggestedItems)
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

        // Allow time for any potential background processing
        await Task.Delay(BackgroundProcessingDelayMs);

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
        await Task.Delay(3000);

        // Act - Add the fourth original item (should result in 7 total items)
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Testing");

        // Allow time for any potential background processing
        await Task.Delay(BackgroundProcessingDelayMs);

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

        // Wait for Wolverine to process domain events in Solo mode
        await Task.Delay(WolverineProcessingDelayMs);

        // Verify initial AI suggestions were generated (should have 6 total items)
        HttpResponseMessage initialResponse = await HttpClient.GetAsync($"/api/v1/todo-lists/{listId}?userId={todoList.UserId}");
        ToDoListDetailApiResponse initialResult = await FromJsonAsync<ToDoListDetailApiResponse>(initialResponse);
        Assert.Equal(6, initialResult.TodoCount);

        // Act - Add more items after AI suggestions have already been generated
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Deploy to staging");
        _ = await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "User testing");

        // Allow time for any potential background processing
        await Task.Delay(BackgroundProcessingDelayMs);

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
        AddToDoResult[] originalTodos = new[]
        {
            await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original task 1"),
            await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original task 2"),
            await ToDoListTestHelper.AddToDoAsync(HttpClient, listId, "Original task 3")
        };

        // Wait for Wolverine to process domain events in Solo mode
        await Task.Delay(WolverineProcessingDelayMs);

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
        // Clear any previous configurations
        DatabaseFixture.MockItemSuggestionsPort.ClearReceivedCalls();

        // Configure mock to return the specified suggestions
        DatabaseFixture.MockItemSuggestionsPort.GenerateSuggestionsAsync(Arg.Any<ToDoListSnapshot>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                int requestedCount = callInfo.ArgAt<int>(1);
                var suggestions = new List<SuggestedItemTitle>();

                // Return up to the requested count or available titles, whichever is smaller
                int countToReturn = Math.Min(requestedCount, suggestionTitles.Length);

                for (int i = 0; i < countToReturn; i++)
                {
                    Result<SuggestedItemTitle> titleResult = SuggestedItemTitle.Create(suggestionTitles[i]);
                    if (titleResult.IsSuccess)
                    {
                        suggestions.Add(titleResult.Value);
                    }
                }

                return Task.FromResult<Result<IReadOnlyList<SuggestedItemTitle>>>(suggestions.AsReadOnly());
            });
    }

}

// Copyright (c) Microsoft. All rights reserved.

using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Planning;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Planners.UnitTests.Handlebars;

public sealed class FunctionCallingStepwisePlannerTests
{
    [Theory]
    [InlineData("Write a poem and send it to Mary.")]
    public async Task ItCanCreatePlanAsync(string goal)
    {
        // Arrange
        var plugins = this.CreatePluginCollection();
        var kernel = this.CreateKernelWithMockCompletionResult(plugins);
        var planner = new FunctionCallingStepwisePlanner();

        // Act
        FunctionCallingStepwisePlannerResult plan = await planner.ExecuteAsync(kernel, goal);

        // Assert
        Assert.True(!string.IsNullOrEmpty(plan.FinalAnswer));
        //Assert.True(!string.IsNullOrEmpty(plan.ToString()));
    }

    [Fact]
    public async Task EmptyGoalThrowsAsync()
    {
        // Arrange
        var kernel = this.CreateKernelWithMockCompletionResult();

        var planner = new FunctionCallingStepwisePlanner();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await planner.ExecuteAsync(kernel, string.Empty));
    }

    private Kernel CreateKernelWithMockCompletionResult(KernelPluginCollection? plugins = null)
    {
        plugins ??= new KernelPluginCollection();

        var chatCompletion = new Mock<IChatCompletionService>();

        // Mock response for no tool/function calling
        chatCompletion
            .Setup(cc => cc.GetChatMessageContentsAsync(
                    It.IsAny<ChatHistory>(),
                    It.Is<PromptExecutionSettings>(pes => pes == null || (pes.ExtensionData == null) || pes.ExtensionData["tool_call_behavior"] == null),
                    It.IsAny<Kernel>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent> { new ChatMessageContent(AuthorRole.Assistant, "Mock model response") });

        // Mock responses for function calling
        chatCompletion
            .Setup(cc => cc.GetChatMessageContentsAsync(
                It.Is<ChatHistory>(ch => ch.Where<ChatMessageContent>(mc => mc.Role == AuthorRole.User).Count() == 1),
                It.Is<PromptExecutionSettings>(pes => (pes is OpenAIPromptExecutionSettings) && ((OpenAIPromptExecutionSettings)pes).ToolCallBehavior != null),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent> { new ChatMessageContent(AuthorRole.Assistant, "First step") /*this._firstStepResponse*/ });
        chatCompletion
            .Setup(cc => cc.GetChatMessageContentsAsync(
                It.Is<ChatHistory>(ch => ch.Where<ChatMessageContent>(mc => mc.Role == AuthorRole.User).Count() == 2),
                It.Is<PromptExecutionSettings>(pes => (pes is OpenAIPromptExecutionSettings) && ((OpenAIPromptExecutionSettings)pes).ToolCallBehavior != null),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent> { new ChatMessageContent(AuthorRole.Assistant, "Second step") /*this._secondStepResponse*/ });
        chatCompletion
            .Setup(cc => cc.GetChatMessageContentsAsync(
                It.Is<ChatHistory>(ch => ch.Where<ChatMessageContent>(mc => mc.Role == AuthorRole.User).Count() == 3),
                It.Is<PromptExecutionSettings>(pes => (pes is OpenAIPromptExecutionSettings) && ((OpenAIPromptExecutionSettings)pes).ToolCallBehavior != null),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent> { new ChatMessageContent(AuthorRole.Assistant, "Third step") /*this._thirdStepResponse*/ });
        chatCompletion
            .Setup(cc => cc.GetChatMessageContentsAsync(
                It.Is<ChatHistory>(ch => ch.Where<ChatMessageContent>(mc => mc.Role == AuthorRole.User).Count() == 3),
                It.Is<PromptExecutionSettings>(pes => (pes is OpenAIPromptExecutionSettings) && ((OpenAIPromptExecutionSettings)pes).ToolCallBehavior != null),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent> { new ChatMessageContent(AuthorRole.Assistant, "Final answer step") /*this._thirdStepResponse*/ });

        // Mock AI service selector
        var serviceSelector = new Mock<IAIServiceSelector>();
        IChatCompletionService resultService = chatCompletion.Object;
        PromptExecutionSettings resultSettings = new();
        serviceSelector
            .Setup(ss => ss.TrySelectAIService<IChatCompletionService>(It.IsAny<Kernel>(), It.IsAny<KernelFunction>(), It.IsAny<KernelArguments>(), out resultService!, out resultSettings!))
            .Returns(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IAIServiceSelector>(serviceSelector.Object);
        serviceCollection.AddSingleton<IChatCompletionService>(chatCompletion.Object);

        return new Kernel(serviceCollection.BuildServiceProvider(), plugins);
    }

    private KernelPluginCollection CreatePluginCollection()
    {
        return new()
        {
            KernelPluginFactory.CreateFromFunctions("email", "Email functions", new[]
            {
                KernelFunctionFactory.CreateFromMethod(() => "Email sent!", "SendEmail", "Send an e-mail"),
                KernelFunctionFactory.CreateFromMethod(() => "Email address received!", "GetEmailAddress", "Get an e-mail address")
            }),
            KernelPluginFactory.CreateFromFunctions("WriterPlugin", "Writer functions", new[]
            {
                KernelFunctionFactory.CreateFromMethod(() => "Content translated!", "Translate", "Translate something"),
                KernelFunctionFactory.CreateFromMethod(() => "Poem written!", "WritePoem", "Write a poem about something"),
            }),
        };
    }

    // user inputs to model
    //string initialGoal = "Write a poem and send it to Mary.";
    string initialPlanResponse = "1. Write poem\n2. Look up Mary's email address\n3. Email poem to Mary";
    private readonly ChatMessageContent _firstStepResponse; // = new OpenAIChatMessageContent(); // TODO: function tool response for calling WritePoem
    private readonly ChatMessageContent _secondStepResponse; // TODO: function tool response for calling GetEmailAddress
    private readonly ChatMessageContent _thirdStepResponse; // TODO: function tool response for calling SendEmail
    private readonly ChatMessageContent _finalStepResponse; // TODO: function tool response for calling SendFinalAnswer


    private readonly ChatMessageContent _chatMessageContent1 = new(AuthorRole.Assistant, content: null, innerContent: null);

    private readonly OpenAIChatMessageContent openaiContent = new OpenAIChatMessageContent(
        AuthorRole.Assistant,
        null,
        "",
        null);

    //var toolCallSendEmail = new ChatCompletionsFunctionToolCall(); { Name = "", Arguments = null, Id = "" };

    // how to handle "perform the next step of the plan"?

    // how to mock the function responses?
    // how to tell if they are executed?


    // system message
    // user: perform next step
    // assistant
    // user: perform next step
    // assistant:


    // Mock our helper that parses the function call object from OpenAI?

}

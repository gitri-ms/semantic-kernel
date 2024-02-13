﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Xunit;

namespace SemanticKernel.Connectors.UnitTests.OpenAI.ChatCompletion;

public sealed class ToolFilterTests : IDisposable
{
    private readonly MultipleHttpMessageHandlerStub _messageHandlerStub;
    private readonly HttpClient _httpClient;

    public ToolFilterTests()
    {
        this._messageHandlerStub = new MultipleHttpMessageHandlerStub();
        this._httpClient = new HttpClient(this._messageHandlerStub, false);
    }

    [Fact]
    public async Task PreInvocationToolFilterIsTriggeredAsync()
    {
        // Arrange
        var toolInvocations = 0;
        var filterInvocations = 0;

        var kernel = new Kernel();
        kernel.ImportPluginFromObject(new FakePlugin(() => toolInvocations++));

        var service = new OpenAIChatCompletionService(modelId: "gpt-3.5-turbo", apiKey: "NOKEY", httpClient: this._httpClient);
        var settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        settings.ToolCallBehavior.Filters.Add(
            new FakeToolFilter(onToolInvoking: (context) =>
            {
                filterInvocations++;
            }));

        using var response1 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ToolResponseNoArgs) };
        using var response2 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(OpenAITestHelper.GetTestResponse("chat_completion_test_response.json")) };
        this._messageHandlerStub.ResponsesToReturn = [response1, response2];

        // Act
        var result = await service.GetChatMessageContentsAsync(new ChatHistory(), settings, kernel);

        // Assert
        Assert.Equal(1, toolInvocations);
        Assert.Equal(1, filterInvocations);
    }

    [Fact]
    public async Task PreInvocationToolFilterChangesArgumentAsync()
    {
        // Arrange
        const string NewInput = "newValue";

        var kernel = new Kernel();
        kernel.ImportPluginFromObject(new FakePluginWithArg((string originalInput) => originalInput));

        var service = new OpenAIChatCompletionService(modelId: "gpt-3.5-turbo", apiKey: "NOKEY", httpClient: this._httpClient);
        var settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        settings.ToolCallBehavior.Filters.Add(
            new FakeToolFilter(onToolInvoking: (context) =>
            {
                context.ToolCall.Arguments!["input"] = NewInput;
            }));

        using var response1 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ToolResponseWithArgs) };
        using var response2 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(OpenAITestHelper.GetTestResponse("chat_completion_test_response.json")) };
        this._messageHandlerStub.ResponsesToReturn = [response1, response2];

        // Act
        var chatHistory = new ChatHistory();
        var result = await service.GetChatMessageContentsAsync(chatHistory, settings, kernel);

        // Assert
        Assert.Equal(NewInput, chatHistory.Where(m => m.Role == AuthorRole.Tool).First().Content);
    }

    [Fact]
    public async Task PreInvocationToolFilterCancellationWorksCorrectlyAsync()
    {
        // Arrange
        var functionInvocations = 0;
        var preFilterInvocations = 0;
        var postFilterInvocations = 0;

        var kernel = new Kernel();
        kernel.ImportPluginFromObject(new FakePlugin(() => functionInvocations++));

        var chatHistory = new ChatHistory();
        var service = new OpenAIChatCompletionService(modelId: "gpt-3.5-turbo", apiKey: "NOKEY", httpClient: this._httpClient);
        var settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

        settings.ToolCallBehavior.Filters.Add(
            new FakeToolFilter(
                onToolInvoking: (context) =>
                {
                    preFilterInvocations++;
                    context.StopBehavior = ToolFilterStopBehavior.Cancel;
                },
                onToolInvoked: (context) =>
                {
                    postFilterInvocations++;
                }));

        using var response1 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ToolResponseNoArgs) };
        using var response2 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(OpenAITestHelper.GetTestResponse("chat_completion_test_response.json")) };
        this._messageHandlerStub.ResponsesToReturn = [response1, response2];

        // Act
        var result = await service.GetChatMessageContentAsync(chatHistory, settings, kernel);

        // Assert
        Assert.Equal(1, preFilterInvocations);
        Assert.Equal(0, functionInvocations);
        Assert.Equal(0, postFilterInvocations);
        Assert.Equal("A tool filter requested cancellation before tool invocation.", chatHistory.Last().Content);
    }

    [Fact]
    public async Task PostInvocationToolFilterIsTriggeredAsync()
    {
        // Arrange
        var functionInvocations = 0;
        var filterInvocations = 0;

        var kernel = new Kernel();
        kernel.ImportPluginFromObject(new FakePlugin(() => functionInvocations++));

        var service = new OpenAIChatCompletionService(modelId: "gpt-3.5-turbo", apiKey: "NOKEY", httpClient: this._httpClient);
        var settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

        settings.ToolCallBehavior.Filters.Add(
            new FakeToolFilter(onToolInvoked: (context) =>
            {
                filterInvocations++;
            }));

        using var response1 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ToolResponseNoArgs) };
        using var response2 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(OpenAITestHelper.GetTestResponse("chat_completion_test_response.json")) };
        this._messageHandlerStub.ResponsesToReturn = [response1, response2];

        // Act
        var result = await service.GetChatMessageContentAsync(new ChatHistory(), settings, kernel);

        // Assert
        Assert.Equal(1, functionInvocations);
        Assert.Equal(1, filterInvocations);
    }

    [Fact]
    public async Task PostInvocationToolFilterChangesChatHistoryAsync()
    {
        // Arrange
        var kernel = new Kernel();
        kernel.ImportPluginFromObject(new FakePlugin(() => { }));

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Hello, world!");
        var service = new OpenAIChatCompletionService(modelId: "gpt-3.5-turbo", apiKey: "NOKEY", httpClient: this._httpClient);
        var settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

        settings.ToolCallBehavior.Filters.Add(
            new FakeToolFilter(onToolInvoked: (context) =>
        {
            context.ChatHistory.AddAssistantMessage("Tool filter was invoked.");
            context.ChatHistory.First().Content = "Hello, SK!";
        }));

        using var response1 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ToolResponseNoArgs) };
        using var response2 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(OpenAITestHelper.GetTestResponse("chat_completion_test_response.json")) };
        this._messageHandlerStub.ResponsesToReturn = [response1, response2];

        // Act
        var result = await service.GetChatMessageContentAsync(chatHistory, settings, kernel);

        // Assert
        Assert.Equal(4, chatHistory.Count); // includes tool call and tool result messages
        Assert.Equal("Hello, SK!", chatHistory.First().Content);
        Assert.Equal("Tool filter was invoked.", chatHistory.Last().Content);
    }

    [Fact]
    public async Task MultipleFunctionFiltersCancellationWorksCorrectlyAsync()
    {
        // Arrange
        var functionInvocations = 0;
        var filterInvocations = 0;
        var kernel = new Kernel();
        kernel.ImportPluginFromObject(new FakePlugin(() => functionInvocations++));

        var toolFilter1 = new FakeToolFilter(onToolInvoking: (context) =>
        {
            filterInvocations++;
            context.StopBehavior = ToolFilterStopBehavior.Cancel;
        });

        var toolFilter2 = new FakeToolFilter(onToolInvoking: (context) =>
        {
            Assert.Equal(ToolFilterStopBehavior.Cancel, context.StopBehavior);

            filterInvocations++;
            context.StopBehavior = ToolFilterStopBehavior.None;
        });

        var service = new OpenAIChatCompletionService(modelId: "gpt-3.5-turbo", apiKey: "NOKEY", httpClient: this._httpClient);
        var settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        settings.ToolCallBehavior.Filters.Add(toolFilter1);
        settings.ToolCallBehavior.Filters.Add(toolFilter2);

        using var response1 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ToolResponseNoArgs) };
        using var response2 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(OpenAITestHelper.GetTestResponse("chat_completion_test_response.json")) };
        this._messageHandlerStub.ResponsesToReturn = [response1, response2];

        // Act
        var result = await service.GetChatMessageContentsAsync(new ChatHistory(), settings, kernel);

        // Assert
        Assert.Equal(1, functionInvocations);
        Assert.Equal(2, filterInvocations);
    }

    [Fact]
    public async Task FiltersAreExecutedInCorrectOrderAsync()
    {
        // Arrange
        var executionOrder = new List<string>();

        var toolFilter1 = new FakeToolFilter(
            (context) => executionOrder.Add("ToolFilter1-Invoking"),
            (context) => executionOrder.Add("ToolFilter1-Invoked"));

        var toolFilter2 = new FakeToolFilter(
            (context) => executionOrder.Add("ToolFilter2-Invoking"),
            (context) => executionOrder.Add("ToolFilter2-Invoked"));

        var toolFilter3 = new FakeToolFilter(
            (context) => executionOrder.Add("ToolFilter3-Invoking"),
            (context) => executionOrder.Add("ToolFilter3-Invoked"));

        var kernel = new Kernel();
        kernel.ImportPluginFromObject(new FakePlugin(() => { }));

        var service = new OpenAIChatCompletionService(modelId: "gpt-3.5-turbo", apiKey: "NOKEY", httpClient: this._httpClient);
        var settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

        settings.ToolCallBehavior.Filters.Add(toolFilter1);
        settings.ToolCallBehavior.Filters.Add(toolFilter2);
        settings.ToolCallBehavior.Filters.Insert(1, toolFilter3);

        using var response1 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ToolResponseNoArgs) };
        using var response2 = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(OpenAITestHelper.GetTestResponse("chat_completion_test_response.json")) };
        this._messageHandlerStub.ResponsesToReturn = [response1, response2];


        // Act
        var result = await service.GetChatMessageContentAsync(new ChatHistory(), settings, kernel);

        // Assert
        Assert.Equal("ToolFilter1-Invoking", executionOrder[0]);
        Assert.Equal("ToolFilter3-Invoking", executionOrder[1]);
        Assert.Equal("ToolFilter2-Invoking", executionOrder[2]);
        Assert.Equal("ToolFilter1-Invoked", executionOrder[3]);
        Assert.Equal("ToolFilter3-Invoked", executionOrder[4]);
        Assert.Equal("ToolFilter2-Invoked", executionOrder[5]);
    }

    private sealed class FakeToolFilter(
        Action<ToolInvokingContext>? onToolInvoking = null,
        Action<ToolInvokedContext>? onToolInvoked = null) : IToolFilter
    {
        private readonly Action<ToolInvokingContext>? _onToolInvoking = onToolInvoking;
        private readonly Action<ToolInvokedContext>? _onToolInvoked = onToolInvoked;

        public void OnToolInvoked(ToolInvokedContext context) =>
            this._onToolInvoked?.Invoke(context);

        public void OnToolInvoking(ToolInvokingContext context) =>
            this._onToolInvoking?.Invoke(context);
    }

    private sealed class FakePlugin(Action action)
    {
        [KernelFunction]
        public void Foo()
        {
            action();
        }
    }

    private sealed class FakePluginWithArg(Func<string, string> action)
    {
        [KernelFunction]
        public string Bar(string input)
        {
            return action(input);
        }
    }

    public void Dispose()
    {
        this._httpClient.Dispose();
        this._messageHandlerStub.Dispose();
    }

    private const string ToolResponseNoArgs = @"{
  ""id"": ""response-id"",
  ""object"": ""chat.completion"",
  ""created"": 1699896916,
  ""model"": ""gpt-3.5-turbo-0613"",
  ""choices"": [
    {
      ""index"": 0,
      ""message"": {
        ""role"": ""assistant"",
        ""content"": null,
        ""tool_calls"": [
          {
            ""id"": ""1"",
            ""type"": ""function"",
            ""function"": {
              ""name"": ""FakePlugin-Foo"",
              ""arguments"": ""{}""
            }
          }
        ]
      },
      ""logprobs"": null,
      ""finish_reason"": ""tool_calls""
    }
  ],
  ""usage"": {
    ""prompt_tokens"": 82,
    ""completion_tokens"": 17,
    ""total_tokens"": 99
  }
}
";

    private const string ToolResponseWithArgs = @"{
  ""id"": ""response-id"",
  ""object"": ""chat.completion"",
  ""created"": 1699896916,
  ""model"": ""gpt-3.5-turbo-0613"",
  ""choices"": [
    {
      ""index"": 0,
      ""message"": {
        ""role"": ""assistant"",
        ""content"": null,
        ""tool_calls"": [
          {
            ""id"": ""1"",
            ""type"": ""function"",
            ""function"": {
              ""name"": ""FakePluginWithArg-Bar"",
              ""arguments"": ""{\n\""input\"": \""oldValue\""\n}""
            }
          }
        ]
      },
      ""logprobs"": null,
      ""finish_reason"": ""tool_calls""
    }
  ],
  ""usage"": {
    ""prompt_tokens"": 82,
    ""completion_tokens"": 17,
    ""total_tokens"": 99
  }
}";

}

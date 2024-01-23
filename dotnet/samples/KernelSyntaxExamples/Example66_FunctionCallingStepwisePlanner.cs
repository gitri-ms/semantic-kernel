// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Plugins;
using Xunit;
using Xunit.Abstractions;

namespace Examples;

public class Example66_FunctionCallingStepwisePlanner : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        string[] questions = {
            "What is the current hour number, plus 5?",
            "What is 387 minus 22? Email the solution to John and Mary.",
            "Write a limerick, translate it to Spanish, and send it to Jane",
        };

        var kernel = await InitializeKernelAsync();

        var config = new FunctionCallingStepwisePlannerConfig
        {
            MaxIterations = 15,
            MaxTokens = 4000,
        };
        var planner = new FunctionCallingStepwisePlanner(config);

        foreach (var question in questions)
        {
            FunctionCallingStepwisePlannerResult result = await planner.ExecuteAsync(kernel, question);
            WriteLine($"Q: {question}\nA: {result.FinalAnswer}");

            // You can uncomment the line below to see the planner's process for completing the request.
            // Console.WriteLine($"Chat history:\n{System.Text.Json.JsonSerializer.Serialize(result.ChatHistory)}");
        }
    }

    /// <summary>
    /// Initialize the kernel and load plugins.
    /// </summary>
    /// <returns>A kernel instance</returns>
    private static async Task<Kernel> InitializeKernelAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                apiKey: TestConfiguration.OpenAI.ApiKey,
                modelId: "gpt-3.5-turbo-1106")
            .Build();

        kernel.ImportPluginFromType<EmailPlugin>();
        kernel.ImportPluginFromType<MathPlugin>();
        kernel.ImportPluginFromType<TimePlugin>();
        await kernel.ImportPluginFromOpenApiAsync(
            "CourseraPlugin",
            new Uri("https://www.coursera.org/api/rest/v1/search/openapi.yaml")
        );

        return kernel;
    }

    public Example66_FunctionCallingStepwisePlanner(ITestOutputHelper output) : base(output)
    {
    }
}

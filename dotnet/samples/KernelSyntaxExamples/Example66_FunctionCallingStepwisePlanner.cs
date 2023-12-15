// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Core;
using Plugins;

// ReSharper disable once InconsistentNaming
public static class Example66_FunctionCallingStepwisePlanner
{
    public static async Task RunAsync()
    {
        string[] questions = new string[]
        {
            "What is the current hour number, plus 5?",
            "What is 387 minus 22? Email the solution to John and Mary.",
            "Write a limerick, translate it to Spanish, and send it to Jane",
        };

        var kernel = InitializeKernel();

        var config = new FunctionCallingStepwisePlannerConfig
        {
            MaxIterations = 15,
            MaxTokens = 4000,
        };
        var planner = new FunctionCallingStepwisePlanner(config);

        foreach (var question in questions)
        {
            FunctionCallingStepwisePlannerResult result = await planner.ExecuteAsync(kernel, question);
            Console.WriteLine($"Q: {question}\nA: {result.FinalAnswer}");

            // You can uncomment the line below to see the planner's process for completing the request.
            //Console.WriteLine($"Chat history:\n{System.Text.Json.JsonSerializer.Serialize(result.ChatHistory)}");
            foreach (ChatMessageContent item in result.ChatHistory!)
            {
                Console.WriteLine($"[\nRole: {item.Role}");
                if (item.Content != null)
                {
                    Console.WriteLine($"Content: {item.Content}");
                }
                if (item.Metadata != null && item.Metadata.TryGetValue("ChatResponseMessage.ToolCalls", out object? value))
                {
                    Console.WriteLine($"{JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true })}");
                }
                Console.WriteLine("]");
                //item.
                // Metadata - ChatResponseMessage.ToolCalls, value.Function.Name / value.Function.Arguments, value.Function.Id
                // ChatCompletionsToolCall.Id,
            }
        }
    }

    /// <summary>
    /// Initialize the kernel and load plugins.
    /// </summary>
    /// <returns>A kernel instance</returns>
    private static Kernel InitializeKernel()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey,
                modelId: TestConfiguration.AzureOpenAI.ChatModelId)
            .Build();

        kernel.ImportPluginFromType<EmailPlugin>();
        kernel.ImportPluginFromType<MathPlugin>();
        kernel.ImportPluginFromType<TimePlugin>();

        return kernel;
    }
}

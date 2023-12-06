// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Core;
using Plugins;
using RepoUtils;

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
            GetPromptTemplate = () => s_promptYaml,
        };
        var planner = new FunctionCallingStepwisePlanner(config);

        foreach (var question in questions)
        {
            FunctionCallingStepwisePlannerResult result = await planner.ExecuteAsync(kernel, question);
            Console.WriteLine($"Q: {question}\nA: {result.FinalAnswer}");

            // You can uncomment the line below to see the planner's process for completing the request.
            // Console.WriteLine($"Chat history:\n{result.ChatHistory?.AsJson()}");
        }
    }

    /// <summary>
    /// Initialize the kernel and load plugins.
    /// </summary>
    /// <returns>A kernel instance</returns>
    private static Kernel InitializeKernel()
    {
        Kernel kernel = new KernelBuilder()
            .WithLoggerFactory(ConsoleLogger.LoggerFactory)
            .WithAzureOpenAIChatCompletion(
                TestConfiguration.AzureOpenAI.ChatDeploymentName,
                TestConfiguration.AzureOpenAI.ChatModelId,
                TestConfiguration.AzureOpenAI.Endpoint,
                TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        kernel.ImportPluginFromObject(new EmailPlugin(), "EmailPlugin");
        kernel.ImportPluginFromObject(new MathPlugin(), "MathPlugin");
        kernel.ImportPluginFromObject(new TimePlugin(), "TimePlugin");

        return kernel;
    }

    private static readonly string s_promptYaml = @"
template_format: semantic-kernel
template: |
  <message role=""system"">
  You are an expert at generating plans from a given GOAL. Think step by step and determine a plan to satisfy the specified GOAL using only the FUNCTIONS provided to you. You can also make use of your own knowledge while forming an answer but you must not use functions that are not provided. Once you have come to a final answer, use the UserInteraction_SendFinalAnswer function to communicate this back to the user.

  [FUNCTIONS]

  {{$available_functions}}

  [END FUNCTIONS]

  To create the plan, follow these steps:
  0. Each step should be something that is capable of being done by the list of available functions.
  1. Steps can use output from one or more previous steps as input, if appropriate.
  2. The plan should be as short as possible.
  </message>
  <message role=""user"">{{$goal}}</message>
description:     Generate a step-by-step plan to satisfy a given goal
name:            GeneratePlan
input_parameters:
  - name:          available_functions
    description:   A list of functions that can be used to generate the plan
  - name:          goal
    description:   The goal to satisfy
execution_settings:
  - model_id:          " + TestConfiguration.AzureOpenAI.ChatModelId + @"
    temperature:       0.0
    top_p:             0.0
    presence_penalty:  0.0
    frequency_penalty: 0.0
    max_tokens:        256
    stop_sequences:    []";
}

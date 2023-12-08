// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.SpeechRecognition;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using RepoUtils;
using Resources;

// ReSharper disable once InconsistentNaming
public static class Example73_SpeechRecognition
{
    /// <summary>
    /// Show how to combine multiple prompt template factories.
    /// </summary>
    public static async Task RunAsync()
    {
        string apiKey = TestConfiguration.AzureOpenAISpeech.ApiKey;
        string endpoint = TestConfiguration.AzureOpenAISpeech.Endpoint;
        string modelId = TestConfiguration.AzureOpenAISpeech.DeploymentName;
        string apiVersion = TestConfiguration.AzureOpenAISpeech.ApiVersion;

        /*if (apiKey == null || apiVersion == null || modelId == null || endpoint == null)
        {
            Console.WriteLine("Azure endpoint, apiKey, deploymentName or modelId not found. Skipping example.");
            return;
        }*/

        var kernel = new KernelBuilder()
            .WithLoggerFactory(ConsoleLogger.LoggerFactory)
            .WithAzureOpenAISpeechRecognition(
                modelId: modelId, // TODO: change to deploymentName
                endpoint: endpoint,
                apiKey: apiKey,
                apiVersion: apiVersion,
                serviceId: "Whisper")
            .Build();

        var whisperService = kernel.GetRequiredService<ISpeechRecognitionService>();

        var data = EmbeddedResource.ReadStream("SpeechRecognition.wikipediaOcelot.wav") ?? throw new FileNotFoundException("Missing required resource");

        var output = await whisperService.TranscribeAsync(data, kernel);
        Console.WriteLine(output);
    }
}

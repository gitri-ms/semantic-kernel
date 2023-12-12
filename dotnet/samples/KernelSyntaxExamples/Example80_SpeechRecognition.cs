// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SpeechRecognition;
using Resources;

// ReSharper disable once InconsistentNaming
public static class Example80_SpeechRecognition
{
    /// <summary>
    /// Show how to combine multiple prompt template factories.
    /// </summary>
    public static async Task RunAsync()
    {
        string apiKey = TestConfiguration.AzureOpenAISpeech.ApiKey;
        string endpoint = TestConfiguration.AzureOpenAISpeech.Endpoint;
        string deploymentName = TestConfiguration.AzureOpenAISpeech.DeploymentName;

        if (apiKey == null || deploymentName == null || endpoint == null)
        {
            Console.WriteLine("Azure endpoint, apiKey, or deploymentName not found. Skipping example.");
            return;
        }

        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAISpeechRecognition(
                deploymentName: deploymentName,
                endpoint: endpoint,
                apiKey: apiKey,
                serviceId: "Whisper")
            .Build();

        var whisperService = kernel.GetRequiredService<ISpeechRecognitionService>();

        var audioStream = EmbeddedResource.ReadStream("SpeechRecognition.wikipediaOcelot.wav") ?? throw new FileNotFoundException("Missing required resource");

        var output = await whisperService.GetTextFromSpeechAsync(audioStream, null, null, kernel);
        Console.WriteLine(output);
    }
}

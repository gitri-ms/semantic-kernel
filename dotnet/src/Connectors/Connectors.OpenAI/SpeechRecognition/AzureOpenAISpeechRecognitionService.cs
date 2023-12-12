// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.SpeechRecognition;
using System.IO;
using Azure.AI.OpenAI;

namespace Microsoft.SemanticKernel.Connectors.OpenAI;
public class AzureOpenAISpeechRecognitionService : ISpeechRecognitionService
{
    public AzureOpenAISpeechRecognitionService(
        string deploymentName,
        string endpoint,
        string apiKey,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(deploymentName);
        Verify.NotNullOrWhiteSpace(endpoint);
        Verify.NotNullOrWhiteSpace(apiKey);

        this._core = new(deploymentName, endpoint, apiKey, httpClient, loggerFactory?.CreateLogger(typeof(AzureOpenAISpeechRecognitionService)));
    }

    public AzureOpenAISpeechRecognitionService(
        string deploymentName,
        OpenAIClient openAIClient,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(deploymentName);

        this._core = new(deploymentName, openAIClient, loggerFactory?.CreateLogger(typeof(AzureOpenAISpeechRecognitionService)));
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> Attributes => this._core.Attributes;

    /// <inheritdoc/>
    public async Task<string> GetTextFromSpeechAsync(
        Stream audioStream,
        string? prompt = null,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        this._core.LogActionDetails();
        return await this._core.GetTextFromSpeechAsync(audioStream, prompt, executionSettings, kernel, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Core implementation shared by Azure OpenAI clients.</summary>
    private readonly AzureOpenAIClientCore _core;
}

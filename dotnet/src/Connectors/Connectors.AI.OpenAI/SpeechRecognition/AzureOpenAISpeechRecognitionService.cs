// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.SpeechRecognition;
using Microsoft.SemanticKernel.Http;
using System;
using System.IO;
using Microsoft.SemanticKernel.AI;

namespace Microsoft.SemanticKernel.Connectors.AI.OpenAI.SpeechRecognition;
public class AzureOpenAISpeechRecognitionService : ISpeechRecognitionService
{
    public AzureOpenAISpeechRecognitionService(
        string deploymentName,
        string endpoint,
        string apiKey,
        string apiVersion,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(deploymentName);
        Verify.NotNullOrWhiteSpace(endpoint);
        Verify.NotNullOrWhiteSpace(apiKey);

        /*this._deploymentName = deploymentName;
        this._endpoint = endpoint;
        this._apiKey = apiKey;
        this._apiVersion = apiVersion;
        this._httpClient = HttpClientProvider.GetHttpClient(httpClient);*/
        //this._attributes.Add(AIServiceExtensions.ModelIdKey, this._modelId);
        //this._attributes.Add(AIServiceExtensions.EndpointKey, this._endpoint);

        this._core = new(deploymentName, endpoint, apiKey, httpClient, loggerFactory?.CreateLogger(typeof(AzureOpenAISpeechRecognitionService)));
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
        /*using var httpRequestMessage = HttpRequest.CreatePostRequest(this.GetRequestUri());

        httpRequestMessage.Headers.Add("User-Agent", HttpHeaderValues.UserAgent);
        httpRequestMessage.Headers.Add("api-key", $"{this._apiKey}");
        //httpRequestMessage.Headers.Add("Content-Type", "multipart/form-data");

        using var formContent = new MultipartFormDataContent();
        formContent.Headers.ContentType.MediaType = "multipart/form-data";

        using var streamContent = new StreamContent(data);
        formContent.Add(streamContent);

        httpRequestMessage.Content = formContent;

        using var response = await this._httpClient.SendWithSuccessCheckAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringWithExceptionMappingAsync().ConfigureAwait(false);
        
        return body;*/

        this._core.LogActionDetails();
        return await this._core.GetTextFromSpeechAsync(audioStream, prompt, executionSettings, kernel, cancellationToken).ConfigureAwait(false);

        //return string.Empty;
    }

    /*private readonly string _deploymentName;
    private readonly string _endpoint;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiVersion;*/
    //private readonly Dictionary<string, object?> _attributes = new();

    /// <summary>Core implementation shared by Azure OpenAI clients.</summary>
    private readonly AzureOpenAIClientCore _core;

    /*private Uri GetRequestUri()
    {
        return new Uri($"{this._endpoint.TrimEnd('/')}/openai/deployments/{this._modelId}/audio/transcriptions?api-version={this._apiVersion}");
    }*/

}

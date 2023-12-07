// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.SpeechRecognition;

namespace Microsoft.SemanticKernel.Connectors.AI.OpenAI.SpeechRecognition;
public class AzureOpenAISpeechRecognitionService : ISpeechRecognitionService
{
    public AzureOpenAISpeechRecognitionService(
        string endpoint,
        string modelId,
        string apiKey,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {

    }
}

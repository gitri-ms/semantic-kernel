// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.Services;
using System.IO;

namespace Microsoft.SemanticKernel.AI.SpeechRecognition;
public interface ISpeechRecognitionService : IAIService
{
    Task<string> GetTextFromSpeechAsync(
        Stream data,
        string? prompt = null,
        PromptExecutionSettings? promptExecutionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default);
}

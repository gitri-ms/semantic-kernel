// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.Services;
using System.IO;

namespace Microsoft.SemanticKernel.SpeechRecognition;
public interface ISpeechRecognitionService : IAIService
{
    Task<string> GetTextFromSpeechAsync(
        Stream audioStream,
        string? prompt = null,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default);
}

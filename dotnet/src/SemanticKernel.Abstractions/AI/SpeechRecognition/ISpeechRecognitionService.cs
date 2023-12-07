// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.Services;

namespace Microsoft.SemanticKernel.AI.SpeechRecognition;
public interface ISpeechRecognitionService : IAIService
{
    Task<string> TranscribeAsync(
        string prompt,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default);
}

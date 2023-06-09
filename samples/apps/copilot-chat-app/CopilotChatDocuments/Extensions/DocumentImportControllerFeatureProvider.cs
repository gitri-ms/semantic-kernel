// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;
using SemanticKernel.Service.CopilotChat.Documents.Controllers;

namespace SemanticKernel.Service.CopilotChat.Documents.Extensions;

public class DocumentImportControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        return base.IsController(typeInfo) && typeInfo == typeof(DocumentImportController).GetTypeInfo();
    }
}

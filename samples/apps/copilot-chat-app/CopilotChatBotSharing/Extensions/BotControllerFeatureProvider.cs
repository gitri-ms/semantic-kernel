// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;
using SemanticKernel.Service.CopilotChat.BotSharing.Controllers;

namespace SemanticKernel.Service.CopilotChat.BotSharing.Extensions;

public class BotControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        return base.IsController(typeInfo) && typeInfo == typeof(BotController).GetTypeInfo();
    }
}

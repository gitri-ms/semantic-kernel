// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using SemanticKernel.Service.CopilotChat.BotSharing.Controllers;

namespace SemanticKernel.Service.CopilotChat.BotSharing.Extensions;

public class BotControllerFeatureProvider : CustomControllerFeatureProvider
{
    public BotControllerFeatureProvider() : base(new List<Type> { typeof(BotController) }) { }
}

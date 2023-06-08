// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernel.Service.CopilotChat.Extensions;

public static class MvcBuilderExtensions
{
    public static IMvcBuilder ConfigureCopilotChatControllers(this IMvcBuilder builder, IConfiguration configuration)
    {
        // TODO: get this list from somewhere - configuration? list of supported features?
        var supportedControllerTypes = new List<Type>
        {
            typeof(CopilotChat.Controllers.BotController),
            typeof(CopilotChat.Controllers.ChatController),
            typeof(CopilotChat.Controllers.ChatHistoryController),
            typeof(CopilotChat.Controllers.DocumentImportController),
            typeof(CopilotChat.Controllers.SpeechTokenController)
        };

        builder.ConfigureApplicationPartManager(mgr =>
        {
            // Override the default controller feature providers to allow us toi specify which controllers should be included.
            var defaultControllerFeatureProviders = mgr.FeatureProviders.OfType<ControllerFeatureProvider>().ToList();
            foreach (var provider in defaultControllerFeatureProviders)
            {
                mgr.FeatureProviders.Remove(provider);
            }
            mgr.FeatureProviders.Add(new CustomControllerFeatureProvider(supportedControllerTypes));
        });

        return builder;
    }

}

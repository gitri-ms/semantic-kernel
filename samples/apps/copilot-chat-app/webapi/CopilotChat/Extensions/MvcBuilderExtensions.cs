// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using SemanticKernel.Service.CopilotChat.Controllers;

namespace SemanticKernel.Service.CopilotChat.Extensions;

public static class MvcBuilderExtensions
{
    public static IMvcBuilder ConfigureCopilotChatRequiredControllers(this IMvcBuilder builder)
    {
        var requiredControllerTypes = new List<Type>
        {
            typeof(ChatController),
            typeof(ChatHistoryController),
            typeof(DocumentImportController),
            typeof(SpeechTokenController),
        };

        builder.RemoveDefaultControllers()
            .ConfigureApplicationPartManager(mgr =>
            {
                mgr.FeatureProviders.Add(new CustomControllerFeatureProvider(requiredControllerTypes));
            });
        return builder;
    }

    public static IMvcBuilder AddCopilotChatBotSharing(this IMvcBuilder builder)
    {
        // Enable the bot controller
        builder.ConfigureApplicationPartManager(mgr =>
        {
            mgr.FeatureProviders.Add(new BotControllerFeatureProvider());
        });
        return builder;
    }

    // Hypothetical "bot sharing" nuget could include Models/Bot.cs, Controllers/BotController.cs,
    // the BotControllerFeatureProvider class, and the AddBotSharing extension to IMvcBuilder.

    #region Private methods
    private static IMvcBuilder RemoveDefaultControllers(this IMvcBuilder builder)
    {
        builder.ConfigureApplicationPartManager(mgr =>
        {
            // Remove the default controller feature providers. This will allow us to later specify which controllers we want to enable.
            var defaultControllerFeatureProviders = mgr.FeatureProviders.OfType<ControllerFeatureProvider>().ToList();
            foreach (var provider in defaultControllerFeatureProviders)
            {
                mgr.FeatureProviders.Remove(provider);
            }
        });
        return builder;
    }
    #endregion
}

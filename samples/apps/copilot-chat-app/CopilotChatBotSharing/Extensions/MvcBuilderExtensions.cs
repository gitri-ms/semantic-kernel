// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernel.Service.CopilotChat.BotSharing.Extensions;

public static class MvcBuilderExtensions
{
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
    // the BotControllerFeatureProvider class, and the AddCopilotChatBotSharing extension above.

}

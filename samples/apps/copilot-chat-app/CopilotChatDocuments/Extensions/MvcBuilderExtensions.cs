// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernel.Service.CopilotChat.Documents.Extensions;

public static class MvcBuilderExtensions
{
    public static IMvcBuilder AddCopilotChatBotSharing(this IMvcBuilder builder)
    {
        // Enable the bot controller
        builder.ConfigureApplicationPartManager(mgr =>
        {
            mgr.FeatureProviders.Add(new DocumentImportControllerFeatureProvider());
        });
        return builder;
    }

}

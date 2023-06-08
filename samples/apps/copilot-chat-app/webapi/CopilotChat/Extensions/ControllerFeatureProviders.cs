using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using SemanticKernel.Service.CopilotChat.Controllers;

namespace SemanticKernel.Service.CopilotChat.Extensions;

public class CustomControllerFeatureProvider : ControllerFeatureProvider
{
    private readonly IEnumerable<TypeInfo> m_typeInfos;

    public CustomControllerFeatureProvider(IEnumerable<Type> types) => this.m_typeInfos = types.Select(t => t.GetTypeInfo());

    protected override bool IsController(TypeInfo typeInfo)
    {
        return base.IsController(typeInfo) && this.m_typeInfos.Contains(typeInfo);
    }
}

public class BotControllerFeatureProvider : CustomControllerFeatureProvider
{
    public BotControllerFeatureProvider() : base(new List<Type> { typeof(BotController) }) { }
}

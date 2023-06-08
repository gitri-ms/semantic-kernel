using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace SemanticKernel.Service.CopilotChat.Extensions;

public class CustomControllerFeatureProvider : ControllerFeatureProvider
{
    private readonly IEnumerable<Type> m_types;

    public CustomControllerFeatureProvider(IEnumerable<Type> types) => this.m_types = types;

    protected override bool IsController(TypeInfo typeInfo)
    {
        return base.IsController(typeInfo) && this.m_types.Contains(typeInfo.GetType());
    }
}

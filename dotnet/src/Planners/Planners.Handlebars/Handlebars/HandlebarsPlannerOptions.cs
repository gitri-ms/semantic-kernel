﻿// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace Microsoft.SemanticKernel.Planning.Handlebars;

/// <summary>
/// Configuration for Handlebars planner instances.
/// </summary>
public sealed class HandlebarsPlannerOptions : PlannerOptions
{
    /// <summary>
    /// The prompt execution settings to use for the planner.
    /// </summary>
    public PromptExecutionSettings? ExecutionSettings { get; set; }

    /// <summary>
    /// Gets or sets the last plan generated by the planner.
    /// </summary>
    public HandlebarsPlan? LastPlan { get; set; }

    /// <summary>
    /// Gets or sets the last error that occurred during planning.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether loops are allowed in the plan.
    /// </summary>
    public bool AllowLoops { get; set; } = true;

    /// <summary>
    /// The Handlebars prompt template options to use for the planner.
    /// </summary>
    public HandlebarsPromptTemplateOptions? PromptTemplateOptions { get; set; }

    /// <inheritdoc/>
    public override string NameDelimiter => this.PromptTemplateOptions?.PrefixSeparator ?? "-";

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlebarsPlannerOptions"/> class.
    /// </summary>
    public HandlebarsPlannerOptions(
        HandlebarsPlan? lastPlan = default,
        string? lastError = default,
        bool allowLoops = true
    )
    {
        this.LastPlan = lastPlan;
        this.LastError = lastError;
        this.AllowLoops = allowLoops;
    }
}

// Copyright (c) Microsoft. All rights reserved.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Xunit;

namespace SemanticKernel.IntegrationTests.Planners.Handlebars;

public sealed class HandlebarsPlanTests
{
    public HandlebarsPlanTests()
    {
        this._kernel = new();
        this._arguments = new() { ["input"] = Guid.NewGuid().ToString("X") };
    }

    private const string PlanTemplate =
    @"{{!-- Step 1: Call Bar function --}}  
{{set ""barResult"" (Foo-Bar)}}  

{{!-- Step 2: Call BazAsync function --}}  
{{set ""bazAsyncResult"" (Foo-Baz)}}

{{!-- Step 3: Call Combine function with two words --}}  
{{set ""combinedWords"" (Foo-Combine x=""Hello"" y=""World"")}}  

{{!-- Step 4: Call StringifyInt function with an integer --}}  
{{set ""stringifiedInt"" (Foo-StringifyInt x=42)}}  

{{!-- Step 5: Output the results --}}  
{{concat barResult bazAsyncResult combinedWords stringifiedInt}}";

    [Fact]
    public async Task InvokeValidPlanAsync()
    {
        // Arrange & Act  
        var result = await this.InvokePlanAsync(PlanTemplate);

        // Assert  
        Assert.Equal("BarBazWorldHello42", result);
    }

    [Fact]
    public async Task InvokePlanWithHallucinatedFunctionAsync()
    {
        // Arrange
        var planWithInvalidHelper = PlanTemplate.Replace("Foo-Combine", "Foo-HallucinatedHelper", StringComparison.CurrentCulture);

        // Act & Assert  
        var exception = await Assert.ThrowsAsync<KernelException>(async () => await this.InvokePlanAsync(planWithInvalidHelper));
        Assert.IsType<HandlebarsRuntimeException>(exception.InnerException);
        Assert.Contains("Template references a helper that cannot be resolved.", exception.InnerException.Message, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task InvokePlanWithParentNotationAsync()
    {
        var planWithParentNotation = @"";

        var result = await this.InvokePlanAsync(planWithParentNotation);
    }

    [Fact]
    public async Task InvokePlanWithObjectDotNotationAsync()
    {
        var planWithObjectDotNotation = @"";

        var result = await this.InvokePlanAsync(planWithObjectDotNotation);
    }

    [Fact]
    public async Task InvokePlanWithArrayNotationAsync()
    {
        var planWithArrayNotation = @"";

        var result = await this.InvokePlanAsync(planWithArrayNotation);
    }
    /*[1:32 PM] Matthew Bolanos
Now I'm wondering if using "../sum" also works (?) It'd be good to have some integration tests that include a few of the different path expressions in handlebars:
../ (parent notation)
coordinate.x (object dot notation)
arr.[0] (array notation)
Expressions | Handlebars
[1:34 PM] Matthew Bolanos
Ah! So this is how they fixed it...
If the argument is of type UndefinedBindingResult, it means that Handlebars attempted to retrieve the value for a binding but was unable to do so because the variable was not defined or not passed to the template context at the time of render. Thus, we try to get the value from the kernel arguments dictionary.
I think that makes sense... I wonder how that behaves when you combine it with path expressions.
[1:35 PM] Matthew Bolanos
For example...
 
{{set "arr" (range 0 10)}}
{{#each arr}}
    {{arr.[0]}}
{{/each}}
    */

    #region private

    private readonly Kernel _kernel;
    private readonly KernelArguments _arguments;

    private async Task<string> InvokePlanAsync(string planTemplate)
    {
        // Arrange
        this._kernel.ImportPluginFromObject(new Foo());
        var plan = new HandlebarsPlan(planTemplate);

        // Act
        return await plan.InvokeAsync(this._kernel, this._arguments);
    }

    private sealed class Foo
    {
        [KernelFunction, Description("Return Bar")]
        public string Bar() => "Bar";

        [KernelFunction, Description("Return Baz")]
        public async Task<string> BazAsync()
        {
            await Task.Delay(1000);
            return await Task.FromResult("Baz");
        }

        [KernelFunction, Description("Return words concatenated")]
        public string Combine([Description("First word")] string x, [Description("Second word")] string y) => y + x;

        [KernelFunction, Description("Return number as string")]
        public string StringifyInt([Description("Number to stringify")] int x) => x.ToString(CultureInfo.InvariantCulture);
    }

    #endregion
}

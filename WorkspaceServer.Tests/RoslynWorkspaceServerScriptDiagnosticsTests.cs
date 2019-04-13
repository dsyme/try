﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.DotNet.Try.Protocol;
using Microsoft.DotNet.Try.Protocol.Execution;
using WorkspaceServer.Servers.Roslyn;
using WorkspaceServer.Servers.Scripting;
using WorkspaceServer.Packaging;
using Xunit;
using Xunit.Abstractions;
using Package = WorkspaceServer.Packaging.Package;

namespace WorkspaceServer.Tests
{
    public class RoslynWorkspaceServerScriptDiagnosticsTests : WorkspaceServerTestsCore
    {
        public RoslynWorkspaceServerScriptDiagnosticsTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override Task<(ICodeRunner runner, Package workspace)> GetRunnerAndWorkspaceBuild(string testName = null)
        {
            return Task.FromResult(((ICodeRunner)new ScriptingWorkspaceServer(),(Package) new NonrebuildablePackage("script")));
        }

        [Fact]
        public async Task Get_diagnostics()
        {
            var code = @"addd";
            var (processed, markLocation) = CodeManipulation.ProcessMarkup(code);

            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("", processed, markLocation) });
            var request = new WorkspaceRequest(ws, activeBufferId: "");
            var server = GetLanguageService();
            var result = await server.GetDiagnostics(request);
            result.Diagnostics.Should().NotBeEmpty();
            result.Diagnostics.Should().Contain(diagnostics => diagnostics.Message == "(1,1): error CS0103: The name \'addd\' does not exist in the current context");
        }

        protected override ILanguageService GetLanguageService(
            [CallerMemberName] string testName = null) => new RoslynWorkspaceServer(
            PackageRegistry.CreateForHostedMode());
    }
}
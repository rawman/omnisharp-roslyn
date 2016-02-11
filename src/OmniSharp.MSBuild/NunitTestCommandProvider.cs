using System;
using System.IO;
using System.Collections.Generic;
using System.Composition;
using Microsoft.CodeAnalysis;
using OmniSharp.MSBuild.ProjectFile;

namespace OmniSharp.MSBuild
{
    [Export(typeof(ITestCommandProvider))]
    public class NunitTestCommandProvider : ITestCommandProvider
    {
        private MSBuildContext _context;

        [ImportingConstructor]
        public NunitTestCommandProvider(MSBuildContext context)
        {
            _context = context;
        }

        public string GetTestCommand(TestContext testContext)
        {
            if (!_context.Projects.ContainsKey(testContext.ProjectFile))
            {
                return null;
            }

            var projectInfo = _context.Projects[testContext.ProjectFile];
           
            
            var projectDir = new DirectoryInfo(testContext.ProjectFile).FullName;
            var assemblyFile = projectInfo.ProjectDirectory + "/bin/Debug/" + projectInfo.AssemblyName + ".dll"; 
             
           
            // Find the test command, if any and use that
            var symbol = testContext.Symbol;
            string arguments = "-nologo ";

            var containingNamespace = "";
            if (!symbol.ContainingNamespace.IsGlobalNamespace)
            {
                containingNamespace = symbol.ContainingNamespace + ".";
            }
            
            switch (testContext.TestCommandType)
            {
                case TestCommandType.Fixture:
                    if (symbol is IMethodSymbol)
                    {
                        arguments = containingNamespace + symbol.ContainingType.Name;
                    }
                    else if (symbol is INamedTypeSymbol)
                    {
                        arguments = containingNamespace + symbol.Name;
                    }
                    break;
                case TestCommandType.Single:
                    if (symbol is IMethodSymbol)
                    {
                        arguments = containingNamespace + symbol.ContainingType.Name + "." + symbol.Name;
                    }
                    else if (symbol is INamedTypeSymbol)
                    {
                        arguments = containingNamespace + symbol.Name;
                    }
                    break;
            }

            return "nunit " + assemblyFile + " -run=" + arguments;
        }
    }
}
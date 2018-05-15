using System;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer
{
    public abstract class GuidelineAnalyzer : DiagnosticAnalyzer
    {
        [NotNull]
        private static readonly CompilerVersionCompatibilityValidator VersionValidator =
            new CompilerVersionCompatibilityValidator();

        protected GuidelineAnalyzer()
        {
            if (!VersionValidator.IsCompatible())
            {
                throw new Exception(VersionValidator.GetMessage());
            }
        }

        private sealed class CompilerVersionCompatibilityValidator
        {
            [NotNull]
            private static readonly Version MinVersion = new Version(2, 6, 0, 0);

            [NotNull]
            private static readonly Lazy<VisualStudioVersionStatus> VersionStatusLazy;

            static CompilerVersionCompatibilityValidator()
            {
                VersionStatusLazy = new Lazy<VisualStudioVersionStatus>(GetVersionStatus, LazyThreadSafetyMode.PublicationOnly);
            }

            private static VisualStudioVersionStatus GetVersionStatus()
            {
                Version version = GetCompilerVersion();

                return version < MinVersion ? VisualStudioVersionStatus.TooLow : VisualStudioVersionStatus.Ok;
            }

            [NotNull]
            private static Version GetCompilerVersion()
            {
                return typeof(Compilation).GetTypeInfo().Assembly.GetName().Version;
            }

            public bool IsCompatible()
            {
                return VersionStatusLazy.Value == VisualStudioVersionStatus.Ok;
            }

            [CanBeNull]
            public string GetMessage()
            {
                return VersionStatusLazy.Value == VisualStudioVersionStatus.TooLow
                    ? "This analyzer package requires Visual Studio 2017 Update 5 or higher. Please upgrade to a newer version of Visual Studio."
                    : null;
            }

            private enum VisualStudioVersionStatus
            {
                Ok,
                TooLow
            }
        }
    }
}

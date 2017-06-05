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
            private static readonly Version MinVersion = new Version(1, 2, 0, 0);

            [NotNull]
            private static readonly Version MaxVersion = new Version(2, 2, 0, 0);

            private const string BaseMessage = "This analyzer package requires Visual Studio 2015 Update 2 - 2017 Update 2.";

            [NotNull]
            private static readonly Lazy<VisualStudioVersionStatus> VersionStatusLazy;

            static CompilerVersionCompatibilityValidator()
            {
                VersionStatusLazy = new Lazy<VisualStudioVersionStatus>(GetVersionStatus, LazyThreadSafetyMode.PublicationOnly);
            }

            private static VisualStudioVersionStatus GetVersionStatus()
            {
                Version version = GetCompilerVersion();

                return version < MinVersion
                    ? VisualStudioVersionStatus.TooLow
                    : version > MaxVersion
                        ? VisualStudioVersionStatus.TooHigh
                        : VisualStudioVersionStatus.Ok;
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
                switch (VersionStatusLazy.Value)
                {
                    case VisualStudioVersionStatus.TooLow:
                    {
                        return BaseMessage + " Please upgrade to a newer version of Visual Studio.";
                    }
                    case VisualStudioVersionStatus.TooHigh:
                    {
                        return BaseMessage + " Please upgrade to a newer version of CSharpGuidelinesAnalyzer.";
                    }
                    default:
                    {
                        return null;
                    }
                }
            }

            private enum VisualStudioVersionStatus
            {
                Ok,
                TooLow,
                TooHigh
            }
        }
    }
}

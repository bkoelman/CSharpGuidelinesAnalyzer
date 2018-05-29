using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer
{
    internal static class KnownTypes
    {
        [CanBeNull]
        public static INamedTypeSymbol SystemObject([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetSpecialType(SpecialType.System_Object);
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemBoolean([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetSpecialType(SpecialType.System_Boolean);
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemNullableT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetSpecialType(SpecialType.System_Nullable_T);
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemEventArgs([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.EventArgs");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemException([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Exception");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemSystemException([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.SystemException");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemApplicationException([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.ApplicationException");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemThreadingTasksTask([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemThreadingTasksTaskT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemRuntimeRemotingObjectHandle([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Runtime.Remoting.ObjectHandle");
        }
    }
}

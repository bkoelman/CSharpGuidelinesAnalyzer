using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    public static class OperationExtensions
    {
        [CanBeNull]
        public static IdentifierInfo TryGetIdentifierInfo([CanBeNull] this IOperation identifier)
        {
            var local = identifier as ILocalReferenceExpression;
            if (local != null)
            {
                if (local.Local.Name.Length == 0)
                {
                    return null;
                }

                return new IdentifierInfo(local.Local.Name,
                    local.Local.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), local.Local.Type,
                    "Variable");
            }

            var parameter = identifier as IParameterReferenceExpression;
            if (parameter != null)
            {
                if (parameter.Parameter.Name.Length == 0)
                {
                    return null;
                }

                return new IdentifierInfo(parameter.Parameter.Name,
                    /* CSharpShortErrorMessageFormat returns 'ref int', ie. without parameter name */
                    parameter.Parameter.Name, parameter.Parameter.Type, parameter.Parameter.Kind.ToString());
            }

            var field = identifier as IFieldReferenceExpression;
            if (field != null)
            {
                if (field.Field.Name.Length == 0)
                {
                    return null;
                }

                return new IdentifierInfo(field.Field.Name,
                    field.Field.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat), field.Field.Type,
                    field.Field.Kind.ToString());
            }

            var property = identifier as IPropertyReferenceExpression;
            if (property != null)
            {
                if (property.Property.Name.Length == 0)
                {
                    return null;
                }

                return new IdentifierInfo(property.Property.Name,
                    property.Property.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                    property.Property.Type, property.Property.Kind.ToString());
            }

            var method = identifier as IInvocationExpression;
            if (method != null)
            {
                if (method.TargetMethod.Name.Length == 0)
                {
                    return null;
                }

                return new IdentifierInfo(method.TargetMethod.Name,
                    method.TargetMethod.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                    method.TargetMethod.ReturnType, method.TargetMethod.Kind.ToString());
            }

            return null;
        }
    }
}
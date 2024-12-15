using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
public static class PropertyBitPackDiagnostics
{
    public static readonly DiagnosticDescriptor InvalidBitsCount = new(
        id: "PRBITS001",
        title: "Invalid BitsCount value",
        messageFormat: "The BitsCount for property '{0}' must be a positive integer",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MissingGetterLargeSizeValue = new(
        id: "PRBITS002",
        title: "Missing GetterLargeSizeValueName",
        messageFormat: "The property '{0}' requires 'GetterLargeSizeValueName' for ExtendedBitFieldAttribute",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MandatoryOfNameofInGetterLargeSizeValueName = new(
        id: "PRBITS003",
        title: "The 'GetterLargeSizeValueName' property requires 'nameof'",
        messageFormat: "The 'GetterLargeSizeValueName' for property '{0}' must use 'nameof' to reference a valid method or property",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: "The 'GetterLargeSizeValueName' in the SmallBitsToFlagAttribute must use 'nameof' to reference a valid method or property.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidReferenceInGetterLargeSizeValueName = new(
        id: "PRBITS004",
        title: "Invalid reference in 'GetterLargeSizeValueName'",
        messageFormat: "The 'GetterLargeSizeValueName' for property '{0}' must reference a valid property or method",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: "The 'GetterLargeSizeValueName' in the SmallBitsToFlagAttribute must reference a property or method. References to other symbols are not allowed.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidTypeForPropertyReference = new(
        id: "PRBITS005",
        title: "Invalid type for property reference in 'GetterLargeSizeValueName'",
        messageFormat: "The property '{0}' referenced in 'GetterLargeSizeValueName' has an invalid type. Expected type is '{1}'.",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: "The property referenced by 'GetterLargeSizeValueName' must have a type compatible with the expected bit field usage.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidReturnTypeForMethodReference = new(
        id: "PRBITS006",
        title: "Invalid return type for method reference in 'GetterLargeSizeValueName'",
        messageFormat: "The method '{0}' referenced in 'GetterLargeSizeValueName' has an invalid return type. Expected type is '{1}'.",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: "The method referenced by 'GetterLargeSizeValueName' must return a value compatible with the expected bit field usage.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MethodWithParametersNotAllowed = new(
        id: "PRBITS007",
        title: "Method with parameters is not allowed in 'GetterLargeSizeValueName'",
        messageFormat: "The method '{0}' referenced in 'GetterLargeSizeValueName' must not have any parameters",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: "The method referenced by 'GetterLargeSizeValueName' must be parameterless to ensure compatibility with bit field logic.",
        isEnabledByDefault: true
    );

}

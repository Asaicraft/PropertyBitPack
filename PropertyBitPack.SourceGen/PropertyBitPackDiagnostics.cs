﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
internal static class PropertyBitPackDiagnostics
{
    public static readonly DiagnosticDescriptor InvalidBitsCount = new(
        id: "PRBITS001",
        title: $"Invalid {nameof(BitFieldAttribute.BitsCount)} value",
        messageFormat: $"The {nameof(BitFieldAttribute.BitsCount)} for property '{{0}}' must be a positive integer",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MissingGetterLargeSizeValue = new(
        id: "PRBITS002",
        title: $"Missing {nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}",
        messageFormat: $"The property '{{0}}' requires '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' for  {nameof(IExtendedBitFieldAttribute)}",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MandatoryOfNameofInGetterLargeSizeValueName = new(
        id: "PRBITS003",
        title: $"The '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' property requires 'nameof'",
        messageFormat: $"The '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' for property '{{0}}' must use 'nameof' to reference a valid method or property",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: $"The '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' in the {nameof(IExtendedBitFieldAttribute)} must use 'nameof' to reference a valid method or property.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidReferenceInGetterLargeSizeValueName = new(
        id: "PRBITS004",
        title: $"Invalid reference in '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}'",
        messageFormat: $"The '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' for property '{{0}}' must reference a valid property or method",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: $"The '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' in the SmallBitsToFlagAttribute must reference a property or method. References to other symbols are not allowed.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidTypeForPropertyReference = new(
        id: "PRBITS005",
        title: $"Invalid type for property reference in '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}'",
        messageFormat: $"The property '{{0}}' referenced in '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' has an invalid type. Expected type is '{{1}}'.",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: $"The property referenced by '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' must have a type compatible with the expected bit field usage.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidReturnTypeForMethodReference = new(
        id: "PRBITS006",
        title: $"Invalid return type for method reference in '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}'",
        messageFormat: $"The method '{{0}}' referenced in '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' has an invalid return type. Expected type is '{{1}}'.",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: $"The method referenced by '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' must return a value compatible with the expected bit field usage.",
        isEnabledByDefault: true
    );


    public static readonly DiagnosticDescriptor MethodWithParametersNotAllowed = new(
        id: "PRBITS007",
        title: $"Method with parameters is not allowed in '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}'",
        messageFormat: $"The method '{{0}}' referenced in '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' must either have no parameters or only parameters with default values",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: $"The method referenced by '{nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName)}' must either be parameterless or have only parameters with default values to ensure compatibility with bit field logic.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor TooManyBitsForAnyType = new(
        id: "PRBITS008",
        title: $"Too many bits required",
        messageFormat: $"Properties with {nameof(BitFieldAttribute.FieldName)} '{{0}}' require {{1}} bits, which is more than the largest available type (64 bits)",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor UnsupportedOwnerType = new(
        id: "PRBITS009",
        title: $"Unsupported owner type",
        messageFormat: $"The owner type '{{0}}' is not supported. Only classes and structs are supported.",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidReferenceInFieldName = new(
        id: "PRBITS010",
        title: $"Invalid reference in '{nameof(BitFieldAttribute.FieldName)}'",
        messageFormat: $"The '{nameof(BitFieldAttribute.FieldName)}' for property '{{0}}' must reference a valid field when using the nameof operation",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor AttributeConflict = new(
        id: "PRBITS011",
        title: $"Conflict between attributes",
        messageFormat: $"Conflict between attributes: {{0}}. Choose only one of them.",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: $"Attributes causing conflict cannot be used together. Ensure only one attribute from the conflicting group is applied.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor TooManyBitsForSpecificType = new(
        id: "PRBITS012",
        title: $"Too many bits required for specific type",
        messageFormat: $"The field '{{0}}' requires '{{1}}' bits, which exceeds the capacity of type {{2}} that can hold a maximum of {{3}} bits",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: $"The field requires more bits than the specified type can accommodate. Ensure the bit count is within the allowable limit for the chosen type.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidReferenceToNonReadOnlyField = new(
        id: "PRBITS013",
        title: $"Invalid reference to non-readonly field in '{nameof(ReadOnlyBitFieldAttribute.FieldName)}'",
        messageFormat: $"The '{nameof(ReadOnlyBitFieldAttribute.FieldName)}' for property '{{0}}' must reference a readonly field when using the nameof operation",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: $"The '{nameof(ReadOnlyBitFieldAttribute.FieldName)}' in the {nameof(ReadOnlyBitFieldAttribute)} must reference a readonly field. Referencing mutable fields is not allowed.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor ReadOnlyPropertyRequiresNoSetterOrInitOnly = new(
        id: "PRBITS014",
        title: $"ReadOnlyBitFieldAttribute requires property without setter or with init-only setter",
        messageFormat: $"The property '{{0}}' with '{nameof(ReadOnlyBitFieldAttribute)}' must either be read-only or have an init-only setter",
        category: "PropertyBitPack",
        defaultSeverity: DiagnosticSeverity.Error,
        description: $"Properties with '{nameof(ReadOnlyBitFieldAttribute)}' must be read-only or have an init-only setter to ensure immutability.",
        isEnabledByDefault: true
    );

}

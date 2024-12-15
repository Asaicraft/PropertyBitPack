using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
public static class NoitraDiagnostics
{
    public static readonly DiagnosticDescriptor PropertyShoudbePartial = new(
        id: "NOITRA001",
        title: "Property should be partial",
        messageFormat: "Property should be partial",
        category: "Noitra",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor UnknowPropertyType = new(
        id: "NOITRA002",
        title: "Unknow property type",
        messageFormat: "Unknow property type",
        category: "Noitra",
        defaultSeverity: DiagnosticSeverity.Error,
        description: "The property type is not recognized, unknown bits count, you should set BitsCount.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MandatoryOfNameofInGetterLargeSizeValueName = new(
        id: "NOITRA003",
        title: "Mandatory of nameof in SmallBitsToFlagAttribute.GetterLargeSizeValueName",
        messageFormat: "Mandatory of nameof in SmallBitsToFlagAttribute.GetterLargeSizeValueName",
        category: "Noitra",
        defaultSeverity: DiagnosticSeverity.Error,
        description: "You should use nameof in SmallBitsToFlagAttribute.GetterLargeSizeValueName.",
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor GetterLargeSizeValueNameNameOfShoudReferenceToPropertyOrMethod = new(
        id: "NOITRA004",
        title: "SmallBitsToFlagAttribute.GetterLargeSizeValueName shoud have valid reference",
        messageFormat: "SmallBitsToFlagAttribute.GetterLargeSizeValueName shoud have valid reference",
        category: "Noitra",
        defaultSeverity: DiagnosticSeverity.Error,
        description: "SmallBitsToFlagAttribute.GetterLargeSizeValueName shoud reference to Method or Property.",
        isEnabledByDefault: true
    );
}

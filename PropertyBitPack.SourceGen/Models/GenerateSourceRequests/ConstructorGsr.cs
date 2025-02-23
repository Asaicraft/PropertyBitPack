using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.ConstructorRequests;
using PropertyBitPack.SourceGen.Models.FieldRequests;
using PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.GenerateSourceRequests;

internal sealed class ConstructorGsr(ImmutableArray<IFieldRequest> fieldRequests, ImmutableArray<BitFieldPropertyInfoRequest> properties, IConstructorRequest constructorRequest) : GenerateSourceRequest, IReadOnlyFieldGsr, IFileNameModifier
{
    public override ImmutableArray<IFieldRequest> Fields { get; } = fieldRequests;
    public override ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; } = properties;
    public IConstructorRequest ConstructorRequest { get; } = constructorRequest;

    public ConstructorGsr(IFieldRequest fieldRequest, ImmutableArray<BitFieldPropertyInfoRequest> properties, IConstructorRequest constructorRequest): this([fieldRequest], properties, constructorRequest)
    {
    }

    public void ModifyFileName(scoped ref readonly StringBuilder stringBuilder)
    {
        stringBuilder.Insert(0, "Constructor_");
    }
}

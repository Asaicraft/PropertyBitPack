using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.AttributeParsers;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.AttributeParsedResults;
using PropertyBitPack.SourceGen.Models.ConstructorRequests;
using PropertyBitPack.SourceGen.Models.FieldRequests;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using PropertyBitPack.SourceGen.Models.ParamRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
internal sealed class ReadOnlyAggregatorComponent
{

    public ImmutableArray<IReadOnlyFieldGsr> Aggregate(
        ILinkedList<BaseBitFieldPropertyInfo> properties,
        in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        var allReadyRequests = requestsBuilder.ToImmutable();

        using var readOnlyFieldGsrBuilder = new ImmutableArrayBuilder<IReadOnlyFieldGsr>();

        var grouped = Group(allReadyRequests);

        for (var i = 0; i < grouped.Length; i++)
        {
            var group = grouped[i];

            var constructor = CreateConstructor(group);

            readOnlyFieldGsrBuilder.Add(new ReadOnlyFieldGsr(group.Fields, group.Properties, constructor));
        }

        return readOnlyFieldGsrBuilder.ToImmutable();
    }

    private ConstructorRequest CreateConstructor(OwnerFieldInfo ownerFieldInfo)
    {
        var constructorAccessModifier = ownerFieldInfo.ConstructorAccessModifier;
        var parametersBuilder = ImmutableArrayBuilder<IParamRequest>.Rent(ownerFieldInfo.Properties.Length);

        for (var i = 0; i < ownerFieldInfo.Properties.Length; i++)
        {
            var property = ownerFieldInfo.Properties[i];
            var parameter = new ParamRequest(property.PropertyType.SpecialType, property.PropertyType.Name);
            parametersBuilder.Add(parameter);
        }

        var parameters = parametersBuilder.ToImmutable();

        return new ConstructorRequest(constructorAccessModifier, parameters);
    }

    private ImmutableArray<OwnerFieldInfo> Group(ImmutableArray<GenerateSourceRequest> allReadyRequests)
    {
        using var resultBuilder = ImmutableArrayBuilder<OwnerFieldInfo>.Rent();

        var groupedByOwner = SymbolAccessPairDictionaryPool.Rent();
        try
        {
            for (var i = 0; i < allReadyRequests.Length; i++)
            {
                var request = allReadyRequests[i];

                if(request.Properties.IsEmpty)
                {
                    continue;
                }

                if(request.Fields.IsEmpty)
                {
                    continue;
                }

                for (var i1 = 0; i1 < request.Properties.Length; i1++)
                {
                    var property = request.Properties[i1];
                    var owner = property.Owner;
                    var accessModifier = property.AttributeParsedResult is ParsedReadOnlyBitFieldAttribute parsedReadOnly 
                        ? parsedReadOnly.ConstructorAccessModifier
                        : AccessModifiers.Invalid;

                    if(accessModifier == AccessModifiers.Invalid)
                    {
                        Debug.WriteLine("Invalid access modifier");

                        if(Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }

                        continue;
                    }

                    var symbolAccessPair = new SymbolAccessPair(owner, accessModifier);

                    if (!groupedByOwner.TryGetValue(symbolAccessPair, out var list))
                    {
                        list = new FieldsPropertiesPair(
                            ListsPool<IFieldRequest>.Rent(),
                            ListsPool<BitFieldPropertyInfoRequest>.Rent()
                        );

                        groupedByOwner.Add(symbolAccessPair, list);
                    }

                    list.Properties.Add(property);
                    list.Fields.Add(property.BitsSpan.FieldRequest);

                }

            }

            foreach (var kvp in groupedByOwner)
            {
                var ownerSymbolAccessPair = kvp.Key;
                var fieldsPropertiesPair = kvp.Value;

                using var fieldsBuilder = ImmutableArrayBuilder<IFieldRequest>.Rent(fieldsPropertiesPair.Fields.Count);
                using var propertiesBuilder = ImmutableArrayBuilder<BitFieldPropertyInfoRequest>.Rent(fieldsPropertiesPair.Properties.Count);

                for (var i = 0; i < fieldsPropertiesPair.Fields.Count; i++)
                {
                    var field = fieldsPropertiesPair.Fields[i];
                    fieldsBuilder.Add(field);
                }

                for (var i = 0; i < fieldsPropertiesPair.Properties.Count; i++)
                {
                    var property = fieldsPropertiesPair.Properties[i];
                    propertiesBuilder.Add(property);
                }

                var fieldsImmutable = fieldsBuilder.ToImmutable();
                var propertiesImmutable = propertiesBuilder.ToImmutable();

                resultBuilder.Add(new OwnerFieldInfo(ownerSymbolAccessPair.Owner, ownerSymbolAccessPair.AccessModifier, fieldsImmutable, propertiesImmutable));
            }
        }
        finally
        {
            foreach (var kvp in groupedByOwner)
            {
                ListsPool<BitFieldPropertyInfoRequest>.Return(kvp.Value.Properties);
                ListsPool<IFieldRequest>.Return(kvp.Value.Fields);
            }

            SymbolAccessPairDictionaryPool.Return(groupedByOwner);
        }

        return resultBuilder.ToImmutable();
    }

    private class OwnerFieldInfo(
        INamedTypeSymbol ownerType,
        AccessModifier accessModifier,
        ImmutableArray<IFieldRequest> fields,
        ImmutableArray<BitFieldPropertyInfoRequest> properties)
    {
        public INamedTypeSymbol OwnerType
        {
            get;
        } = ownerType;

        public AccessModifier ConstructorAccessModifier
        {
            get;
        } = accessModifier;

        public ImmutableArray<IFieldRequest> Fields
        {
            get;
        } = fields;

        public ImmutableArray<BitFieldPropertyInfoRequest> Properties
        {
            get;
        } = properties;
    }


}

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
using static PropertyBitPack.SourceGen.BitFieldPropertyAggregators.ReadOnlyAggregatorComponent;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;

/// <summary>
/// Provides functionality to aggregate readonly field GSR objects from the provided requests.
/// </summary>
/// <remarks>
/// This component collects all necessary data from <see cref="IGenerateSourceRequest"/> instances,
/// groups them, and then constructs <see cref="IReadOnlyFieldGsr"/> objects that capture fields
/// and properties associated with specific owners.
/// </remarks>
internal sealed class ReadOnlyAggregatorComponent
{
    private readonly Func<OwnerFieldPropertyGroup, ConstructorRequest, IReadOnlyFieldGsr> _createReadOnlyFieldGsr;

    public ReadOnlyAggregatorComponent()
    {
        _createReadOnlyFieldGsr = static (group, constructor) => new ReadOnlyFieldGsr(group.Fields, group.Properties, constructor);
    }


    public ReadOnlyAggregatorComponent(Func<OwnerFieldPropertyGroup, ConstructorRequest, IReadOnlyFieldGsr> factory)
    {
        _createReadOnlyFieldGsr = factory;
    }

    /// <summary>
    /// Aggregates <see cref="IReadOnlyFieldGsr"/> instances based on the provided bit field properties
    /// and the collection of <see cref="IGenerateSourceRequest"/> objects.
    /// </summary>
    /// <param name="properties">
    /// A linked list of <see cref="BaseBitFieldPropertyInfo"/> representing bit field properties
    /// that can provide additional context for the aggregation process.
    /// </param>
    /// <param name="requestsBuilder">
    /// An <see cref="ImmutableArrayBuilder{T}"/> holding the <see cref="IGenerateSourceRequest"/> items
    /// that contain fields and properties for grouping.
    /// </param>
    /// <param name="diagnostics">
    /// An <see cref="ImmutableArrayBuilder{T}"/> for collecting any diagnostic information encountered
    /// during the aggregation process.
    /// </param>
    /// <returns>
    /// Returns an <see cref="ImmutableArray{T}"/> of <see cref="IReadOnlyFieldGsr"/> objects 
    /// that have been aggregated by owner and access modifier.
    /// </returns>
    public ImmutableArray<IReadOnlyFieldGsr> Aggregate(
        ILinkedList<BaseBitFieldPropertyInfo> properties,
        in ImmutableArrayBuilder<IGenerateSourceRequest> requestsBuilder,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        // Convert the builder to an immutable array of GenerateSourceRequest.
        var allReadyRequests = requestsBuilder.ToImmutable();

        // Use a builder to construct the final set of IReadOnlyFieldGsr objects.
        using var readOnlyFieldGsrBuilder = new ImmutableArrayBuilder<IReadOnlyFieldGsr>();

        // Group the requests to organize fields and properties by owner + access modifier.
        var grouped = Group(allReadyRequests);

        // For each group, create a corresponding IReadOnlyFieldGsr.
        for (var i = 0; i < grouped.Length; i++)
        {
            var group = grouped[i];

            // Create a constructor request for the grouped fields and properties.
            var constructor = CreateConstructor(group);
            var readOnlyFieldGsr = _createReadOnlyFieldGsr(group, constructor);

            // Add the newly created ReadOnlyFieldGsr to the builder.
            readOnlyFieldGsrBuilder.Add(
                readOnlyFieldGsr
            );
        }

        // Return the collected IReadOnlyFieldGsr objects as an immutable array.
        return readOnlyFieldGsrBuilder.ToImmutable();
    }

    /// <summary>
    /// Creates a <see cref="ConstructorRequest"/> based on the specified <see cref="OwnerFieldPropertyGroup"/>.
    /// </summary>
    /// <param name="ownerFieldInfo">
    /// The grouped information containing the owner, access modifier, fields, and bit field properties.
    /// </param>
    /// <returns>
    /// Returns a <see cref="ConstructorRequest"/> object containing the access modifier
    /// and parameters extracted from the properties.
    /// </returns>
    private ConstructorRequest CreateConstructor(OwnerFieldPropertyGroup ownerFieldInfo)
    {
        // Extract the constructor's access modifier from the owner's stored access information.
        var constructorAccessModifier = ownerFieldInfo.ConstructorAccessModifier;

        // Prepare a builder for the constructor's parameters, sized to the total properties count.
        var parametersBuilder = ImmutableArrayBuilder<IParamRequest>.Rent(ownerFieldInfo.Properties.Length);

        // For each property, create a parameter based on its type and name.
        for (var i = 0; i < ownerFieldInfo.Properties.Length; i++)
        {
            var property = ownerFieldInfo.Properties[i];
            var parameter = new ParamRequest(property.PropertyType.SpecialType, property.PropertyType.Name);
            parametersBuilder.Add(parameter);
        }

        // Convert the parameter builder into an immutable array.
        var parameters = parametersBuilder.ToImmutable();

        // Return the final ConstructorRequest.
        return new ConstructorRequest(constructorAccessModifier, parameters);
    }

    /// <summary>
    /// Groups the provided requests by their corresponding owner and access modifier,
    /// collecting both fields and properties into unified structures.
    /// </summary>
    /// <param name="allReadyRequests">
    /// An immutable array of <see cref="IGenerateSourceRequest"/> objects to be grouped.
    /// </param>
    /// <returns>
    /// Returns an <see cref="ImmutableArray{T}"/> of <see cref="OwnerFieldPropertyGroup"/> items,
    /// each containing the owner, the constructor access modifier, and the grouped fields and properties.
    /// </returns>
    private ImmutableArray<OwnerFieldPropertyGroup> Group(ImmutableArray<IGenerateSourceRequest> allReadyRequests)
    {
        // Rent a builder for the final list of OwnerFieldPropertyGroup objects.
        using var resultBuilder = ImmutableArrayBuilder<OwnerFieldPropertyGroup>.Rent();

        // Rent a dictionary from the pool, keyed by (INamedTypeSymbol Owner, AccessModifier AccessModifier).
        var groupedByOwner = SymbolAccessPairDictionaryPool.Rent();
        try
        {
            // Iterate over all requests.
            for (var i = 0; i < allReadyRequests.Length; i++)
            {
                var request = allReadyRequests[i];

                // If there are no properties or fields, skip this request.
                if (request.Properties.IsEmpty)
                {
                    continue;
                }

                if (request.Fields.IsEmpty)
                {
                    continue;
                }

                // For every property, determine the owner and constructor access modifier.
                for (var i1 = 0; i1 < request.Properties.Length; i1++)
                {
                    var property = request.Properties[i1];
                    var owner = property.Owner;
                    var accessModifier =
                        property.AttributeParsedResult is ParsedReadOnlyBitFieldAttribute parsedReadOnly
                            ? parsedReadOnly.ConstructorAccessModifier
                            : AccessModifiers.Invalid;

                    // If we detect an invalid access modifier, log and possibly break for debugging.
                    if (accessModifier == AccessModifiers.Invalid)
                    {
                        Debug.WriteLine("Invalid access modifier");

                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }

                        continue;
                    }

                    // Group by the symbolic owner and the associated access modifier.
                    var symbolAccessPair = new SymbolAccessPair(owner, accessModifier);

                    // If not found in the dictionary, create a new FieldsPropertiesPair with rented lists.
                    if (!groupedByOwner.TryGetValue(symbolAccessPair, out var list))
                    {
                        list = new FieldsPropertiesPair(
                            ListsPool<IFieldRequest>.Rent(),
                            ListsPool<BitFieldPropertyInfoRequest>.Rent()
                        );

                        groupedByOwner.Add(symbolAccessPair, list);
                    }

                    // Retrieve the field from the property and add them to their respective lists.
                    var field = property.BitsSpan.FieldRequest;

                    list.Properties.Add(property);

                    // Avoid adding the same field multiple times if it appears repeatedly.
                    if (!list.Fields.Contains(field))
                    {
                        list.Fields.Add(field);
                    }
                }
            }

            // Convert each entry in the dictionary to an OwnerFieldPropertyGroup object.
            foreach (var kvp in groupedByOwner)
            {
                var ownerSymbolAccessPair = kvp.Key;
                var fieldsPropertiesPair = kvp.Value;

                // Create builders for fields and properties.
                using var fieldsBuilder =
                    ImmutableArrayBuilder<IFieldRequest>.Rent(fieldsPropertiesPair.Fields.Count);
                using var propertiesBuilder =
                    ImmutableArrayBuilder<BitFieldPropertyInfoRequest>.Rent(fieldsPropertiesPair.Properties.Count);

                // Populate the fields builder.
                for (var i = 0; i < fieldsPropertiesPair.Fields.Count; i++)
                {
                    var field = fieldsPropertiesPair.Fields[i];
                    fieldsBuilder.Add(field);
                }

                // Populate the properties builder.
                for (var i = 0; i < fieldsPropertiesPair.Properties.Count; i++)
                {
                    var property = fieldsPropertiesPair.Properties[i];
                    propertiesBuilder.Add(property);
                }

                // Convert builders to immutable arrays.
                var fieldsImmutable = fieldsBuilder.ToImmutable();
                var propertiesImmutable = propertiesBuilder.ToImmutable();

                // Add a new OwnerFieldPropertyGroup record to the result builder.
                resultBuilder.Add(
                    new OwnerFieldPropertyGroup(
                        ownerSymbolAccessPair.Owner,
                        ownerSymbolAccessPair.AccessModifier,
                        fieldsImmutable,
                        propertiesImmutable
                    )
                );
            }
        }
        finally
        {
            // Return the rented lists to the pool.
            foreach (var kvp in groupedByOwner)
            {
                ListsPool<BitFieldPropertyInfoRequest>.Return(kvp.Value.Properties);
                ListsPool<IFieldRequest>.Return(kvp.Value.Fields);
            }

            // Return the dictionary to the pool.
            SymbolAccessPairDictionaryPool.Return(groupedByOwner);
        }

        // Produce the immutable array of OwnerFieldPropertyGroup.
        return resultBuilder.ToImmutable();
    }

    /// <summary>
    /// Represents information about a group of fields and properties associated with a particular owner and access modifier.
    /// </summary>`
    public class OwnerFieldPropertyGroup(
        INamedTypeSymbol ownerType,
        AccessModifier accessModifier,
        ImmutableArray<IFieldRequest> fields,
        ImmutableArray<BitFieldPropertyInfoRequest> properties)
    {
        /// <summary>
        /// Gets the symbol representing the type that owns these fields and properties.
        /// </summary>
        public INamedTypeSymbol OwnerType { get; } = ownerType;

        /// <summary>
        /// Gets the constructor access modifier that will be used when generating the constructor for this owner.
        /// </summary>
        public AccessModifier ConstructorAccessModifier { get; } = accessModifier;

        /// <summary>
        /// Gets an immutable array of field requests associated with the owner.
        /// </summary>
        public ImmutableArray<IFieldRequest> Fields { get; } = fields;

        /// <summary>
        /// Gets an immutable array of bit field property requests associated with the owner.
        /// </summary>
        public ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; } = properties;
    }
}
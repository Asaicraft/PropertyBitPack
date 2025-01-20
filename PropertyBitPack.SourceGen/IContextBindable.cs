using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;

/// <summary>
/// Provides a mechanism to bind a context to components that require access 
/// to the <see cref="PropertyBitPackGeneratorContext"/> for their operations.
/// </summary>
/// <remarks>
/// This interface is implemented by components such as parsers, aggregators, and 
/// syntax generators to establish a connection to the current generation context. 
/// The context provides shared resources, such as attribute parsers, property parsers, 
/// aggregators, and syntax generators, enabling coordinated processing of source generation requests.
/// </remarks>
internal interface IContextBindable
{
    /// <summary>
    /// Binds the specified <see cref="PropertyBitPackGeneratorContext"/> to the current component.
    /// </summary>
    /// <param name="context">
    /// The <see cref="PropertyBitPackGeneratorContext"/> instance to bind to, 
    /// which provides access to shared resources and configuration.
    /// </param>
    /// <remarks>
    /// Implementing classes typically use this method to initialize or configure themselves 
    /// with context-specific data. This method is called during the initialization phase 
    /// when the context is being constructed.
    /// </remarks>
    public void BindContext(PropertyBitPackGeneratorContext context);
}

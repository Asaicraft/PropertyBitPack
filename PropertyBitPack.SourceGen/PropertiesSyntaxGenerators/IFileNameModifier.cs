using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;

/// <summary>
/// Defines a contract for modifying file names using a <see cref="StringBuilder"/>.
/// </summary>
/// <remarks>
/// Implementations must not store a reference to the provided <see cref="StringBuilder"/>.
/// It should only be used within the scope of the method.
/// </remarks>
internal interface IFileNameModifier
{
    /// <summary>
    /// Modifies the given file name represented by a <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="stringBuilder">
    /// A <see cref="StringBuilder"/> containing the file name to be modified.
    /// The parameter is passed as <c>scoped ref readonly</c>, meaning it must be used
    /// only within the method and should not be stored or passed outside.
    /// </param>
    public void ModifyFileName(scoped ref readonly StringBuilder stringBuilder);
}

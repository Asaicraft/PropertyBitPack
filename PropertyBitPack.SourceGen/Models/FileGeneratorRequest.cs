using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal sealed record FileGeneratorRequest(SourceText SourceText, string FileName);
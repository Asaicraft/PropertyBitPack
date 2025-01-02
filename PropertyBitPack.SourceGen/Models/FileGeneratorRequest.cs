using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
public sealed record FileGeneratorRequest(SourceText SourceText, string FileName);
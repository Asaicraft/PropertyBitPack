﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;

public sealed class ExtendedBitFieldAttribute : BitsMappingAttributeBase
{
    public required string? GetterLargeSizeValueName
    {
        get; set;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;

#if PUBLIC_PACKAGE
public
#else
internal
#endif
interface IBitFieldAttribute
{
    byte BitsCount
    {
        get; set;
    }

    string? FieldName
    {
        get; set;
    }
}
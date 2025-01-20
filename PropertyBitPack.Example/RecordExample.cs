using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Example;
internal sealed partial record RecordExample
{
    [BitField(BitsCount = 4)]
    public partial byte Property1
    {
        get;
        init;
    }

    [BitField(BitsCount = 11)]
    public partial int Property2
    {
        get;
        init;
    }
}

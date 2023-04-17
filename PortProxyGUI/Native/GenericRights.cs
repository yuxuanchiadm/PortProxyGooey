using System;

namespace PortProxyGooey.Native
{
    [Flags]
    public enum GenericRights : uint
    {
        GENERIC_READ = 0x80000000,
    }
}

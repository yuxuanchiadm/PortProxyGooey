using System;

namespace PortProxyGooey.Native
{
    [Flags]
    public enum ServiceControls : uint
    {
        SERVICE_CONTROL_PARAMCHANGE = 0x00000006,
    }
}

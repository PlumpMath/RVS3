using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RobControls
{
    [Flags]
    public enum PositionEnum
    {
        Left = 0x01,
        Top = 0x02,
        Right = 0x04,
        Bottom = 0x08,
    }
}

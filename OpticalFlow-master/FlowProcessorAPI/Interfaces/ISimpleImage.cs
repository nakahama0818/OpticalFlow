﻿//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

using System.Drawing;

namespace FlowProcessorAPI.Interfaces
{
    public interface ISimpleImage
    {
        Bitmap Bitmap { get; }
    }
}

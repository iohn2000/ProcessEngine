﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.Shared.Enums
{
    public enum EnumTaskStatus
    {
        Completed,
        Wait,
        Timeout,
        ErrorStop,
        ErrorToHandle
    }
}
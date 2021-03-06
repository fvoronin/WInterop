﻿// ------------------------
//    WInterop Framework
// ------------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using WInterop.Handles.Types;

namespace WInterop.ProcessAndThreads.Types
{
    /// <summary>
    /// Safe handle for a thread.
    /// </summary>
    public class SafeThreadHandle : CloseHandle
    {
        public SafeThreadHandle() : base() { }
    }
}

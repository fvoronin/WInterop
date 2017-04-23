﻿// ------------------------
//    WInterop Framework
// ------------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using WInterop.Handles.Types;

namespace WInterop.SecurityManagement.Types
{
    public class SafeLsaHandle : SafeHandleZeroIsInvalid
    {
        public SafeLsaHandle() : base(ownsHandle: true) { }

        public SafeLsaHandle(IntPtr handle, bool ownsHandle = true) : base(ownsHandle)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            SecurityMethods.LsaClose(handle);
            return true;
        }
    }
}
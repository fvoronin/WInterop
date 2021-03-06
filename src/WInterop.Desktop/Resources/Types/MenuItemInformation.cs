﻿// ------------------------
//    WInterop Framework
// ------------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using WInterop.Gdi.Types;

namespace WInterop.Resources.Types
{
    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647578.aspx
    public struct MenuItemInformation
    {
        public MenuItemInfoMembers Mask;
        public MenuItemTypes Type;
        public MenuStates State;
        public uint Id;
        public MenuHandle SubMenu;
        public BitmapHandle Checked;
        public BitmapHandle Unchecked;
        public UIntPtr ItemData;
        public object TypeData;
        public uint Length;
        public BitmapHandle Item;
    }
}

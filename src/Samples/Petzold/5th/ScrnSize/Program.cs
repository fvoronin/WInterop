﻿// ------------------------
//    WInterop Framework
// ------------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using WInterop.Windows;
using WInterop.Windows.Types;

namespace ScrnSize
{
    /// <summary>
    /// Sample from Programming Windows, 5th Edition.
    /// Original (c) Charles Petzold, 1998
    /// Figure 2-3, Pages 37-38.
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            int cxScreen, cyScreen;

            cxScreen = Windows.GetSystemMetrics(SystemMetric.CXSCREEN);
            cyScreen = Windows.GetSystemMetrics(SystemMetric.CYSCREEN);

            Windows.MessageBox($"The screen is {cxScreen} pixels wide by {cyScreen} pixels high.", "ScrnSize");
        }
    }
}

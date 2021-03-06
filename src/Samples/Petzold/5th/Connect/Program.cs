﻿// ------------------------
//    WInterop Framework
// ------------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using WInterop.Extensions.WindowExtensions;
using WInterop.Gdi.Types;
using WInterop.Modules.Types;
using WInterop.Resources.Types;
using WInterop.Windows;
using WInterop.Windows.Types;

namespace Connect
{
    /// <summary>
    /// Sample from Programming Windows, 5th Edition.
    /// Original (c) Charles Petzold, 1998
    /// Figure 7-1, Pages 278-280.
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            const string szAppName = "Connect";

            ModuleInstance module = Marshal.GetHINSTANCE(typeof(Program).Module);
            WindowClass wndclass = new WindowClass
            {
                Style = ClassStyle.HorizontalRedraw | ClassStyle.VerticalRedraw,
                WindowProcedure = WindowProcedure,
                Instance = module,
                Icon = IconId.Application,
                Cursor = CursorId.Arrow,
                Background = StockBrush.White,
                ClassName = szAppName
            };

            Windows.RegisterClass(ref wndclass);

            WindowHandle window = Windows.CreateWindow(
                module,
                szAppName,
                "Connect-the-Points Mouse Demo",
                WindowStyles.OverlappedWindow);

            window.ShowWindow(ShowWindow.Normal);
            window.UpdateWindow();

            while (Windows.GetMessage(out MSG message))
            {
                Windows.TranslateMessage(ref message);
                Windows.DispatchMessage(ref message);
            }
        }

        const int MAXPOINTS = 1000;
        static POINT[] pt = new POINT[MAXPOINTS];
        static int iCount;
        static int sampleCount;
        const int TakeEvery = 10;

        static LRESULT WindowProcedure(WindowHandle window, WindowMessage message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                case WindowMessage.LeftButtonDown:
                    iCount = 0;
                    window.Invalidate(true);
                    return 0;
                case WindowMessage.MouseMove:
                    // Machines are way to fast to make this look interesting now, adding TakeEvery
                    if ((MouseKey)wParam == MouseKey.LeftButton && iCount < MAXPOINTS && (sampleCount++ % TakeEvery == 0))
                    {
                        pt[iCount].x = lParam.LowWord;
                        pt[iCount++].y = lParam.HighWord;

                        using (DeviceContext dc = window.GetDeviceContext())
                        {
                            dc.SetPixel(lParam.LowWord, lParam.HighWord, 0);
                        }
                    }
                    return 0;
                case WindowMessage.LeftButtonUp:
                    window.Invalidate(false);
                    return 0;
                case WindowMessage.Paint:
                    using (DeviceContext dc = window.BeginPaint())
                    {
                        Windows.SetCursor(CursorId.Wait);
                        Windows.ShowCursor(true);

                        for (int i = 0; i < iCount - 1; i++)
                            for (int j = i + 1; j < iCount; j++)
                            {
                                dc.MoveTo(pt[i].x, pt[i].y);
                                dc.LineTo(pt[j].x, pt[j].y);
                            }

                        Windows.ShowCursor(false);
                        Windows.SetCursor(CursorId.Arrow);
                    }
                    return 0;
                case WindowMessage.Destroy:
                    Windows.PostQuitMessage(0);
                    return 0;
            }

            return Windows.DefaultWindowProcedure(window, message, wParam, lParam);
        }
    }
}

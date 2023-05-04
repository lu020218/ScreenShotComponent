/*
 * 作者：赵忠印
 * 时间：2010-9-2 17:34:36
 * 描述：程序用到的Windows API
 * 参考：http://www.codeproject.com/KB/dialog/FindWindow.aspx
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections;

namespace ScreenShotComponent
{
    internal class WindowsAPI
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public Rectangle ToRectangle()
            {
                return new Rectangle(Left, Top, Right - Left, Bottom - Top);
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr handle, ref Point point);

        [DllImport("user32.dll")]
        public static extern IntPtr ChildWindowFromPointEx(IntPtr hWndParent, Point pt, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hwnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        public static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern Int32 ReleaseDC(IntPtr hWnd, IntPtr hdc);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int smIndex);

        public enum RopMode : int
        {
            R2_NOT = 6
        }
        [DllImport("gdi32.dll")]
        public static extern int SetROP2(IntPtr hdc, int fnDrawMode);

        public enum PenStyles : int
        {
            PS_INSIDEFRAME = 6
        }
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        public enum StockObjects : int
        {
            NULL_BRUSH = 5
        }
        [DllImport("gdi32.dll")]
        public static extern IntPtr GetStockObject(int fnObject);

        [DllImport("gdi32.dll")]
        public static extern uint Rectangle(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        // helper function return directly a Rectangle object
        public static Rectangle GetWindowRect(IntPtr hWnd)
        {
            //Debug.Assert(hWnd != IntPtr.Zero);
            RECT rect = new RECT();
            if (GetWindowRect(hWnd, ref rect) == false)
                throw new Exception("GetWindowRect failed");
            return rect.ToRectangle();
        }

        public enum GetSystem_Metrics : int
        {
            SM_CXBORDER = 5,
            SM_CXFULLSCREEN = 16,
            SM_CYFULLSCREEN = 17
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            StringBuilder WindowText = new StringBuilder(GetWindowTextLength(hWnd) + 1);
            GetWindowText(hWnd, WindowText, WindowText.Capacity);
            return WindowText.ToString();
        }

        /// <summary>
        /// 返回给定位置所在窗体
        /// </summary>
        /// <param name="point"></param>
        /// <returns>if return == IntPtr.Zero no window was found</returns>
        public static IntPtr ChildWindowFromPoint(Point point)
        {
            IntPtr WindowPoint = WindowsAPI.WindowFromPoint(point);
            if (WindowPoint == IntPtr.Zero)
                return IntPtr.Zero;

            if (WindowsAPI.ScreenToClient(WindowPoint, ref point) == false)
                throw new Exception("ScreenToClient failed");

            IntPtr Window = WindowsAPI.ChildWindowFromPointEx(WindowPoint, point, 0);
            if (Window == IntPtr.Zero)
                return WindowPoint;

            if (WindowsAPI.ClientToScreen(WindowPoint, ref point) == false)
                throw new Exception("ClientToScreen failed");

            if (WindowsAPI.IsChild(WindowsAPI.GetParent(Window), Window) == false)
                return Window;

            // create a list to hold all childs under the point
            ArrayList WindowList = new ArrayList();
            while (Window != IntPtr.Zero)
            {
                Rectangle rect = WindowsAPI.GetWindowRect(Window);
                if (rect.Contains(point))
                    WindowList.Add(Window);
                Window = WindowsAPI.GetWindow(Window, (uint)WindowsAPI.GetWindow_Cmd.GW_HWNDNEXT);
            }

            // search for the smallest window in the list
            int MinPixel = WindowsAPI.GetSystemMetrics((int)WindowsAPI.GetSystem_Metrics.SM_CXFULLSCREEN) * WindowsAPI.GetSystemMetrics((int)WindowsAPI.GetSystem_Metrics.SM_CYFULLSCREEN);
            for (int i = 0; i < WindowList.Count; ++i)
            {
                Rectangle rect = WindowsAPI.GetWindowRect((IntPtr)WindowList[i]);
                int ChildPixel = rect.Width * rect.Height;
                if (ChildPixel < MinPixel)
                {
                    MinPixel = ChildPixel;
                    Window = (IntPtr)WindowList[i];
                }
            }
            return Window;
        }

        /// <summary>
        /// 显示窗体边框
        /// </summary>
        /// <param name="window"></param>
        public static void ShowInvertRectTracker(IntPtr window)
        {
            if (window != IntPtr.Zero)
            {
                // get the coordinates from the window on the screen
                Rectangle WindowRect = GetWindowRect(window);
                // get the window's device context
                IntPtr dc = GetWindowDC(window);

                // Create an inverse pen that is the size of the window border
                SetROP2(dc, (int)RopMode.R2_NOT);

                Color color = Color.FromArgb(255, 0 ,0);
                IntPtr Pen = CreatePen((int)PenStyles.PS_INSIDEFRAME, 3 * GetSystemMetrics((int)GetSystem_Metrics.SM_CXBORDER), (uint)color.ToArgb());

                // Draw the rectangle around the window
                IntPtr OldPen = SelectObject(dc, Pen);
                IntPtr OldBrush = SelectObject(dc, GetStockObject((int)StockObjects.NULL_BRUSH));
                Rectangle(dc, 0, 0, WindowRect.Width, WindowRect.Height);

                SelectObject(dc, OldBrush);
                SelectObject(dc, OldPen);

                //release the device context, and destroy the pen
                ReleaseDC(window, dc);
                DeleteObject(Pen);
            }
        }
    }
}

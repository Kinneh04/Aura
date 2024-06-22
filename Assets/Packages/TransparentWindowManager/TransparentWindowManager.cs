using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;

public class TransparentWindowManager : SingletonMonoBehaviour<TransparentWindowManager>
{
    #region Enum

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    #endregion Enum

    #region Struct

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    #endregion Struct

    #region DLL Import

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    const int GWL_EXSTYLE = -20;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    const uint LWA_COLORKEY = 0x00000001;

    IntPtr hWnd;
    #endregion DLL Import

    #region Method

    // CAUTION:
    // To control enable or disable, use Start method instead of Awake.

    protected virtual void Start()
    {
        //   #if !UNITY_EDITOR && UNITY_STANDALONE_WIN

        //const int  GWL_STYLE = -16;
        //const uint WS_POPUP = 0x80000000;
        //const uint WS_VISIBLE = 0x10000000;

        //// NOTE:
        //// https://msdn.microsoft.com/ja-jp/library/cc410861.aspx

        //IntPtr windowHandle = GetActiveWindow();

        //// NOTE:
        //// https://msdn.microsoft.com/ja-jp/library/cc411203.aspx
        //// 
        //// "SetWindowLong" is used to update window parameter.
        //// The arguments shows (target, parameter, value).

        //SetWindowLong(windowHandle, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        //// NOTE:
        //// https://msdn.microsoft.com/ja-jp/library/windows/desktop/aa969512(v=vs.85).aspx
        //// https://msdn.microsoft.com/ja-jp/library/windows/desktop/bb773244(v=vs.85).aspx
        //// 
        //// DwmExtendFrameIntoClientArea will spread the effects
        //// which attached to window frame to contents area.
        //// So if the frame is transparent, the contents area gets also transparent.
        //// MARGINS is structure to set the spread range.
        //// When set -1 to MARGIN, it means spread range is all of the contents area.

        //MARGINS margins = new MARGINS()
        //{
        //    cxLeftWidth = -1
        //};

        //DwmExtendFrameIntoClientArea(windowHandle, ref margins);

        //#endif // !UNITY_EDITOR && UNITY_STANDALONE_WIN

#if !UNITY_EDITOR

        hWnd = GetActiveWindow();

        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
       
        SetLayeredWindowAttributes(hWnd, 0, 0, LWA_COLORKEY);
         SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0);

         Application.runInBackground = true;
#endif

    }

    private void Update()
    {
      //  SetClickThrough(Physics2D.OverlapPoint(CodeMonkey.Utils.UtilsClass.GetMouseWorldPosition()) == null);
    }

    private void SetClickThrough(bool clickthrough)
    {
        if (clickthrough)
        {
            SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
        else
        {
            SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED );
        }
    }

#endregion Method
}
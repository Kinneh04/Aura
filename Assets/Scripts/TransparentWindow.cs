using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class TransparentWindow : MonoBehaviour
{
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
  
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("Dwmapi.dll")]
    private static extern uint DwnExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    private void Start()
    {


        //  MessageBox(new IntPtr(0), "Hi", "Test", 0);
        IntPtr hWnd = GetActiveWindow();
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwnExtendFrameIntoClientArea(hWnd, ref margins);
    }



}

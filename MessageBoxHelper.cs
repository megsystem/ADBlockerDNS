using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class MessageBoxHelper
{
    private const int WH_CBT = 5;
    private const int HCBT_ACTIVATE = 5;

    private static IntPtr hHook = IntPtr.Zero;
    private static IntPtr hOwner = IntPtr.Zero;
    private static HookProc hookProc;

    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hHook);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern int GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    // Corrected: GetCurrentThreadId is in kernel32.dll, not user32.dll
    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    /// <summary>
    /// Shows a MessageBox centered relative to the specified owner form.
    /// </summary>
    public static DialogResult ShowMessageBoxAtOwner(Form owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        // Store the owner handle to be used in the hook.
        hOwner = owner.Handle;
        hookProc = new HookProc(MessageBoxHookProc);
        // Set the hook on the current thread.
        hHook = SetWindowsHookEx(WH_CBT, hookProc, IntPtr.Zero, GetCurrentThreadId());

        DialogResult result = MessageBox.Show(owner, text, caption, buttons, icon);

        // Clean up hook if it hasn't been unhooked already.
        if (hHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(hHook);
            hHook = IntPtr.Zero;
        }

        return result;
    }

    private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        // When the message box is activated, reposition it.
        if (nCode == HCBT_ACTIVATE)
        {
            // Get the owner form's screen rectangle.
            RECT ownerRect = new RECT();
            GetWindowRect(hOwner, ref ownerRect);

            // Get the message box window handle.
            IntPtr msgBoxHandle = wParam;

            // Get the message box's rectangle.
            RECT msgBoxRect = new RECT();
            GetWindowRect(msgBoxHandle, ref msgBoxRect);

            int ownerWidth = ownerRect.Right - ownerRect.Left;
            int ownerHeight = ownerRect.Bottom - ownerRect.Top;
            int msgBoxWidth = msgBoxRect.Right - msgBoxRect.Left;
            int msgBoxHeight = msgBoxRect.Bottom - msgBoxRect.Top;

            // Calculate position to center the MessageBox on the owner form.
            int posX = ownerRect.Left + (ownerWidth - msgBoxWidth) / 2;
            int posY = ownerRect.Top + (ownerHeight - msgBoxHeight) / 2;

            // Reposition the MessageBox.
            MoveWindow(msgBoxHandle, posX, posY, msgBoxWidth, msgBoxHeight, false);

            // Unhook immediately after repositioning.
            UnhookWindowsHookEx(hHook);
            hHook = IntPtr.Zero;
        }
        return IntPtr.Zero;
    }
}
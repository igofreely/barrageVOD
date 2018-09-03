using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace WinApiRemoteLib
{
    public static class SendKeysInterop
    {
        private const int WmKeydown = 0x100;
        private const int WmKeyup = 0x101;
        private const int WmSysKeydown = 0x0104;
        private const int WmSysKeyup = 0x0105;
        private const int VK_SHIFT = 0x010;
        private const int KEYEVENTF_KEYUP = 0x02; 
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        internal static extern bool PostMessage(IntPtr handle, uint msg, int wParam, int lParam);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        public static void SendCtrlC(IntPtr hWnd)
        {
            uint KEYEVENTF_KEYUP = 2;
            byte VK_CONTROL = 0x11;
            SetForegroundWindow(hWnd);
            keybd_event(VK_SHIFT, 0, 0, 0);
            //System.Threading.Thread.Sleep(100);
            keybd_event(37, 0, 0, 0); //Send the C key (43 is "C") 39=right arrow
            System.Threading.Thread.Sleep(100);
            keybd_event(37, 0, KEYEVENTF_KEYUP, 0);
            //System.Threading.Thread.Sleep(100);
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);// 'Left Control Up
        }

        public static bool PressShiftKey0(IntPtr handle, Key key)
        {
            keybd_event(VK_SHIFT, 0, 0, 0); //' 模拟按下SHIFT键，&H2A是VK_SHIFT的扫描码
            PostMessage(handle, WmKeydown, KeyInterop.VirtualKeyFromKey(key), 0);// ' 模拟按下 A 键，SHIFT+A产生一个大写A字符
            PostMessage(handle, WmKeyup, KeyInterop.VirtualKeyFromKey(key), 0);  //' 模拟抬起 A 键
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);  // ' 模拟抬起 SHIFT 键
            return PostMessage(handle, WmKeydown, KeyInterop.VirtualKeyFromKey(key), 0);
        }

        public static bool PressKey(IntPtr handle, Key key)
        {
            return PostMessage(handle, WmKeydown, KeyInterop.VirtualKeyFromKey(key), 0);
        }

        public static bool ReleaseKey(IntPtr handle, Key key)
        {
            return PostMessage(handle, WmKeyup, KeyInterop.VirtualKeyFromKey(key), 0);
        }

        public static bool HoldKeyDown(IntPtr handle, Key key)
        {
            return PostMessage(handle, WmSysKeydown, KeyInterop.VirtualKeyFromKey(key), 0);
        }

        public static bool ReleaseKeyUp(IntPtr handle, Key key)
        {
            return PostMessage(handle, WmSysKeyup, KeyInterop.VirtualKeyFromKey(key), 0);
        }

        public static void PressKeys(IntPtr handle, IEnumerable<Key> keys)
        {
            foreach (Key key in keys)
            {
                PressKey(handle, key);
            }
        }
    }
}
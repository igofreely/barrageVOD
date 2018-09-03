using System.Windows.Forms;
using System.Windows.Input;

namespace WinApiRemoteLib
{
    public class CommandRemoteBase
    {
        protected readonly ProcessWindow Window;
        protected readonly WindowFocuser Focuser;

        public CommandRemoteBase(ProcessWindow window)
        {
            Window = window;
            Focuser = new WindowFocuser();
        }

        public void Command(Key key)
        {
            Focuser.FocusByHandle(Window.Handle, WindowMode.Restore);
            SendKeysInterop.PressKey(Window.Handle, key);
        }
        public void Command(Key key1, Key key2)
        {
            Focuser.FocusByHandle(Window.Handle, WindowMode.Restore);

            SendKeysInterop.PressKey(Window.Handle, key2);
            SendKeysInterop.PressKey(Window.Handle, key1);
            //SendKeysInterop.ReleaseKeyUp(Window.Handle, key2);
        }
        public void Command2(Key key)
        {
            Focuser.FocusByHandle(Window.Handle, WindowMode.Restore);
            SendKeysInterop.SendCtrlC(Window.Handle);
            //SendKeysInterop.ReleaseKeyUp(Window.Handle, key2);
        }
        public void Command(Key key1, Key key2, Key key3)
        {
            Focuser.FocusByHandle(Window.Handle, WindowMode.Restore);
            SendKeysInterop.HoldKeyDown(Window.Handle, key2);
            SendKeysInterop.HoldKeyDown(Window.Handle, key3);
            SendKeysInterop.PressKey(Window.Handle, key1);
            SendKeysInterop.ReleaseKeyUp(Window.Handle, key2);
            SendKeysInterop.ReleaseKeyUp(Window.Handle, key3);
        }
        public void CommandSequence(string keys)
        {
            Focuser.FocusByHandle(Window.Handle, WindowMode.Restore);
            SendKeys.SendWait(keys);
        }
    }
}
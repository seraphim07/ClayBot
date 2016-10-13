﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClayBot
{
    partial class MainForm : Form
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private IntPtr hhk;
        private LowLevelKeyboardProc callbackDelegate;

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                switch ((Keys)Marshal.ReadInt32(lParam))
                {
                    case Keys.Escape:
                        UnhookWindowsHookEx(hhk);
                        Application.Exit();
                        break;
                    case Keys.F12:
                        Pause();
                        break;
                }
            }

            return CallNextHookEx(hhk, nCode, wParam, lParam);
        }

        private void InitializeInput()
        {
            callbackDelegate = new LowLevelKeyboardProc(HookCallback);
            hhk = SetWindowsHookEx(
                WH_KEYBOARD_LL,
                callbackDelegate,
                GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
                0);
        }

        private void Pause()
        {
            Invoke((MethodInvoker)delegate
            {
                mainWorker.Quit();
                workerThread.Join();

                ShowConfigForm();

                InitializeWorker();
            });
        }

        #region P/Invoke
        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int hookType, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);
        #endregion
    }
}
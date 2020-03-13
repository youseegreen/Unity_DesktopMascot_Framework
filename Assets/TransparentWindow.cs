using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class TransparentWindow : MonoBehaviour {
	public Vector2 windowSize = new Vector2(1000, 1000);


#if !UNITY_EDITOR && UNITY_STANDALONE_WIN

	#region WINDOWS API
	private struct MARGINS {
		public int cxLeftWidth;
		public int cxRightWidth;
		public int cyTopHeight;
		public int cyBottomHeight;
	}

	[DllImport("User32.dll")]
	private static extern IntPtr GetActiveWindow();
	[DllImport("User32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
	[DllImport("User32.dll")]
	private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
	[DllImport("Dwmapi.dll")]
	private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

	// 非アクティブ時にアクティブ化するための関数たち
	[DllImport("user32.dll", SetLastError = true)]
	static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
	[DllImport("user32.dll")]
	static extern IntPtr GetForegroundWindow();
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool SetForegroundWindow(IntPtr hWnd);
	[DllImport("user32.dll", SetLastError = true)]
	static extern bool BringWindowToTop(IntPtr hWnd);
	[DllImport("user32.dll")]
	extern static bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
	[return: MarshalAs(UnmanagedType.Bool)]
	[DllImport("user32.dll", SetLastError = true)]
	extern static bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	//タスクバーの高さを入手
	[DllImport("USER32.DLL", CharSet = CharSet.Auto)]
	public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
	[StructLayout(LayoutKind.Sequential)]
	private struct RECT {
		public int left;
		public int top;
		public int right;
		public int bottom;
	}
	#endregion

	IntPtr windowHandle;

	private void Awake() {

		//タスクバーをさがす
		IntPtr taskHandle = FindWindow("Shell_TrayWnd", null);
		RECT rect;
		bool flag = GetWindowRect(taskHandle, out rect);
		int height = rect.bottom - rect.top;

		Screen.SetResolution((int)(windowSize.x), (int)(windowSize.y), false, 60);
		Resolution res = Screen.currentResolution;
		windowHandle = GetActiveWindow();
		{ // SetWindowLong
			const int GWL_STYLE = -16;
			const int GWL_EXSTYLE = -20;
			const uint WS_POPUP = 0x80000000;
			const uint WS_EX_LAYERD = 0x080000;
			const uint WS_EX_TRANSPARENT = 0x00000020;

			SetWindowLong(windowHandle, GWL_STYLE, WS_POPUP);
			SetWindowLong(windowHandle, GWL_EXSTYLE, WS_EX_LAYERD | WS_EX_TRANSPARENT);
		}

		{ // SetWindowPos
			IntPtr HWND_TOPMOST = new IntPtr(-1);
			const uint SWP_NOSIZE = 0x0001;
			const uint SWP_NOACTIVE = 0x0010;
			const uint SWP_SHOWWINDOW = 0x0040;

			//    SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVE | SWP_SHOWWINDOW);
			SetWindowPos(windowHandle, HWND_TOPMOST, (int)(res.width - windowSize.x), (int)(res.height - height - windowSize.y),
							0, 0, SWP_NOSIZE | SWP_NOACTIVE | SWP_SHOWWINDOW);
		}

		{ // DwmExtendFrameIntoClientArea
			MARGINS margins = new MARGINS() {
				cxLeftWidth = -1
			};

			DwmExtendFrameIntoClientArea(windowHandle, ref margins);
		}
	}

	public void ActivateWindow() {
		uint nullProcessId = 0;
		uint targetThreadId = GetWindowThreadProcessId(windowHandle, out nullProcessId);
		uint currentActiveThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out nullProcessId);

		SetForegroundWindow(windowHandle);
		if (targetThreadId == currentActiveThreadId) {
			BringWindowToTop(windowHandle);
		}
		else {
			AttachThreadInput(targetThreadId, currentActiveThreadId, true);
			try {
				BringWindowToTop(windowHandle);
			}
			finally {
				AttachThreadInput(targetThreadId, currentActiveThreadId, false);
			}
		}
	}
#endif // !UNITY_EDITOR && UNITY_STANDALONE_WIN
}
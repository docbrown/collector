#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' \
	version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")

#include "resources.rc"

#include <stdexcept>
#include <unordered_set>

#include <Windows.h>
#include <CommCtrl.h>
#include <dbt.h>
#include <Shlwapi.h>
#include <ShlObj.h>
#include <ShObjIdl_core.h>
#include <propkey.h>
#include <propvarutil.h>
#include <strsafe.h>
#include <pathcch.h>
#include <atlbase.h>

#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Storage.Streams.h>
#include <winrt/Windows.Data.Json.h>
#include <winrt/Windows.Devices.PointOfService.h>

using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::Storage::Streams;
using namespace winrt::Windows::Data::Json;
using namespace winrt::Windows::Devices::PointOfService;

static GUID PosBarcodeScannerClassGuid = { 0xC243FFBD, 0x3AFC, 0x45E9, { 0xB3, 0xD3, 0x2B, 0xA1, 0x8B, 0xC7, 0xEB, 0xC5 } };

HINSTANCE g_hInstance;
WCHAR g_szLogFilePath[MAX_PATH];
HANDLE g_hLogFile = INVALID_HANDLE_VALUE;
HWND g_hWnd;
HMENU g_hniMenu;
NOTIFYICONDATAW g_niData;
HWINEVENTHOOK g_hWinEvent;
ClaimedBarcodeScanner g_ClaimedScanner{ nullptr };
BOOL g_InitializingScanner = FALSE;
std::unordered_set<HWND> g_Subscribers;

#define WINDOW_CLASS L"Collector.BarcodeScanner"

static UINT WM_BARCODEHELLO;
static UINT WM_BARCODEGOODBYE;
#define WM_BARCODESUBSCRIBE   (WM_USER + 1)
#define WM_BARCODEUNSUBSCRIBE (WM_USER + 2)
#define WM_COPYDATA_BARCODE   0xDEC0DED1

#define WM_NOTIFYICON          (WM_APP + 1)
#define WM_MYDEVICECHANGE      (WM_APP + 2)
#define WM_SCANNERCLOSED       (WM_APP + 3)
#define WM_SCANNERDATARECEIVED (WM_APP + 4)
#define WM_SCANNERCLAIMED      (WM_APP + 5)
#define WM_SCANNERENABLED      (WM_APP + 6)

HRESULT InitializeLogging();
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
void CALLBACK WinEventProc(HWINEVENTHOOK hWinEventHook, DWORD event, HWND hwnd,
	LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime);
void SetKeyboardInput(LPINPUT lpInput, WORD wCode, DWORD dwFlags);
BOOL DoRegisterDeviceNotification(HWND hWnd, HDEVNOTIFY* phDeviceNotify);
void SubscribeWindow(HWND hWnd);
BOOL UnsubscribeWindow(HWND hWnd);
void OnDeviceArrival(PDEV_BROADCAST_DEVICEINTERFACE pDeviceInterface);
void OnDeviceRemoveComplete(PDEV_BROADCAST_DEVICEINTERFACE pDeviceInterface);
void OnBarcodeScannerGetDefaultAsyncCompleted(IAsyncOperation<BarcodeScanner> const&, AsyncStatus const);
void OnBarcodeScannerFromIdAsyncCompleted(IAsyncOperation<BarcodeScanner> const&, AsyncStatus const);
void OnBarcodeScannerClaimScannerAsyncCompleted(IAsyncOperation<ClaimedBarcodeScanner> const&, AsyncStatus const);
void OnClaimedBarcodeScannerEnableAsyncCompleted(IAsyncAction const&, AsyncStatus const);
void OnClaimedBarcodeScannerReleaseDeviceRequested(IInspectable const&, ClaimedBarcodeScanner const&);
void OnClaimedBarcodeScannerClosed(ClaimedBarcodeScanner, ClaimedBarcodeScannerClosedEventArgs const&);
void OnClaimedBarcodeScannerDataReceived(ClaimedBarcodeScanner, BarcodeScannerDataReceivedEventArgs const&);
void SendScannerReportToWindow(BarcodeScannerReport Report, HWND hWnd);
void SetVirtualKeystroke(LPINPUT pInputs, WORD wVk);
void SetUnicodeKeystroke(LPINPUT pInputs, WORD wScan);
void SimulateKeyboardInput(BarcodeScannerReport Report);
BOOL WINAPI GetWindowThreadProcessInformationW(HWND hWnd, LPDWORD lpdwThreadId, LPDWORD lpdwProcessId, LPWSTR* lpExeName);
LPWSTR WINAPI CryptBinaryToStringAllocW(LPCBYTE pbBinary, DWORD cbBinary, DWORD dwFlags);
LRESULT WINAPI SendCopyDataMessage(HWND hWnd, DWORD dwData, PVOID lpData, DWORD cbData, WPARAM wParam);
BOOL FormatHResultMessageW(PCWSTR pszSource, HRESULT hr, PWSTR* ppszMessage);
BOOL FormatWin32MessageW(PCWSTR pszSource, DWORD dwCode, PWSTR* ppszMessage);
void ShowErrorW(PCWSTR pszContent);
void ShowErrorExW(PCWSTR pszContent, PCWSTR pszContentEx);
void ShowHResultErrorW(PCWSTR pszContent, PCWSTR pszSource, HRESULT hr);
void ShowWin32ErrorW(PCWSTR pszContent, PCWSTR pszSource, DWORD dwCode);
void ShowLastWin32ErrorW(PCWSTR pszContent, PCWSTR pszSource);
void LogPrintfW(PCWSTR pszFormat, ...);
void LogPutsW(PCWSTR pszMessage);
void LogErrno(PCWSTR pszSource, int iErrno);
void LogHResult(PCWSTR pszSource, HRESULT hr);
void LogWin32Error(PCWSTR pszSource, DWORD dwErrorCode);
void LogLastWin32Error(PCWSTR pszSource);

int WINAPI
wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
	PWSTR pCmdLine, int nCmdShow)
{
	MSG Msg;
	WNDCLASS wc;

	g_hInstance = hInstance;

	SetThreadDescription(GetCurrentThread(), L"Main");

	//std::set_terminate(app::on_unhandled_exception);

	winrt::init_apartment();

	InitializeLogging();

	WM_BARCODEHELLO = RegisterWindowMessageW(L"Collector.BarcodeScanner.Hello");
	WM_BARCODEGOODBYE = RegisterWindowMessageW(L"Collector.BarcodeScanner.Goodbye");

	ZeroMemory(&wc, sizeof(wc));
	wc.lpfnWndProc = WndProc;
	wc.lpszClassName = WINDOW_CLASS;
	wc.hInstance = hInstance;
	RegisterClassW(&wc);

	//
	// Create our hidden window.
	//
	g_hWnd = CreateWindowExW(0, WINDOW_CLASS, WINDOW_CLASS,
		0, 0, 0, 300, 80, HWND_MESSAGE, NULL, hInstance, NULL);

	//
	// Create and show our notification tray icon.
	//
	g_niData.cbSize = sizeof(NOTIFYICONDATAW);
	g_niData.uVersion = NOTIFYICON_VERSION_4;
	g_niData.uID = 1;
	g_niData.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
	g_niData.hIcon = LoadIconW(hInstance, MAKEINTRESOURCE(IDI_APPICON));
	g_niData.hWnd = g_hWnd;
	g_niData.dwInfoFlags = NIIF_INFO;
	g_niData.uCallbackMessage = WM_NOTIFYICON;
	wcscpy_s(g_niData.szTip, L"Barcode Scanner");
	Shell_NotifyIconW(NIM_ADD, &g_niData);
	Shell_NotifyIconW(NIM_SETVERSION, &g_niData);

	//
	// Create a right-click menu for the notification tray icon.
	//
	g_hniMenu = CreatePopupMenu();
	AppendMenuW(g_hniMenu, MF_STRING, IDC_LOG, L"View Log...");
	AppendMenuW(g_hniMenu, MF_SEPARATOR, 0, NULL);
	AppendMenuW(g_hniMenu, MF_STRING, IDC_EXIT, L"Exit");

	//
	// Register to receive events when any window opens or closes.
	//
	g_hWinEvent = SetWinEventHook(EVENT_OBJECT_CREATE, EVENT_OBJECT_DESTROY,
		nullptr, WinEventProc, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

	LogPutsW(L"Application initialized");

	//
	// Run the main wndow message processing loop.
	//
	while (GetMessageW(&Msg, NULL, 0, 0))
	{
		TranslateMessage(&Msg);
		DispatchMessageW(&Msg);
	}

	LogPutsW(L"Application exiting");

	UnhookWinEvent(g_hWinEvent);
	
	FlushFileBuffers(g_hLogFile);
	CloseHandle(g_hLogFile);

	return (int)Msg.wParam;
}

HRESULT InitializeLogging()
{
	static BYTE Utf16LEBom[] = { 0xFF, 0xFE };
	DWORD dwErrorCode;
	HRESULT hr;
	PWSTR pszAppDataPath;
	DWORD NumberOfBytesWritten;

	hr = SHGetKnownFolderPath(FOLDERID_LocalAppData, KF_FLAG_CREATE, NULL, &pszAppDataPath);
	if (FAILED(hr))
	{
		CoTaskMemFree(pszAppDataPath);
		LogHResult(L"SHGetKnownFolderPath", hr);
		return hr;
	}

	hr = StringCchCopyW(g_szLogFilePath, ARRAYSIZE(g_szLogFilePath), pszAppDataPath);
	CoTaskMemFree(pszAppDataPath);
	if (FAILED(hr))
	{
		LogHResult(L"StringCchCopyW", hr);
		return hr;
	}

	hr = StringCchCatW(g_szLogFilePath, ARRAYSIZE(g_szLogFilePath), L"\\BarcodeScanner");
	if (FAILED(hr))
	{
		LogHResult(L"StringCchCatW", hr);
		return hr;
	}

	if (!CreateDirectoryW(g_szLogFilePath, NULL))
	{
		dwErrorCode = GetLastError();
		if (dwErrorCode != ERROR_ALREADY_EXISTS)
		{
			LogWin32Error(L"CreateDirectoryW", dwErrorCode);
			return HRESULT_FROM_WIN32(dwErrorCode);
		}
	}

	hr = StringCchCatW(g_szLogFilePath, ARRAYSIZE(g_szLogFilePath), L"\\log.txt");
	if (FAILED(hr))
	{
		LogHResult(L"StringCchCatW", hr);
		return hr;
	}

	g_hLogFile = CreateFileW(g_szLogFilePath, GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
	if (g_hLogFile == INVALID_HANDLE_VALUE)
	{
		LogLastWin32Error(L"CreateFileW");
		return HRESULT_FROM_WIN32(GetLastError());
	}

	if (!WriteFile(g_hLogFile, Utf16LEBom, 2, &NumberOfBytesWritten, NULL))
	{
		LogLastWin32Error(L"WriteFile");
		CloseHandle(g_hLogFile);
		g_hLogFile = INVALID_HANDLE_VALUE;
		return HRESULT_FROM_WIN32(GetLastError());
	}

	return S_OK;
}

/*void app::on_unhandled_exception()
{
	try
	{
		std::rethrow_exception(std::current_exception());
	}
	catch (const std::exception& ex)
	{
		MessageBoxA(nullptr, ex.what(), nullptr, MB_OK | MB_ICONERROR);
	}
	catch (const winrt::hresult_error& ex)
	{
		MessageBoxW(nullptr, ex.message().data(), nullptr, MB_OK | MB_ICONERROR);
	}
	catch (...)
	{
		MessageBoxW(nullptr, L"An unknown unhandled exception occurred.", nullptr, MB_OK | MB_ICONERROR);
	}
	abort();
}*/

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	static HDEVNOTIFY hDeviceNotify;
	POINT ptCursor;

	switch (message)
	{
	case WM_CREATE:
		if (!DoRegisterDeviceNotification(hWnd, &hDeviceNotify))
		{
			ShowLastWin32ErrorW(L"Failed to register for device event notifications.", L"DoRegisterDeviceNotification");
			ExitProcess(1);
		}
		BarcodeScanner::GetDefaultAsync().Completed(OnBarcodeScannerGetDefaultAsyncCompleted);
		PostMessageW(HWND_BROADCAST, WM_BARCODEHELLO, (WPARAM)hWnd, 0);
		break;

	case WM_COMMAND:
		switch (LOWORD(wParam))
		{
		case IDC_LOG:
			ShellExecuteW(hWnd, L"open", g_szLogFilePath, NULL, NULL, SW_SHOWNORMAL);
			break;
		case IDC_EXIT:
			DestroyWindow(hWnd);
			return 1;
		}
		break;

	case WM_DESTROY:
		PostMessageW(HWND_BROADCAST, WM_BARCODEGOODBYE, (WPARAM)hWnd, 0);
		UnregisterDeviceNotification(hDeviceNotify);
		Shell_NotifyIconW(NIM_DELETE, &g_niData);
		PostQuitMessage(0);
		break;

	case WM_NOTIFYICON:
		switch (LOWORD(lParam))
		{
		case WM_CONTEXTMENU:
			GetCursorPos(&ptCursor);
			SetForegroundWindow(hWnd); // Fix for menu not disappearing when you click outside of it
			TrackPopupMenu(g_hniMenu, 0, ptCursor.x, ptCursor.y, 0, hWnd, NULL);
			PostMessageW(hWnd, WM_NULL, 0, 0);
			break;
		}
		break;

	case WM_DEVICECHANGE:
	case WM_MYDEVICECHANGE:
		switch (wParam)
		{
		case DBT_DEVICEARRIVAL:
			OnDeviceArrival((PDEV_BROADCAST_DEVICEINTERFACE)lParam);
			break;
		case DBT_DEVICEREMOVECOMPLETE:
			OnDeviceRemoveComplete((PDEV_BROADCAST_DEVICEINTERFACE)lParam);
			break;
		}
		return TRUE;

	case WM_BARCODESUBSCRIBE:
		SubscribeWindow((HWND)wParam);
		return TRUE;

	case WM_BARCODEUNSUBSCRIBE:
		UnsubscribeWindow((HWND)wParam);
		return TRUE;

	case WM_SCANNERCLAIMED:
		LogPutsW(L"Scanner claimed");
		g_ClaimedScanner = ClaimedBarcodeScanner{ (void*)lParam, winrt::take_ownership_from_abi };
		g_ClaimedScanner.Closed(OnClaimedBarcodeScannerClosed);
		g_ClaimedScanner.DataReceived(OnClaimedBarcodeScannerDataReceived);
		g_ClaimedScanner.IsDecodeDataEnabled(true);
		g_ClaimedScanner.EnableAsync().Completed(OnClaimedBarcodeScannerEnableAsyncCompleted);
		g_InitializingScanner = FALSE;
		LogPutsW(L"Scanner initialized");
		break;

	case WM_SCANNERENABLED:
		LogPutsW(L"Scanner enabled");
		break;

	case WM_SCANNERCLOSED:
		LogPutsW(L"Scanner closed");
		g_ClaimedScanner = nullptr;
		break;

	case WM_SCANNERDATARECEIVED:
	{
		BarcodeScannerReport Report{ (void*)lParam, winrt::take_ownership_from_abi };
		HWND hWnd = GetForegroundWindow();
		if (g_Subscribers.contains(hWnd))
		{
			SendScannerReportToWindow(Report, hWnd);
		}
		else
		{
			SimulateKeyboardInput(Report);
		}
		break;
	}

	default:
		return DefWindowProcW(hWnd, message, wParam, lParam);
	}

	return 0;
}

void CALLBACK WinEventProc(HWINEVENTHOOK hWinEventHook, DWORD event, HWND hWnd,
	LONG idObject, LONG idChild, DWORD dwEventThread, DWORD dwmsEventTime)
{
	UNREFERENCED_PARAMETER(hWinEventHook);
	UNREFERENCED_PARAMETER(idChild);
	UNREFERENCED_PARAMETER(dwEventThread);
	UNREFERENCED_PARAMETER(dwmsEventTime);
	if (event == EVENT_OBJECT_DESTROY && idObject == OBJID_WINDOW)
	{
		UnsubscribeWindow(hWnd);
	}
}

void SetKeyboardInput(LPINPUT lpInput, WORD wCode, DWORD dwFlags)
{
	ZeroMemory(lpInput, sizeof(INPUT));
	lpInput->type = INPUT_KEYBOARD;
	if (dwFlags & (KEYEVENTF_SCANCODE | KEYEVENTF_UNICODE))
	{
		lpInput->ki.wScan = wCode;
	}
	else
	{
		lpInput->ki.wVk = wCode;
	}
	lpInput->ki.dwFlags = dwFlags;
}

BOOL DoRegisterDeviceNotification(HWND hWnd, HDEVNOTIFY* phDeviceNotify)
{
	DEV_BROADCAST_DEVICEINTERFACE NotificationFilter;

	ZeroMemory(&NotificationFilter, sizeof(NotificationFilter));
	NotificationFilter.dbcc_size = sizeof(DEV_BROADCAST_DEVICEINTERFACE);
	NotificationFilter.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
	NotificationFilter.dbcc_classguid = PosBarcodeScannerClassGuid;

	*phDeviceNotify = RegisterDeviceNotificationW(hWnd, &NotificationFilter, DEVICE_NOTIFY_WINDOW_HANDLE);
	if (*phDeviceNotify == NULL)
	{
		LogLastWin32Error(L"RegisterDeviceNotificationW");
		return FALSE;
	}

	return TRUE;
}

void SubscribeWindow(HWND hWnd)
{
	g_Subscribers.insert(hWnd);
	LogPrintfW(L"Window handle 0x%08x subscribed", hWnd);
}

BOOL UnsubscribeWindow(HWND hWnd)
{
	if (g_Subscribers.erase(hWnd) > 0)
	{
		LogPrintfW(L"Window handle 0x%08x unsubscribed", hWnd);
		return TRUE;
	}
	return FALSE;
}

void OnDeviceArrival(PDEV_BROADCAST_DEVICEINTERFACE pDeviceInterface)
{
	assert(pDeviceInterface != NULL);
	assert(pDeviceInterface->dbcc_devicetype == DBT_DEVTYP_DEVICEINTERFACE);
	assert(pDeviceInterface->dbcc_classguid == PosBarcodeScannerClassGuid);

	LogPrintfW(L"Device connected: %s", pDeviceInterface->dbcc_name);

	if (g_ClaimedScanner)
	{
		LogPutsW(L"Ignoring because a scanner is already claimed");
		return;
	}

	if (g_InitializingScanner)
	{
		LogPutsW(L"Ignoring because a scanner is already being initialized");
		return;
	}

	g_InitializingScanner = TRUE;
	
	BarcodeScanner::FromIdAsync(pDeviceInterface->dbcc_name)
		.Completed(OnBarcodeScannerFromIdAsyncCompleted);
}

void OnDeviceRemoveComplete(PDEV_BROADCAST_DEVICEINTERFACE pDeviceInterface)
{
	assert(pDeviceInterface != NULL);
	assert(pDeviceInterface->dbcc_devicetype == DBT_DEVTYP_DEVICEINTERFACE);
	assert(pDeviceInterface->dbcc_classguid == PosBarcodeScannerClassGuid);

	LogPrintfW(L"Device removed: %s", pDeviceInterface->dbcc_name);

	if (g_ClaimedScanner && _wcsicmp(g_ClaimedScanner.DeviceId().c_str(), pDeviceInterface->dbcc_name) == 0)
	{
		g_ClaimedScanner.Close();
	}
}

void OnBarcodeScannerGetDefaultAsyncCompleted(IAsyncOperation<BarcodeScanner> const& sender, AsyncStatus const)
{
	BarcodeScanner DefaultScanner{ nullptr };
	winrt::hstring DeviceId;
	PDEV_BROADCAST_DEVICEINTERFACE pDeviceInfo;

	DefaultScanner = sender.GetResults();
	if (!DefaultScanner)
	{
		return;
	}
	DeviceId = DefaultScanner.DeviceId();

	pDeviceInfo = (PDEV_BROADCAST_DEVICEINTERFACE)HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY,
		sizeof(DEV_BROADCAST_DEVICEINTERFACE) + (DeviceId.size() * sizeof(WCHAR)));
	if (pDeviceInfo == NULL)
	{
		return;
	}
	pDeviceInfo->dbcc_size = sizeof(DEV_BROADCAST_DEVICEINTERFACE);
	pDeviceInfo->dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
	pDeviceInfo->dbcc_classguid = PosBarcodeScannerClassGuid;
	wcscpy_s(pDeviceInfo->dbcc_name, DeviceId.size() + 1, DeviceId.c_str());

	/*
	Send a fake DBT_DEVICEARRIVAL message so that the default scanner is claimed.
	We can't send a WM_DEVICECHANGE message directly, so we use a custom message
	that gets handled the same way by WndProc.
	*/
	SendMessageW(g_hWnd, WM_MYDEVICECHANGE, DBT_DEVICEARRIVAL, (LPARAM)pDeviceInfo);

	HeapFree(GetProcessHeap(), 0, pDeviceInfo);
}

void OnBarcodeScannerFromIdAsyncCompleted(IAsyncOperation<BarcodeScanner> const& Sender, AsyncStatus const)
{
	BarcodeScanner Scanner = Sender.GetResults();
	if (Scanner)
	{
		Scanner.ClaimScannerAsync().Completed(OnBarcodeScannerClaimScannerAsyncCompleted);
	}
}

void OnBarcodeScannerClaimScannerAsyncCompleted(IAsyncOperation<ClaimedBarcodeScanner> const& Sender, AsyncStatus const)
{
	ClaimedBarcodeScanner ClaimedScanner = Sender.GetResults();
	if (ClaimedScanner)
	{
		PostMessageW(g_hWnd, WM_SCANNERCLAIMED, 0, (LPARAM)winrt::detach_abi(ClaimedScanner));
	}
}

void OnClaimedBarcodeScannerEnableAsyncCompleted(IAsyncAction const& Sender, AsyncStatus const)
{
	PostMessageW(g_hWnd, WM_SCANNERENABLED, 0, 0);
}

void OnClaimedBarcodeScannerReleaseDeviceRequested(IInspectable const&, ClaimedBarcodeScanner const& Scanner)
{
	Scanner.RetainDevice(); // Never gonna give you up...
}

void OnClaimedBarcodeScannerClosed(ClaimedBarcodeScanner, ClaimedBarcodeScannerClosedEventArgs const&)
{
	PostMessageW(g_hWnd, WM_SCANNERCLOSED, 0, 0);
}

void OnClaimedBarcodeScannerDataReceived(ClaimedBarcodeScanner, BarcodeScannerDataReceivedEventArgs const& Args)
{
	PostMessageW(g_hWnd, WM_SCANNERDATARECEIVED, 0, (LPARAM)winrt::detach_abi(Args.Report()));
}

void SendScannerReportToWindow(BarcodeScannerReport Report, HWND hWnd)
{
	PWSTR pszDataBase64 = NULL;
	IBuffer ScanDataLabel = Report.ScanDataLabel();
	JsonObject RootObject;
	std::string JsonString;

	pszDataBase64 = CryptBinaryToStringAllocW(ScanDataLabel.data(), ScanDataLabel.Length(),
		CRYPT_STRING_BASE64 | CRYPT_STRING_NOCRLF);

	RootObject.SetNamedValue(L"Symbology", JsonValue::CreateStringValue(BarcodeSymbologies::GetName(Report.ScanDataType())));
	RootObject.SetNamedValue(L"Data", JsonValue::CreateStringValue(pszDataBase64));
	JsonString = winrt::to_string(RootObject.Stringify()); /* Convert to UTF-8 */

	SendCopyDataMessage(hWnd, WM_COPYDATA_BARCODE, (PVOID)JsonString.c_str(), JsonString.size() + 1, (WPARAM)g_hWnd);

	CryptMemFree(pszDataBase64);
}

BOOL WINAPI GetWindowThreadProcessInformationW(HWND hWnd, LPDWORD lpdwThreadId, LPDWORD lpdwProcessId, LPWSTR* ppszExeName)
{
	DWORD dwThreadId = 0;
	DWORD dwProcessId = 0;
	HANDLE hProcess = NULL;
	LPWSTR pszExeName = NULL;
	DWORD cchExeName = MAX_PATH;

	dwThreadId = GetWindowThreadProcessId(hWnd, &dwProcessId);

	if (ppszExeName)
	{
		hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, dwProcessId);
		if (!hProcess)
		{
			LogLastWin32Error(L"OpenProcess");
			return FALSE;
		}
		pszExeName = (LPWSTR)LocalAlloc(0, cchExeName * sizeof(WCHAR));
		if (!pszExeName)
		{
			CloseHandle(hProcess);
			LogLastWin32Error(L"LocalAlloc");
			return FALSE;
		}
		if (!QueryFullProcessImageNameW(hProcess, 0, pszExeName, &cchExeName))
		{
			LocalFree(pszExeName);
			CloseHandle(hProcess);
			LogLastWin32Error(L"QueryFullProcessImageNameW");
			return FALSE;
		}
		CloseHandle(hProcess);
	}

	if (lpdwThreadId)
	{
		*lpdwThreadId = dwThreadId;
	}

	if (lpdwProcessId)
	{
		*lpdwProcessId = dwProcessId;
	}

	if (ppszExeName)
	{
		*ppszExeName = pszExeName;
	}

	return TRUE;
}

LPWSTR WINAPI CryptBinaryToStringAllocW(LPCBYTE pbBinary, DWORD cbBinary, DWORD dwFlags)
{
	LPWSTR pszString = NULL;
	DWORD cchString = 0;
	if (!CryptBinaryToStringW(pbBinary, cbBinary, dwFlags, NULL, &cchString))
	{
		return NULL;
	}
	pszString = (LPWSTR)CryptMemAlloc(cchString * sizeof(WCHAR));
	if (!CryptBinaryToStringW(pbBinary, cbBinary, dwFlags, pszString, &cchString))
	{
		CryptMemFree(pszString);
		return NULL;
	}
	return pszString;
}

LRESULT WINAPI SendCopyDataMessage(HWND hWnd, DWORD dwData, PVOID lpData, DWORD cbData, WPARAM wParam)
{
	COPYDATASTRUCT CopyDataStruct;
	ZeroMemory(&CopyDataStruct, sizeof(CopyDataStruct));
	CopyDataStruct.dwData = dwData;
	CopyDataStruct.cbData = cbData;
	CopyDataStruct.lpData = lpData;
	return SendMessageW(hWnd, WM_COPYDATA, wParam, (LPARAM)&CopyDataStruct);
}

BOOL FormatHResultMessageW(PCWSTR pszSource, HRESULT hr, PWSTR* ppszMessage)
{
	static WCHAR pcszNoDescription[] = L"No description available.";
	DWORD dwResult;
	LPWSTR pszDescription = NULL;
	DWORD_PTR pArgs[4];

	dwResult = FormatMessageW(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL, hr, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPWSTR)&pszDescription, 0, NULL);
	if (dwResult && pszDescription)
	{
		StrTrimW(pszDescription, L" \t\r\n");
	}
	else if (FAILED(SHLocalStrDupW(pcszNoDescription, &pszDescription)))
	{
		SetLastError(E_OUTOFMEMORY);
		return FALSE;
	}

	pArgs[0] = (DWORD_PTR)(pszSource ? pszSource : L"Operation");
	pArgs[1] = (DWORD_PTR)(SUCCEEDED(hr) ? L"succeeded" : L"failed");
	pArgs[2] = hr;
	pArgs[3] = (DWORD_PTR)pszDescription;

	dwResult = FormatMessageW(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_STRING | FORMAT_MESSAGE_ARGUMENT_ARRAY,
		L"%1 %2 with HRESULT 0x%3!08x!: %4", 0, 0,
		(LPWSTR)ppszMessage, 0, (va_list*)pArgs);

	LocalFree(pszDescription);

	return dwResult != 0;
}

BOOL FormatWin32MessageW(PCWSTR pszSource, DWORD dwCode, PWSTR* ppszMessage)
{
	static WCHAR pcszNoDescription[] = L"No description available.";
	DWORD dwResult;
	LPWSTR pszDescription = NULL;
	DWORD_PTR pArgs[4];

	dwResult = FormatMessageW(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL, dwCode, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPWSTR)&pszDescription, 0, NULL);
	if (dwResult && pszDescription)
	{
		StrTrimW(pszDescription, L" \t\r\n");
	}
	else if (FAILED(SHLocalStrDupW(pcszNoDescription, &pszDescription)))
	{
		SetLastError(E_OUTOFMEMORY);
		return FALSE;
	}

	pArgs[0] = (DWORD_PTR)(pszSource ? pszSource : L"Operation");
	pArgs[1] = (DWORD_PTR)(dwCode == 0 ? L"succeeded" : L"failed");
	pArgs[2] = dwCode;
	pArgs[3] = (DWORD_PTR)pszDescription;

	dwResult = FormatMessageW(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_STRING | FORMAT_MESSAGE_ARGUMENT_ARRAY,
		L"%1 %2 with Win32 code 0x%3!08x!: %4", 0, 0,
		(LPWSTR)ppszMessage, 0, (va_list*)pArgs);

	LocalFree(pszDescription);

	return dwResult != 0;
}

void ShowErrorW(PCWSTR pszContent)
{
	ShowErrorExW(pszContent, NULL);
}

void ShowErrorExW(PCWSTR pszContent, PCWSTR pszContentEx)
{
	static TASKDIALOG_BUTTON pButtons[] = {
		{ IDC_LOG, L"View Log...\nThe log may show additional details about what led to this error." }
	};
	int nButton;
	TASKDIALOGCONFIG Config;

	if (!pszContent)
	{
		if (!pszContentEx)
		{
			pszContent = L"No details are available.";
		}
		else
		{
			pszContent = pszContentEx;
			pszContentEx = NULL;
		}
	}

	LogPutsW(L"An error occurred:");
	LogPutsW(pszContent);
	if (pszContentEx)
	{
		LogPutsW(pszContentEx);
	}

	ZeroMemory(&Config, sizeof(TASKDIALOGCONFIG));
	Config.cbSize = sizeof(TASKDIALOGCONFIG);
	Config.hInstance = g_hInstance;
	Config.dwFlags = TDF_EXPANDED_BY_DEFAULT;
	Config.pszWindowTitle = L"Barcode Scanner";
	Config.pszMainIcon = TD_ERROR_ICON;
	Config.pszMainInstruction = L"Something went wrong!";
	Config.pszContent = pszContent;
	Config.pszExpandedInformation = pszContentEx;
	Config.dwCommonButtons = TDCBF_OK_BUTTON;
	if (g_hLogFile != INVALID_HANDLE_VALUE)
	{
		Config.dwFlags |= TDF_USE_COMMAND_LINKS;
		Config.cButtons = ARRAYSIZE(pButtons);
		Config.pButtons = pButtons;
	}

	TaskDialogIndirect(&Config, &nButton, NULL, NULL);

	switch (nButton)
	{
	case IDC_LOG:
		ShellExecuteW(g_hWnd, L"open", g_szLogFilePath, NULL, NULL, SW_SHOWNORMAL);
		break;
	}
}

void ShowHResultErrorW(PCWSTR pszContent, PCWSTR pszSource, HRESULT hr)
{
	PWSTR pszContentEx;
	FormatHResultMessageW(pszSource, hr, &pszContentEx);
	ShowErrorExW(pszContent, pszContentEx);
	LocalFree(pszContentEx);
}

void ShowWin32ErrorW(PCWSTR pszContent, PCWSTR pszSource, DWORD dwCode)
{
	PWSTR pszContentEx;
	FormatWin32MessageW(pszSource, dwCode, &pszContentEx);
	ShowErrorExW(pszContent, pszContentEx);
	LocalFree(pszContentEx);
}

void ShowLastWin32ErrorW(PCWSTR pszContent, PCWSTR pszSource)
{
	ShowWin32ErrorW(pszContent, pszSource, GetLastError());
}

void LogPrintfW(PCWSTR pszFormat, ...)
{
	SYSTEMTIME LocalTime;
	DWORD NumberOfBytesWritten;
	DWORD cchMessage;
	WCHAR szMessage[512];
	va_list Arguments;

	va_start(Arguments, pszFormat);

	GetLocalTime(&LocalTime);

	cchMessage = swprintf_s(szMessage, L"%04d/%02d/%02d %02d:%02d:%02d.%04d ",
		LocalTime.wYear, LocalTime.wMonth, LocalTime.wDay,
		LocalTime.wHour, LocalTime.wMinute, LocalTime.wSecond, LocalTime.wMilliseconds);
	cchMessage += vswprintf_s(szMessage + cchMessage, ARRAYSIZE(szMessage) - cchMessage, pszFormat, Arguments);
	wcscat_s(szMessage + cchMessage, ARRAYSIZE(szMessage) - cchMessage, L"\r\n");
	cchMessage += 2;

	OutputDebugStringW(szMessage);
	if (g_hLogFile != INVALID_HANDLE_VALUE)
	{
		WriteFile(g_hLogFile, szMessage, cchMessage * 2, &NumberOfBytesWritten, NULL);
	}

	va_end(Arguments);
}

void LogPutsW(PCWSTR pszMessage)
{
	LogPrintfW(L"%s", pszMessage);
}

void LogErrno(PCWSTR pszSource, int iErrno)
{
	/*LPCWSTR pszResult;
	LPCWSTR pszMessage;
	pszResult = iErrno == 0 ? L"succeeded" : L"failed";
	pszMessage = _wcserror(iErrno);
	LogPrintfW(L"%s %s with errno 0x%08x: %s", pszSource, pszResult, iErrno, pszMessage);*/
}

void LogHResult(PCWSTR pszSource, HRESULT hr)
{
	LPWSTR pszMessage;
	FormatHResultMessageW(pszSource, hr, &pszMessage);
	LogPutsW(pszMessage);
	LocalFree(pszMessage);
}

void LogWin32Error(PCWSTR pszSource, DWORD dwErrorCode)
{
	LPWSTR pszMessage;
	FormatWin32MessageW(pszSource, dwErrorCode, &pszMessage);
	LogPutsW(pszMessage);
	LocalFree(pszMessage);
}

void LogLastWin32Error(PCWSTR pszSource)
{
	DWORD dwErrorCode = GetLastError();
	LogWin32Error(pszSource, dwErrorCode);
	SetLastError(dwErrorCode); /* Restore the original error in case LogWin32Error failed */
}

void SetVirtualKeystroke(LPINPUT pInputs, WORD wVk)
{
	pInputs[0].type = INPUT_KEYBOARD;
	pInputs[0].ki.wVk = wVk;
	pInputs[1].type = INPUT_KEYBOARD;
	pInputs[1].ki.wVk = wVk;
	pInputs[1].ki.dwFlags = KEYEVENTF_KEYUP;
}

void SetUnicodeKeystroke(LPINPUT pInputs, WORD wScan)
{
	pInputs[0].type = INPUT_KEYBOARD;
	pInputs[0].ki.wScan = wScan;
	pInputs[0].ki.dwFlags = KEYEVENTF_UNICODE;
	pInputs[1].type = INPUT_KEYBOARD;
	pInputs[1].ki.wScan = wScan;
	pInputs[1].ki.dwFlags = KEYEVENTF_KEYUP | KEYEVENTF_UNICODE;
}

void SimulateKeyboardInput(BarcodeScannerReport Report)
{
	int i, j = 0;
	LPINPUT pInputs;
	UINT cInputs;
	IBuffer ScanDataLabel = Report.ScanDataLabel();

	cInputs = (ScanDataLabel.Length() * 2) + 2;
	pInputs = (LPINPUT)LocalAlloc(LMEM_ZEROINIT, sizeof(INPUT) * cInputs);
	if (pInputs == NULL)
	{
		return;
	}

	for (i = 0; i < ScanDataLabel.Length(); i++)
	{
		BYTE ch = ScanDataLabel.data()[i];
		if (ch == '\n')
		{
			SetVirtualKeystroke(&pInputs[j], VK_RETURN);
		}
		else if (ch > 31 && ch < 127)
		{
			SetUnicodeKeystroke(&pInputs[j], ch);
		}
		else
		{
			continue;
		}
		j += 2;
	}

	if (j > 0)
	{
		SetVirtualKeystroke(&pInputs[j], VK_RETURN);
		SendInput(cInputs, pInputs, sizeof(INPUT));
	}

	LocalFree(pInputs);
}

/*void app::show_notification(PCWSTR title, PCWSTR text, UINT timeout_millis)
{
	std::lock_guard lock(notify_icon_mutex);
	notify_icon_data.uFlags |= NIF_INFO;
	notify_icon_data.uTimeout = timeout_millis;
	wcscpy_s(notify_icon_data.szInfoTitle, title);
	wcscpy_s(notify_icon_data.szInfo, text);
	Shell_NotifyIconW(NIM_MODIFY, &notify_icon_data);
}*/

// dllmain.cpp : Defines the entry point for the DLL application.
//
// Injected into steamcmd.exe so that it always sees the Steam client
// "Language" setting as "english", forcing its console output to English
// regardless of the client's configured language.
//
// The hooks are installed with Microsoft Detours (vendored under ./detours,
// MIT licensed) instead of the unmaintained EasyHook. Detours builds the
// trampolines and handles all instruction patching; our hooks only describe
// the desired behaviour and call the original function through the Real_*
// pointers, which is safe and recursion-free.

#include "pch.h"
#include "detours/detours.h"

#include <string.h>
#include <unordered_set>

#pragma comment(lib, "Advapi32.lib")

typedef LSTATUS (WINAPI *PF_REGOPENKEYEXA)(HKEY, LPCSTR, DWORD, REGSAM, PHKEY);
typedef LSTATUS (WINAPI *PF_REGOPENKEYEXW)(HKEY, LPCWSTR, DWORD, REGSAM, PHKEY);
typedef LSTATUS (WINAPI *PF_REGCLOSEKEY)(HKEY);
typedef LSTATUS (WINAPI *PF_REGQUERYVALUEEXW)(HKEY, LPCWSTR, LPDWORD, LPDWORD, LPBYTE, LPDWORD);

// Original implementations. After DetourAttach() these point at Detours'
// trampoline for the original code, so calling them from inside a hook
// always executes the real API exactly once.
static PF_REGOPENKEYEXA    Real_RegOpenKeyExA = nullptr;
static PF_REGOPENKEYEXW    Real_RegOpenKeyExW = nullptr;
static PF_REGCLOSEKEY      Real_RegCloseKey = nullptr;
static PF_REGQUERYVALUEEXW Real_RegQueryValueExW = nullptr;

// Open handles that point at "Software\Valve\Steam".
static std::unordered_set<HKEY> Dummies;
static CRITICAL_SECTION DummiesLock;

static const CHAR  kSteamKeyA[] = "Software\\Valve\\Steam";
static const WCHAR kSteamKeyW[] = L"Software\\Valve\\Steam";
static const WCHAR kLanguageW[] = L"Language";
static const WCHAR kEnglishW[] = L"english"; // 7 chars + null = 16 bytes

static bool IsDummyKey(HKEY hKey)
{
	EnterCriticalSection(&DummiesLock);
	bool found = Dummies.find(hKey) != Dummies.end();
	LeaveCriticalSection(&DummiesLock);
	return found;
}

static void TrackKey(HKEY hKey)
{
	if (hKey == nullptr)
		return;
	EnterCriticalSection(&DummiesLock);
	Dummies.insert(hKey);
	LeaveCriticalSection(&DummiesLock);
}

static void UntrackKey(HKEY hKey)
{
	EnterCriticalSection(&DummiesLock);
	Dummies.erase(hKey);
	LeaveCriticalSection(&DummiesLock);
}

// ---------------------------------------------------------------------------
// Detours for the registry API. Each hook replicates the behaviour of the
// original EasyHook hook: track the Steam key, and answer "Language"
// queries on it with "english".
// ---------------------------------------------------------------------------

LSTATUS WINAPI Detour_RegOpenKeyExA(HKEY hKey, LPCSTR lpSubKey, DWORD ulOptions, REGSAM samDesired, PHKEY phkResult)
{
	LSTATUS ret = Real_RegOpenKeyExA(hKey, lpSubKey, ulOptions, samDesired, phkResult);

	if (ret == ERROR_SUCCESS &&
		hKey == HKEY_CURRENT_USER &&
		lpSubKey != nullptr &&
		_stricmp(lpSubKey, kSteamKeyA) == 0 &&
		phkResult != nullptr)
	{
		TrackKey(*phkResult);
	}

	return ret;
}

LSTATUS WINAPI Detour_RegOpenKeyExW(HKEY hKey, LPCWSTR lpSubKey, DWORD ulOptions, REGSAM samDesired, PHKEY phkResult)
{
	LSTATUS ret = Real_RegOpenKeyExW(hKey, lpSubKey, ulOptions, samDesired, phkResult);

	if (ret == ERROR_SUCCESS &&
		hKey == HKEY_CURRENT_USER &&
		lpSubKey != nullptr &&
		_wcsicmp(lpSubKey, kSteamKeyW) == 0 &&
		phkResult != nullptr)
	{
		TrackKey(*phkResult);
	}

	return ret;
}

LSTATUS WINAPI Detour_RegCloseKey(HKEY hKey)
{
	UntrackKey(hKey);
	return Real_RegCloseKey(hKey);
}

LSTATUS WINAPI Detour_RegQueryValueExW(HKEY hKey, LPCWSTR lpValueName, LPDWORD lpReserved, LPDWORD lpType, LPBYTE lpData, LPDWORD lpcbData)
{
	if (!IsDummyKey(hKey) ||
		lpValueName == nullptr || lstrcmpW(lpValueName, kLanguageW) != 0 ||
		lpType == nullptr)
	{
		return Real_RegQueryValueExW(hKey, lpValueName, lpReserved, lpType, lpData, lpcbData);
	}

	if (*lpType == REG_NONE)
	{
		*lpType = REG_SZ;
		// Report the size a follow-up data call will need, so callers that
		// query the size first allocate a buffer that fits "english".
		if (lpcbData != nullptr)
			*lpcbData = (DWORD)sizeof(kEnglishW);
		return ERROR_SUCCESS;
	}

	if (*lpType == REG_SZ)
	{
		if (lpData != nullptr && lpcbData != nullptr && *lpcbData >= sizeof(kEnglishW))
		{
			memcpy(lpData, kEnglishW, sizeof(kEnglishW));
			*lpcbData = (DWORD)sizeof(kEnglishW);
			return ERROR_SUCCESS;
		}
		// Caller did not provide a buffer large enough; fall back to the
		// real value instead of overflowing it.
	}

	return Real_RegQueryValueExW(hKey, lpValueName, lpReserved, lpType, lpData, lpcbData);
}

// ---------------------------------------------------------------------------
// Hook installation
// ---------------------------------------------------------------------------

static FARPROC ResolveApi(LPCSTR apiModule, LPCSTR function)
{
	HMODULE hModule = GetModuleHandleA(apiModule);
	if (hModule == nullptr)
		hModule = LoadLibraryA(apiModule);
	if (hModule == nullptr)
		return nullptr;
	return GetProcAddress(hModule, function);
}

static BOOL InstallHooks()
{
	// Resolve the functions the way the original implementation did: through
	// advapi32, which is what steamcmd.exe calls. (On systems where that
	// forward-resolves elsewhere, Detours follows the resolved target.)
	Real_RegOpenKeyExA = (PF_REGOPENKEYEXA)ResolveApi("advapi32", "RegOpenKeyExA");
	Real_RegOpenKeyExW = (PF_REGOPENKEYEXW)ResolveApi("advapi32", "RegOpenKeyExW");
	Real_RegCloseKey = (PF_REGCLOSEKEY)ResolveApi("advapi32", "RegCloseKey");
	Real_RegQueryValueExW = (PF_REGQUERYVALUEEXW)ResolveApi("advapi32", "RegQueryValueExW");

	if (!Real_RegOpenKeyExA || !Real_RegOpenKeyExW || !Real_RegCloseKey || !Real_RegQueryValueExW)
		return FALSE;

	LONG error = DetourTransactionBegin();
	if (error != NO_ERROR)
		return FALSE;

	// Make sure Detours never suspends us while patching.
	error = DetourUpdateThread(GetCurrentThread());
	if (error == NO_ERROR)
		error = DetourAttach(&(PVOID&)Real_RegOpenKeyExA, (PVOID)Detour_RegOpenKeyExA);
	if (error == NO_ERROR)
		error = DetourAttach(&(PVOID&)Real_RegOpenKeyExW, (PVOID)Detour_RegOpenKeyExW);
	if (error == NO_ERROR)
		error = DetourAttach(&(PVOID&)Real_RegCloseKey, (PVOID)Detour_RegCloseKey);
	if (error == NO_ERROR)
		error = DetourAttach(&(PVOID&)Real_RegQueryValueExW, (PVOID)Detour_RegQueryValueExW);

	if (DetourTransactionCommit() != NO_ERROR)
		return FALSE;

	return TRUE;
}

// ---------------------------------------------------------------------------
// DllMain
// ---------------------------------------------------------------------------

BOOL APIENTRY DllMain(HMODULE /*hModule*/, DWORD ul_reason_for_call, LPVOID /*lpReserved*/)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		InitializeCriticalSection(&DummiesLock);
		
		// The injector (steamcmdprox.exe) suspends the target's main thread,
		// injects us, and only then resumes it - so no other thread is running
		// while we patch. If hook installation fails we simply leave the
		// process untouched (it will run with its native language).
		if (!InstallHooks())
			OutputDebugStringA("steamcmdinj: failed to install registry hooks\n");
		break;

	case DLL_PROCESS_DETACH:
		DeleteCriticalSection(&DummiesLock);
		break;
	}

	return TRUE;
}

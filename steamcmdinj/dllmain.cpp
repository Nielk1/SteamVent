// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

//#include <Winreg.h>
#include <easyhook.h>
#include <unordered_set>

std::unordered_set<HKEY> Dummies = {};

LSTATUS WINAPI RegOpenKeyExAHook(HKEY hKey, LPCSTR lpSubKey, DWORD ulOptions, REGSAM samDesired, PHKEY phkResult)
{
	if (hKey == HKEY_CURRENT_USER && strcmp(lpSubKey, "Software\\Valve\\Steam") == 0)
	{
		LSTATUS ret = RegOpenKeyExA(hKey, lpSubKey, ulOptions, samDesired, phkResult);
		Dummies.insert(*phkResult);
		return ret;
	}

	return RegOpenKeyExA(hKey, lpSubKey, ulOptions, samDesired, phkResult);
}
LSTATUS WINAPI RegCloseKeyHook(HKEY hKey)
{
	std::unordered_set<HKEY>::const_iterator got = Dummies.find(hKey);
	if (got != Dummies.end())
		Dummies.erase(got);
	
	return RegCloseKey(hKey);
}

LSTATUS WINAPI RegQueryValueExWHook(HKEY hKey, LPCWSTR lpValueName, LPDWORD lpReserved, LPDWORD lpType, LPBYTE  lpData, LPDWORD lpcbData)
{
	std::unordered_set<HKEY>::const_iterator got = Dummies.find(hKey);
	if (got == Dummies.end())
		return RegQueryValueExW(hKey, lpValueName, lpReserved, lpType, lpData, lpcbData);

	if (lstrcmp(lpValueName, L"Language") != 0)
		return RegQueryValueExW(hKey, lpValueName, lpReserved, lpType, lpData, lpcbData);

	if (*lpType == REG_NONE)
	{
		*lpType = REG_SZ;
		return ERROR_SUCCESS;
	}

	if (*lpType == REG_SZ)
	{
		lpData[0] = 'e'; lpData[1] = 0x00;
		lpData[2] = 'n'; lpData[3] = 0x00;
		lpData[4] = 'g'; lpData[5] = 0x00;
		lpData[6] = 'l'; lpData[7] = 0x00;
		lpData[8] = 'i'; lpData[9] = 0x00;
		lpData[10] = 's'; lpData[11] = 0x00;
		lpData[12] = 'h'; lpData[13] = 0x00;
		lpData[14] = 0x00; lpData[15] = 0x00;
		*lpcbData = 16;
		return ERROR_SUCCESS;
	}

	return RegQueryValueExW(hKey, lpValueName, lpReserved, lpType, lpData, lpcbData);
}

// EasyHook will be looking for this export to support DLL injection. If not found then 
// DLL injection will fail.
extern "C" void __declspec(dllexport) __stdcall NativeInjectionEntryPoint(REMOTE_ENTRY_INFO * inRemoteInfo);

void __stdcall NativeInjectionEntryPoint(REMOTE_ENTRY_INFO* inRemoteInfo)
{
	HOOK_TRACE_INFO hHook1 = { NULL }; // keep track of our hook
	HOOK_TRACE_INFO hHook2 = { NULL }; // keep track of our hook
	HOOK_TRACE_INFO hHook3 = { NULL }; // keep track of our hook
	NTSTATUS result = LhInstallHook(GetProcAddress(GetModuleHandle(TEXT("advapi32")), "RegOpenKeyExA"), RegOpenKeyExAHook, NULL, &hHook1);
	if (FAILED(result)) return; // Hook could not be installed, see RtlGetLastErrorString() for details
	result = LhInstallHook(GetProcAddress(GetModuleHandle(TEXT("advapi32")), "RegCloseKey"), RegCloseKeyHook, NULL, &hHook2);
	if (FAILED(result)) return; // Hook could not be installed, see RtlGetLastErrorString() for details
	result = LhInstallHook(GetProcAddress(GetModuleHandle(TEXT("advapi32")), "RegQueryValueExW"), RegQueryValueExWHook, NULL, &hHook3);
	if (FAILED(result)) return; // Hook could not be installed, see RtlGetLastErrorString() for details

	// If the threadId in the ACL is set to 0, 
	// then internally EasyHook uses GetCurrentThreadId()
	ULONG ACLEntries[1] = { 0 };

	// Enable the hook for the provided threadIds
	//LhSetInclusiveACL(ACLEntries, 1, &hHook1);
	LhSetExclusiveACL(ACLEntries, 1, &hHook1);
	//LhSetInclusiveACL(ACLEntries, 1, &hHook2);
	LhSetExclusiveACL(ACLEntries, 1, &hHook2);
	//LhSetInclusiveACL(ACLEntries, 1, &hHook3);
	LhSetExclusiveACL(ACLEntries, 1, &hHook3);

	// do I need to remove the hook at all if the program closed?
	////////////////////
	// Remove the hook handler
	//LhUninstallHook(&hHook);

	// This will restore all functions that have any 
	// uninstalled hooks back to their original state.
	//LhWaitForPendingRemovals();
	////////////////////

    return;
}
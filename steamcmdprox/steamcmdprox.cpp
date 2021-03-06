// steamcmdprox.cpp : Defines the entry point for the console application.
//
//
#include "stdafx.h"
//
//
//int main()
//{
//    return 0;
//}

//#define WIN32_LEAN_AND_MEAN		// Exclude rarely-used stuff from Windows headers
#include <windows.h>
#include <tchar.h>


#include <iostream>
#include <io.h>
#include <fcntl.h>

#include <easyhook.h>

/*
Quick note:
*/

#define SLEEP_TIME 50 // reactivity: sleep time to wait for subprocess to output some more data

CONST COORD origin = { 0, 0 };



// we should have been spawned using SW_HIDE, so our console window is not visible
int main(int argc, char* argv[])
{
	_setmode(_fileno(stdout), _O_U16TEXT);

	// get pipe/console to output to
	HANDLE hOutput = GetStdHandle(STD_OUTPUT_HANDLE);
	DWORD dwDummy;

	// parse command line : skip to RTconsole's arguments
	LPTSTR commandLine = GetCommandLine();
	if (*commandLine == '"')
		commandLine = _tcschr(commandLine + 1, _T('"'));
	else
		commandLine = _tcspbrk(commandLine, _T(" \t"));
	if (!commandLine) return -1;
	commandLine += _tcsspn(commandLine + 1, _T(" \t")) + 1;
	if (commandLine[0] == '\0') return -1;

	// prepare the console window & inherited screen buffer
	SECURITY_ATTRIBUTES sa;
	sa.nLength = sizeof(sa);
	sa.lpSecurityDescriptor = NULL;
	sa.bInheritHandle = TRUE;
	HANDLE hConsole = CreateConsoleScreenBuffer(GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, &sa, CONSOLE_TEXTMODE_BUFFER, NULL);

	//{
	//	short consoleX = SM_CXMIN;
	//	short consoleY = SM_CYMIN;
	//	CONSOLE_SCREEN_BUFFER_INFO csbiInfo;
	//	ZeroMemory(&csbiInfo, sizeof(CONSOLE_SCREEN_BUFFER_INFO));
	//	if (GetConsoleScreenBufferInfo(hConsole, &csbiInfo))
	//	{
	//		consoleX = csbiInfo.dwMaximumWindowSize.X;
	//		consoleY = csbiInfo.dwMaximumWindowSize.Y;
	//	}
	//	while (SetConsoleScreenBufferSize(hConsole, COORD{ consoleX + 10000, consoleY })) { consoleX = consoleX + 10000; }
	//	while (SetConsoleScreenBufferSize(hConsole, COORD{ consoleX + 1000, consoleY })) { consoleX = consoleX + 1000; }
	//	while (SetConsoleScreenBufferSize(hConsole, COORD{ consoleX + 100, consoleY })) { consoleX = consoleX + 100; }
	//	while (SetConsoleScreenBufferSize(hConsole, COORD{ consoleX + 10, consoleY })) { consoleX = consoleX + 10; }
	//	while (SetConsoleScreenBufferSize(hConsole, COORD{ consoleX + 1, consoleY })) { consoleX = consoleX + 1; }
	//	// resizing the Y portion over and over again is horribly slow, so lets try to be efficent about it
	//	if (SetConsoleScreenBufferSize(hConsole, { consoleX, consoleX })) { consoleY = consoleX; }
	//	while (SetConsoleScreenBufferSize(hConsole, { consoleX, consoleY + 10000 })) { consoleY = consoleY + 10000; }
	//	while (SetConsoleScreenBufferSize(hConsole, { consoleX, consoleY + 1000 })) { consoleY = consoleY + 1000; }
	//	while (SetConsoleScreenBufferSize(hConsole, { consoleX, consoleY + 100 })) { consoleY = consoleY + 100; }
	//	while (SetConsoleScreenBufferSize(hConsole, { consoleX, consoleY + 10 })) { consoleY = consoleY + 10; }
	//	while (SetConsoleScreenBufferSize(hConsole, { consoleX, consoleY + 1 })) { consoleY = consoleY + 1; }
	//	if (!SetConsoleScreenBufferSize(hConsole, { consoleX, consoleY }))
	//		throw "Console buffer won't size";
	//	for (short y = 0;y < consoleY;y++)
	//	{
	//		FillConsoleOutputCharacter(hConsole, '\0', consoleX * 2, { 0,y }, &dwDummy); // fill screen buffer with zeroes
	//	}
	//}
	
	{
		auto minX = GetSystemMetrics(SM_CXMIN);
		auto minY = GetSystemMetrics(SM_CYMIN);
		short consoleX = (short)minX;
		short consoleY = (short)minY;
		CONSOLE_SCREEN_BUFFER_INFO csbiInfo;
		ZeroMemory(&csbiInfo, sizeof(CONSOLE_SCREEN_BUFFER_INFO));
		if (GetConsoleScreenBufferInfo(hConsole, &csbiInfo))
		{
			consoleX = csbiInfo.dwSize.X;
			consoleY = csbiInfo.dwSize.Y;
		}
		//if (consoleX < 100) consoleX = 100;
		//if (consoleY < 5000) consoleY = 5000;
		//if (!SetConsoleScreenBufferSize(hConsole, { consoleX, consoleY }))
		//	throw "Console buffer won't size";
		for (short y = 0;y < consoleY;y++)
		{
			FillConsoleOutputCharacter(hConsole, '\0', consoleX * 2, { 0,y }, &dwDummy); // fill screen buffer with zeroes
		}
	}

	FillConsoleOutputCharacter(hConsole, '\0', MAXDWORD, origin, &dwDummy); // fill screen buffer with zeroes
	//FillConsoleOutputCharacter(hConsole, '\0', MAXLONG, origin, &dwDummy); // fill screen buffer with zeroes
	SetStdHandle(STD_OUTPUT_HANDLE, hConsole); // to be inherited by child process

											   // start the subprocess
	PROCESS_INFORMATION pi;
	STARTUPINFO si;
	ZeroMemory(&si, sizeof(STARTUPINFO));
	si.cb = sizeof(STARTUPINFO);
	si.dwFlags = STARTF_FORCEOFFFEEDBACK; // we don't want the "app starting" cursor
	//									  // all other default options are already good : we want subprocess to share the same console and to inherit our STD handles
	//si.dwFlags = STARTF_FORCEOFFFEEDBACK | STARTF_USESTDHANDLES;
	if (!CreateProcess(NULL, commandLine, NULL, NULL, TRUE, CREATE_SUSPENDED, NULL, NULL, &si, &pi))
	{
		CloseHandle(hConsole);
		return -2;
	}
	
	////////////////////
	LPWSTR dllToInject = new wchar_t[256];
	memset(dllToInject, 0x00, 256);
	int bytes = GetModuleFileName(NULL, dllToInject, 255);

	for (; bytes > 1; bytes--)
	{
		if (dllToInject[bytes - 1] == '\\')
		{
			dllToInject[bytes] = 's';
			dllToInject[bytes + 1] = 't';
			dllToInject[bytes + 2] = 'e';
			dllToInject[bytes + 3] = 'a';
			dllToInject[bytes + 4] = 'm';
			dllToInject[bytes + 5] = 'c';
			dllToInject[bytes + 6] = 'm';
			dllToInject[bytes + 7] = 'd';
			dllToInject[bytes + 8] = 'i';
			dllToInject[bytes + 9] = 'n';
			dllToInject[bytes + 10] = 'j';
			dllToInject[bytes + 11] = '.';
			dllToInject[bytes + 12] = 'd';
			dllToInject[bytes + 13] = 'l';
			dllToInject[bytes + 14] = 'l';
			dllToInject[bytes + 15] = '\0';
			break;
		}
	}

	//WCHAR* dllToInject = (WCHAR*)L"steamcmdinj.dll";
	NTSTATUS nt = RhInjectLibrary(
		pi.dwProcessId, // The process to inject into
		0,              // ThreadId to wake up upon injection
		EASYHOOK_INJECT_DEFAULT,
		dllToInject,    // 32-bit
		NULL,           // 64-bit not provided
		NULL,           // data to send to injected DLL entry point
		NULL            // size of data to send
	);

	ResumeThread(pi.hThread);

	//if (nt != 0)
	//{
	//	printf("RhInjectLibrary failed with error code = %d\n", nt);
	//	PWCHAR err = RtlGetLastErrorString();
	//	std::wcout << err << "\n";
	//}
	//else
	//{
	//	std::wcout << L"Library injected successfully.\n";
	//}
	////////////////////

	CloseHandle(pi.hThread); // always close the hThread after a CreateProcess

	COORD lastpos = { 0, 0 };
	CONSOLE_SCREEN_BUFFER_INFO csbi;
	bool exitNow = false;
	do
	{
		if (WaitForSingleObject(pi.hProcess, 0) != WAIT_TIMEOUT)
			exitNow = true; // exit after this last iteration

							// get screen buffer state
		GetConsoleScreenBufferInfo(hConsole, &csbi);
		int lineWidth = csbi.dwSize.X;

		if ((csbi.dwCursorPosition.X == lastpos.X) && (csbi.dwCursorPosition.Y == lastpos.Y))
			Sleep(SLEEP_TIME); // text cursor did not move, sleep a while
		else
		{
			DWORD count = (csbi.dwCursorPosition.Y - lastpos.Y)*lineWidth + csbi.dwCursorPosition.X - lastpos.X;
			// read newly output characters starting from last cursor position
			LPTSTR buffer = (LPTSTR)LocalAlloc(0, count * sizeof(TCHAR));
			ReadConsoleOutputCharacter(hConsole, buffer, count, lastpos, &count);
			// fill screen buffer with zeroes
			FillConsoleOutputCharacter(hConsole, '\0', count, lastpos, &dwDummy);

			SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);
			lastpos = csbi.dwCursorPosition;
			GetConsoleScreenBufferInfo(hConsole, &csbi);
			if ((csbi.dwCursorPosition.X == lastpos.X) && (csbi.dwCursorPosition.Y == lastpos.Y))
			{ // text cursor did not move since this treatment, hurry to reset it to home
				SetConsoleCursorPosition(hConsole, origin);
				lastpos = origin;
			}
			SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_NORMAL);

			// scan screen buffer and transmit character to real output handle
			LPTSTR scan = buffer;
			do
			{
				if (*scan)
				{
					DWORD len = 1;
					while (scan[len] && (len < count))
					{
						len++;
					}
					WriteFile(hOutput, scan, len * sizeof(TCHAR), &dwDummy, NULL); // nielk1 added unicode side fix
					//WriteConsoleW(hOutput, scan, len, &dwDummy, NULL);
					//std::wcout << scan << std::flush;
					scan += len;
					count -= len;
				}
				else
				{
					DWORD len = 1;
					while (!scan[len] && (len < count))
						len++;
					scan += len;
					count -= len;
					len = (len + lineWidth - 1) / lineWidth;
					for (;len;len--)
					{
						//WriteFile(hOutput, "\r\0\n\0", 2 * sizeof(TCHAR), &dwDummy, NULL); // nielk1 added unicode side fix
						//WriteConsoleW(hOutput, L"\r\n", 2, &dwDummy, NULL);
						std::wcout << std::endl << std::flush;
					}
				}
			} while (count);
			FlushFileBuffers(hOutput); // seems unnecessary
			LocalFree(buffer);
		}
		// loop until end of subprocess
	} while (!exitNow);



	CloseHandle(hConsole);

	// release subprocess handle
	DWORD exitCode;
	if (!GetExitCodeProcess(pi.hProcess, &exitCode))
		exitCode = -3;
	CloseHandle(pi.hProcess);
	return exitCode;
}

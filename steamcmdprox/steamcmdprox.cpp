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
#define MINIMUM_CPU_TIME 5000 // minimum nanoseconds of CPU work for subprocess between read and sleep, else force quit

CONST COORD origin = { 0, 0 };

typedef LONG(NTAPI* pNtSuspendProcess)(HANDLE ProcessHandle);
pNtSuspendProcess NtSuspendProcess;
typedef LONG(NTAPI* pNtResumeProcess)(HANDLE ProcessHandle);
pNtResumeProcess NtResumeProcess;

LARGE_INTEGER SubtractTime(FILETIME a, FILETIME b)
{
	ULARGE_INTEGER  left, right;
	LARGE_INTEGER    dif;
	left.LowPart = a.dwLowDateTime;
	left.HighPart = a.dwHighDateTime;
	right.LowPart = b.dwLowDateTime;
	right.HighPart = b.dwHighDateTime;

	dif.QuadPart = left.QuadPart - right.QuadPart;
	return dif;
}


// we should have been spawned using SW_HIDE, so our console window is not visible
int main(int argc, char* argv[])
{
	{
		HMODULE ntdll = ::GetModuleHandleA("ntdll.dll");
		NtSuspendProcess = (pNtSuspendProcess)::GetProcAddress(ntdll, "NtSuspendProcess");
		NtResumeProcess = (pNtResumeProcess)::GetProcAddress(ntdll, "NtResumeProcess");
	}

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
	
	/* {
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
	}*/

	FillConsoleOutputCharacter(hConsole, '\0', MAXDWORD, origin, &dwDummy); // fill screen buffer with zeroes
	//FillConsoleOutputCharacter(hConsole, '\0', MAXLONG, origin, &dwDummy); // fill screen buffer with zeroes

	// attempt to save and restore the original output handle but give the child process a differnt one
	//HANDLE stdOut = GetStdHandle(STD_OUTPUT_HANDLE);

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
	
	// attempt to save and restore the original output handle but give the child process a differnt one
	//SetStdHandle(STD_OUTPUT_HANDLE, stdOut); // restore original output handle

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

	//COORD lastpos = { 0, 0 };
	CONSOLE_SCREEN_BUFFER_INFO csbi;
	bool exitNow = false;
	//bool lastRespondingTime = 0;
	
	FILETIME lastGoodKernelTime;
	FILETIME lastGoodUserTime;
	memset(&lastGoodKernelTime, 0, sizeof(FILETIME));
	memset(&lastGoodUserTime, 0, sizeof(FILETIME));
	FILETIME lastStallKernelTime;
	FILETIME lastStallUserTime;

	FILETIME lastGoodCreateTime;
	FILETIME lastGoodExitTime;
	FILETIME lastStallCreateTime;
	FILETIME lastStallExitTime;

	// get screen buffer state
	GetConsoleScreenBufferInfo(hConsole, &csbi);
	int lineWidth = csbi.dwSize.X;

	LPTSTR lineBuffer = (LPTSTR)LocalAlloc('\0', (lineWidth + 1) * sizeof(TCHAR));
	if (!lineBuffer)
		return -1; // failed to create buffer
	lineBuffer[lineWidth] = '\0';
	short bufferWriteOffset = 0;
	DWORD outWrite = 0;

	do
	{
		if (WaitForSingleObject(pi.hProcess, 0) != WAIT_TIMEOUT)
			exitNow = true; // exit after this last iteration

		//if ((csbi.dwCursorPosition.X == lastpos.X) && (csbi.dwCursorPosition.Y == lastpos.Y))
		/*if ((csbi.dwCursorPosition.X == 0) && (csbi.dwCursorPosition.Y == 0))
		{
			GetProcessTimes(pi.hProcess, &lastGoodCreateTime, &lastGoodExitTime, &lastGoodKernelTime, &lastGoodUserTime);

			Sleep(SLEEP_TIME); // text cursor did not move, sleep a while

			GetProcessTimes(pi.hProcess, &lastStallCreateTime, &lastStallExitTime, &lastStallKernelTime, &lastStallUserTime);

			LARGE_INTEGER A = SubtractTime(lastStallKernelTime, lastGoodKernelTime);
			LARGE_INTEGER B = SubtractTime(lastStallUserTime, lastGoodUserTime);
			LONGLONG merge = A.QuadPart + B.QuadPart;

			// TODO confirm this experimental freeze detection
			// check if we spent enough CPU time between attempts, if we didn't do enough work force quit
			//std::wcout << "[CPU WORK TIME SLOT " << merge << "]" << std::endl << std::flush;
			if (merge < MINIMUM_CPU_TIME)
			{
				//exitNow = true;
			}
		}
		else*/
		{
			Sleep(SLEEP_TIME);

			// despite the suspension we're still missing blocks of output, we need to find a way to lock the console buffer the program is using
			NtSuspendProcess(pi.hProcess);
			
			GetConsoleScreenBufferInfo(hConsole, &csbi);

			int bufferWipeSize = bufferWriteOffset;
			for (short line = 0; line < csbi.dwCursorPosition.Y + 1; line++)
			{
				// read the new screen buffer characters into our line buffer
				ReadConsoleOutputCharacter(hConsole, &lineBuffer[bufferWriteOffset], lineWidth - bufferWriteOffset, { bufferWriteOffset, line }, &outWrite);

				// ensure we actually did something
				if (outWrite > 0)
				{
					bufferWipeSize += outWrite;
					bufferWriteOffset += outWrite; // push bufferWriteOffset forward
					bufferWriteOffset %= lineWidth; // wrap bufferWriteOffset around, this should also work as a bufferWriteOffset == lineWidth check but we're being safe

					// we are at the start of a line, which means the buffer has fresh data in it since we have to read something to get here
					if (bufferWriteOffset == 0)
					{
						//WriteFile(hOutput, lineBuffer, lineWidth * sizeof(TCHAR), &dwDummy, NULL); // unicode fix
						//std::wcout << std::endl << std::flush;
						for (int i = lineWidth; i >= 0; i--)
						{
							if (i == 0)
							{
                                lineBuffer[i] = _T('\0');
                                break;
                            }
							if (lineBuffer[i - 1] != ' ')
							{
                                lineBuffer[i] = _T('\0');
                                break;
                            }
                        }
						if (_tcslen(lineBuffer) > 0)
						{
							std::wcout << lineBuffer << std::endl << std::flush;
							//WriteFile(hOutput, lineBuffer, lineWidth * sizeof(TCHAR), &dwDummy, NULL);
						}
						memset(lineBuffer, 0, lineWidth * sizeof(TCHAR));
					}
				}
			}
			FillConsoleOutputCharacter(hConsole, 0, bufferWipeSize, {0, 0}, &dwDummy);
			//csbi.dwCursorPosition.X = 0; // leave X alone actually
			csbi.dwCursorPosition.Y = 0;
			SetConsoleCursorPosition(hConsole, csbi.dwCursorPosition);

			NtResumeProcess(pi.hProcess);
		}
		// loop until end of subprocess
	} while (!exitNow);

	// there is unwritten text in the buffer
	if (bufferWriteOffset > 0)
	{
		//WriteFile(hOutput, lineBuffer, lineWidth * sizeof(TCHAR), &dwDummy, NULL); // unicode fix
		//std::wcout << std::endl << std::flush;
		for (int i = lineWidth; i > 0; i--)
		{
			if (i == 0)
			{
				lineBuffer[i] = '\0';
				break;
			}
			if (lineBuffer[i - 1] != ' ')
			{
				lineBuffer[i] = '\0';
				break;
			}
		}
		if (_tcslen(lineBuffer) > 0)
			std::wcout << lineBuffer << std::endl << std::flush;
			//WriteFile(hOutput, lineBuffer, lineWidth * sizeof(TCHAR), &dwDummy, NULL);
		//memset(lineBuffer, ' ', sizeof(TCHAR) * lineWidth); // fill buffer with spaces
	}

	LocalFree(lineBuffer);

	CloseHandle(hConsole);

	// release subprocess handle
	DWORD exitCode;
	if (!GetExitCodeProcess(pi.hProcess, &exitCode))
		exitCode = -3;
	CloseHandle(pi.hProcess);
	return exitCode;
}

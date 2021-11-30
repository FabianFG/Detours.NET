#pragma once

using namespace System;

namespace DetoursNET
{
	public ref class Detours abstract sealed
	{
	public:
		static long DetourTransactionBegin();
		static long DetourTransactionAbort();
		static long DetourTransactionCommit();
		static long DetourTransactionCommitEx(void*** pppFailedPointer);
		static long DetourTransactionCommitEx(IntPtr hThread) { return DetourTransactionCommitEx(static_cast<void***>(hThread.ToPointer())); }

		static long DetourUpdateThread(void* hThread);
		static long DetourUpdateThread(IntPtr hThread) { return DetourUpdateThread(hThread.ToPointer()); }

		static long DetourAttach(void** ppPointer, void* pDetour);
		static long DetourAttach(void** ppPointer, IntPtr pDetour) { return DetourAttach(ppPointer, pDetour.ToPointer()); }

		static long DetourDetach(void** ppPointer, void* pDetour);
		static long DetourDetach(void** ppPointer, IntPtr pDetour) { return DetourDetach(ppPointer, pDetour.ToPointer()); }
	};
}

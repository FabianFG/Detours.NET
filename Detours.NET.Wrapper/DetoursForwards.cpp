#include "pch.h"

#include "DetoursForwards.h"
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include "detours.h"

namespace DetoursNET
{
	long Detours::DetourTransactionBegin()
	{
		return ::DetourTransactionBegin();
	}

	long Detours::DetourTransactionAbort()
	{
		return ::DetourTransactionAbort();
	}

	long Detours::DetourTransactionCommit()
	{
		return ::DetourTransactionCommit();
	}

	long Detours::DetourTransactionCommitEx(void*** pppFailedPointer)
	{
		return ::DetourTransactionCommitEx(pppFailedPointer);
	}

	long Detours::DetourUpdateThread(void* hThread)
	{
		return ::DetourUpdateThread(hThread);
	}

	long Detours::DetourAttach(void** ppPointer, void* pDetour)
	{
		return ::DetourAttach(ppPointer, pDetour);
	}

	long Detours::DetourDetach(void** ppPointer, void* pDetour)
	{
		return ::DetourDetach(ppPointer, pDetour);
	}
}

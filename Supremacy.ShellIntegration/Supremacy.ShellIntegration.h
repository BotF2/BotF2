// Supremacy.ShellIntegration.h

#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

IntPtr _appHwnd;
LPCWSTR _appPath;
LPCWSTR _workingDirectory;

#define ClrStringToLpcwstr(s) (const wchar_t*)(Marshal::StringToHGlobalUni(s)).ToPointer()

namespace Supremacy
{
    namespace ShellIntegration 
    {
	    public ref class TaskListManager abstract sealed
	    {
        public:
            static void PopulateTaskList(String^ appPath, IntPtr appHwnd);
	    };
    }
}

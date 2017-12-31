// This is the main DLL file.

#include "stdafx.h"
#include "Supremacy.ShellIntegration.h"

bool _IsItemInArray(IShellItem *item, IObjectArray *itemArray)
{
    return true;
}

HRESULT _AddCategoryToList(
    ICustomDestinationList *pcdl,
    IObjectArray *poaRemoved)
{
    IObjectCollection *poc;
    HRESULT hr = CoCreateInstance(CLSID_EnumerableObjectCollection, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&poc));
    if (SUCCEEDED(hr))
    {
        //for (UINT i = 0; i < ARRAYSIZE(c_rgpszFiles); i++)
        {           
            IShellItem *psi;
            if (SUCCEEDED(SHCreateItemFromParsingName(L"C:\\Users\\Mike Strobel\\Workspace\\supremacy\\Mainline\\SupremacyClient\\bin\\Debug\\auto.sav", NULL, IID_PPV_ARGS(&psi))))
            {                
                if(!_IsItemInArray(psi, poaRemoved))                
                {                    
                    poc->AddObject(psi);                
                }                 
                psi->Release();
            }        
        }         

        /*IShellLink *psl;
        hr = CoCreateInstance(
            CLSID_ShellLink,
            NULL,
            CLSCTX_INPROC_SERVER,
            IID_IShellLink, 
            reinterpret_cast<void**>(&psl));

        if (SUCCEEDED(hr))        
        {
            psl->SetDescription(L"Host Multiplayer Game");
            psl->SetPath(_appPath);
            psl->SetWorkingDirectory(_workingDirectory);
            psl->SetIconLocation(L"C:\\Users\\Mike Strobel\\Workspace\\supremacy\\Mainline\\SupremacyClient\\Supremacy.ico", 1);
            psl->Resolve((HWND)_appHwnd.ToPointer(), 0);
            poc->AddObject(psl);
            psl->Release();
        }   */

        
        IObjectArray *poa;        
        hr = poc->QueryInterface(IID_PPV_ARGS(&poa));        
        if (SUCCEEDED(hr))        
        {
            pcdl->AppendCategory(L"Saved Games", poa);
            //pcdl->AddUserTasks(poa);
            poa->Release();        
        }        
        poc->Release();    
    }    
    return hr;
};

void CreateJumpList()
{             
    ICustomDestinationList *pcdl;         
    HRESULT hr = CoCreateInstance(
        CLSID_DestinationList,
        NULL,
        CLSCTX_INPROC_SERVER,
        IID_PPV_ARGS(&pcdl));
    
    if (SUCCEEDED(hr))
    {
        hr = pcdl->SetAppID(L"BotF2.StarTrekSupremacy");
        if (SUCCEEDED(hr))
        {
            UINT uMaxSlots;
            IObjectArray *poaRemoved;
            hr = pcdl->BeginList(&uMaxSlots, IID_PPV_ARGS(&poaRemoved));
            if (SUCCEEDED(hr))
            {
                hr = _AddCategoryToList(pcdl, poaRemoved);
                if (SUCCEEDED(hr))
                {
                    pcdl->CommitList();
                }
                poaRemoved->Release();
            }
        }
    } 
};

namespace Supremacy
{
    namespace ShellIntegration
    {
        void TaskListManager::PopulateTaskList(String^ appPath, IntPtr appHwnd)
        {
            HRESULT hr = SetCurrentProcessExplicitAppUserModelID(L"BotF2.StarTrekSupremacy");
            _appPath = ClrStringToLpcwstr(appPath);
            _appHwnd = appHwnd;
            _workingDirectory = ClrStringToLpcwstr(System::IO::Path::GetDirectoryName(appPath));
            CreateJumpList();
        }
    }
}
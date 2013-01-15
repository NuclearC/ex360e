#define DLL_IMPORT _declspec(dllexport) 

#include <Windows.h>
#include <stdint.h>
#include <stdio.h>
#include <malloc.h>

#define D3D_DEBUG_INFO

#include <d3d9.h>
#include <DxErr.h>
#include <d3d9types.h>

#pragma comment (lib, "d3d9.lib")
#pragma comment (lib, "dxerr.lib")

#include "DisplayMode.h"
#include "DepthFormat.h"

// DirectX Stuff
LPDIRECT3D9 D3D; 
LPDIRECT3DDEVICE9 D3DDevice; 

// Window Stuff
LRESULT CALLBACK WindowProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
HWND hWindow;

extern "C"
{	
	DLL_IMPORT uint32_t D3D_CreateDirect3DHandle()
	{
		printf("D3D_CreateDirect3DHandle()\n");

		// Create Window
		WNDCLASSEX wc;
		ZeroMemory(&wc, sizeof(WNDCLASSEX));

		wc.cbSize = sizeof(WNDCLASSEX);
		wc.style = CS_HREDRAW | CS_VREDRAW;
		wc.lpfnWndProc = WindowProc;
		wc.hInstance = GetModuleHandle(NULL);
		wc.hCursor = LoadCursor(NULL, IDC_ARROW);
		wc.hbrBackground = (HBRUSH)COLOR_WINDOW;
		wc.lpszClassName = L"XNA360Class";

		RegisterClassEx(&wc);

		hWindow = CreateWindowEx(NULL,
						  L"XNA360Class",    // name of the window class
						  L"Xbox 360 XNA Player for Windows",   // title of the window	
						  WS_OVERLAPPEDWINDOW,    // window style
						  0,    // x-position of the window
						  0,    // y-position of the window
						  1280,    // width of the window
						  720,    // height of the window
						  NULL,    // we have no parent window, NULL
						  NULL,    // we aren't using menus, NULL
						  GetModuleHandle(NULL),    // application handle
						  NULL);    // used with multiple windows, NULL

		ShowWindow(hWindow, SW_SHOW);

		// Process initial Window Message (WM_CREATE)
		MSG msg;
		GetMessage(&msg, NULL, 0, 0);
		{
			// translate keystroke messages into the right format
			TranslateMessage(&msg);

			// send the message to the WindowProc function
			DispatchMessage(&msg);
		}

		// Create Direct3D Interface
		D3D = Direct3DCreate9(D3D_SDK_VERSION);

		return (uint32_t)D3D;
	}

	DLL_IMPORT void D3D_GetVideoMode(DisplayMode mode, uint32_t* widescreen)
	{
		printf("D3D_GetVideoMode(0x%X, 0x%x)\n", mode, widescreen);
		mode.width = 1280;
		mode.height = 720;
		mode.refreshRate = 60;
		mode.format = SurfaceFormat::Depth24;
	}

	DLL_IMPORT void D3D_ReleaseHandle(uint32_t handle)
	{
		printf("D3D_ReleaseHandle(0x%X)\n", handle);
		D3D->Release();
	}

	// Always return false, tell XNA we are HD.
	DLL_IMPORT uint32_t D3D_IsStandardDefinitionDisplay()
	{
		printf("D3D_IsStandardDefinitionDisplay()\n");
		return 0;
	}

	DLL_IMPORT uint32_t D3D_CheckDeviceFormat(uint32_t handle, SurfaceFormat::SurfaceFormat adapter, uint32_t usage, D3DRESOURCETYPE rtype, SurfaceFormat::SurfaceFormat checkformat)
	{
		printf("D3D_CheckDeviceFormat(0x%X, 0x%X, 0x%X, 0x%X, 0x%X)\n", handle, SurfaceFormat::ToPC(adapter), usage, rtype, SurfaceFormat::ToPC(checkformat));
		int ref = D3D->CheckDeviceFormat(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, SurfaceFormat::ToPC(adapter), usage, rtype, SurfaceFormat::ToPC(checkformat));
		if(ref!=D3D_OK)
		{
			printf("INCOMPATIBLE DEVICE FORMAT: %s\n", DXGetErrorStringA(ref));
		}
		return SUCCEEDED(ref); 
	}

	DLL_IMPORT uint32_t D3D_CheckDepthStencilMatch(uint32_t handle, D3DFORMAT adapterFormat, D3DFORMAT renderTargetFormat, DepthFormat::DepthFormat backbuffer)
	{
		printf("D3D_CheckStencilMatch(0x%X, 0x%X, 0x%X, 0x%X)\n", handle, adapterFormat, renderTargetFormat, backbuffer);
		return D3D_OK;

	}

	DLL_IMPORT uint32_t D3D_Device_GetDepthStencil(uint32_t handle)
	{
		printf("D3D_Device_GetDepthStencil(0x%X)\n", handle);	
		printf("TODO: Everything\n");
		return NULL;
	}
	
	DLL_IMPORT uint32_t D3D_DepthStencilBufferGetDesc(uint32_t deviceHandle, uint32_t handle, D3DSURFACE_DESC desc)
	{
		printf("D3D_DepthStencilBufferGetDesc(0x%X, 0x%X, 0x%X)\n", deviceHandle, handle, desc);
		return D3D_OK;
		IDirect3DSurface9* surface;
		int ret = D3DDevice->GetDepthStencilSurface(&surface);
		if(surface)
		{
			surface->GetDesc(&desc);
		}
		return SUCCEEDED(ret);
	}

	DLL_IMPORT uint32_t D3D_DepthStencilBufferReleaseHandle(uint32_t deviceHandle, uint32_t handle)
	{
		return 0;
	}

	DLL_IMPORT uint32_t D3D_CreateDeviceHandle(uint32_t handle, D3DPRESENT_PARAMETERS& pp, int32_t structureSize, uint32_t preserveContents)
	{
		printf("D3D_CreateDeviceHandle(0x%X, 0x%x, 0x%X, 0x%X, 0x%X)\n", handle, pp, structureSize, preserveContents);		

		pp.hDeviceWindow = hWindow;
		pp.Windowed = true;
		
		// Convert surface formats from XNA to DirectX 
		pp.BackBufferFormat = SurfaceFormat::ToPC((SurfaceFormat::SurfaceFormat)pp.BackBufferFormat);
		pp.AutoDepthStencilFormat = SurfaceFormat::ToPC((SurfaceFormat::SurfaceFormat)pp.AutoDepthStencilFormat);
		
		int ret = D3D->CreateDevice(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, hWindow, D3DCREATE_SOFTWARE_VERTEXPROCESSING, &pp, &D3DDevice);
		if(ret!=D3D_OK)
		{
			printf("ERROR CREATING DIRECT3D DEVICE: %s\n", DXGetErrorStringA(ret));
		}
		return ret;
	}


	DLL_IMPORT uint32_t D3D_Device_ReceivePackets(uint32_t handle, uint8_t* pPacketData, uint32_t packetSize)
	{
		printf("D3D_Device_ReceivePackets(0x%X, 0x%x, 0x%X): NOT YET IMPLEMENTED\n", handle, pPacketData, packetSize);
		return 1;
	}

	DLL_IMPORT uint32_t D3D_Device_ReleaseHandle(uint32_t handle)
	{
		printf("D3D_Device_ReleaseHandle(0x%X)\n", handle);	
		return D3D_OK;
		return SUCCEEDED(D3DDevice->Release());
	}
	

	DLL_IMPORT uint32_t D3D_Decl_CreateHandle(uint32_t deviceHandle, void* pElements, uint32_t elementCount)
	{
		printf("D3D_Decl_CreateHandle(0x%X, 0x%X, 0x%X)\n", deviceHandle, pElements, elementCount);	

		IDirect3DVertexDeclaration9* vertexDecl;
		uint32_t ret =  D3DDevice->CreateVertexDeclaration((D3DVERTEXELEMENT9*)pElements, &vertexDecl);
		return ret;
	}
}

LRESULT CALLBACK WindowProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	return DefWindowProc(hWnd, message, wParam, lParam);
}
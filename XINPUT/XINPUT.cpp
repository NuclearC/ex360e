#define DLL_IMPORT _declspec(dllexport) 
#include <Windows.h>
#include <stdint.h>
#include <stdio.h>

#include <Xinput.h>
#pragma comment (lib, "XInput.lib")

extern "C"
{
	DLL_IMPORT DWORD XINPUT_GetState(DWORD dwUserIndex, XINPUT_STATE *pState)
	{
		printf("XInput_GetState(0x%X, 0x%X)\n", dwUserIndex, pState);
		return XInputGetState(dwUserIndex, pState);
	}


	DLL_IMPORT DWORD XInput_SetState(DWORD dwUserIndex, XINPUT_VIBRATION pVibration)
	{
		printf("XInput_SetState(0x%X, 0x%X)\n", dwUserIndex, pVibration);
		return XInputSetState(dwUserIndex, &pVibration);	
	}
}

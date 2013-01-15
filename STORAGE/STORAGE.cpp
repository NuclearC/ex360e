#define DLL_IMPORT _declspec(dllexport) 


#include <Windows.h>
#include <stdint.h>
#include <stdio.h>

extern "C"
{
	DLL_IMPORT uint32_t STORAGE_GetStorageLocation(char* str, size_t length)
	{
		printf("STORAGE_GetStorageLocation(0x%X, %d)\n", str, length);
		strcpy_s(str, length, "./"); 
		return 0;
	}
}

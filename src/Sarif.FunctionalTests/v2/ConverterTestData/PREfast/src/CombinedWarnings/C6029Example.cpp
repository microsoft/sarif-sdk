#include "windows.h"
bool C6029_Example(HANDLE hFile)
{
	char buff[MAX_PATH];
	DWORD cbLen;
	DWORD cbRead;

	if (!ReadFile (hFile, &cbLen, sizeof (cbLen), &cbRead, NULL))  
	{
		// Read the bytes
		if (!ReadFile (hFile, buff, cbLen, &cbRead, NULL))  // warning 6029
		{
			return false;
		}
	}

	return true;
}




// Replace popular expanded macros with human readable form that was originally in the code

#include <winerror.h>

void HResultSucceeded(HRESULT hr)
{
	long * data = 0;
	if (SUCCEEDED(hr))
	{
		*data = 42;
	}
}

void HResultSucceeded2(HRESULT hr, bool b)
{
	long * data = 0;
	if (SUCCEEDED(hr) && b)
	{
		b = false;
	}
	else
	{
		*data = 42;
	}
}

void HResultSucceeded3(HRESULT hr, bool b, bool c)
{
	long * data = 0;
	if ((SUCCEEDED(hr) || b) && c)
	{
		*data = 42;
	}
}

void HResultSucceeded4(HRESULT _hResult)
{
	long * data = 0;
	if ((!SUCCEEDED(_hResult)))
	{
		*data = 42;
	}
}

void HResultFailed(HRESULT hr)
{
	long * data = 0;
	if (FAILED(hr))
	{
		*data = 42;
	}
}

void HResultFailed2(HRESULT hr, bool b)
{
	long * data = 0;
	if (FAILED(hr) && b || SUCCEEDED(hr))
	{
		b = false;
	}
	else
	{
		*data = 42;
	}
}

void HResultFailed3(HRESULT Hr_2, bool b, bool c)
{
	long * data = 0;
	if ((FAILED(Hr_2) || b) && c)
	{
		*data = 42;
	}
}

void HResultFailed4(HRESULT hr)
{
	long * data = 0;
	if ((!FAILED(hr)))
	{
		*data = 42;
	}
}

void HResultSFalse(HRESULT hr)
{
	long * data = 0;
	if (hr == S_FALSE)
	{
		*data = 42;
	}
}

void HResultSFalse2(HRESULT hr, bool b, bool c)
{
	long * data = 0;
	if (b && (c || S_FALSE == hr))
	{
		*data = 42;
	}
}

void HResultSOk(HRESULT hr)
{
	long * data = 0;
	if (S_OK == hr)
	{
		*data = 42;
	}
}

void HResultSOk2(HRESULT hr)
{
	long * data = 0;
	if ((S_FALSE+S_OK) == hr)
	{
		*data = 42;
	}
}


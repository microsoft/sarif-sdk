
// Don't report function key events for 'new' and 'delete', or possibly any
// other functions under predefined c++ types. For class members look for appropriate
// eOffset abstracts

#include <sal.h>

int BadFunctionKvtInNewArray()
{
	int n = 5;
	int * data = new int[n];
	data[0] = 42;

	int * data2 = new int[n]; // BAD 'n' is an In/Out argument to 'new' (declared at c:\future~1\future~1\predefined c++ types (compiler internal):23)

	int y = data[n];

	delete [] data;
	delete [] data2;

	return y;
}

class SampleClass
{
	int member;
public:
	SampleClass(_In_ int *param)
	{
		member = *param;
	}
};

int BadFunctionKvtInConstructor()
{
	int n = 5;
	int * data = new int[n];
	data[0] = 43;

	SampleClass * widget = new SampleClass(&n); // BAD 'n' is an Input argument to 'unknown function'

	int y = data[n];

	delete [] data;
	delete widget;

	return y;
}


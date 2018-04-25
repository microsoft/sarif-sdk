
// declaration keyevent for 6385/6

#include <sal.h>

void WritesArray(
	_Out_writes_(5) long * seq
	)
{
	seq[5] = 42;
}

void WritesArrayRange(
	_Out_writes_(size) long * seq,
	_In_range_(4,4) int size)
{
	seq[size] = 42;
}

void WritesArrayLoop(
	_Out_writes_(size) long * seq,
	int size)
{
	int i;
	for (i=0; i< size; i++)
	{
		seq[i] = 44;
	}
	seq[i] = 44;
}

void WritesArrayCompare(
	_Out_writes_(size) long * seq,
	int size)
{
	if (size == 4)
	{
		seq[0] = 44;
	}
	seq[size] = 44;
}



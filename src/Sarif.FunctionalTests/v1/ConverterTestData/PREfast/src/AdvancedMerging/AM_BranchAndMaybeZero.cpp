// Merge branch key events and maybezero key events

void CompareToZero(int * input)
{
	int * p = input;
	if (0 == p)
		int x = 5;
	int y = *p;
}

void CompareToZeroParam(int * input)
{
	if (input != 0)
		int x = 5;
	int y = *input;
}

// From a hard to understand bug:
void ComplexCompareToZero(int * input)
{
	int * p = input;
	if (&p[0])
		int x = 5;
	int y = *p;
}

#define SUCCEEDED(hrLongVar)		(((int)(hrLongVar)) > 4)

int C6001MacroExpansion_Example(int hrLongVar)
{
	int simpleVar;

	if ( SUCCEEDED(hrLongVar) )
	{
		simpleVar = 5;
	}

	return simpleVar;
}
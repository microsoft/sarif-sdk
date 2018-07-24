bool isInitializzedd = true;
bool isActivated = true;
bool isDefined = true;

int C6001LongCondition_Example()
{
	int simpleVar;

	if (  ( isInitializzedd && isActivated && !isDefined ) ||
		  ( isInitializzedd && !isActivated && isDefined ) ||
		  ( isInitializzedd && isActivated && !isDefined )  )
	{
		simpleVar = 5;
	}

	return simpleVar;
}

int C6001LongCondition15_Example()
{
	int simpleVar;

	if ( !isInitializzedd )
	{
		simpleVar = 5;
	}

	return simpleVar;
}

int C6001LongCondition14_Example()
{
	int simpleVar;

	if ( !isDefined )
	{
		simpleVar = 5;
	}

	return simpleVar;
}
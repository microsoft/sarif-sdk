
// Key Events in the presence of class member alias
// Look for 'this->source' which contains the sizeof information, not just 'source'

class Missed6305
{
	int j;
	int MissedAlias(wchar_t * p)
	{
		int i = 6;
		j = sizeof(p);

		i = j;

		p += i; //Expect 6305

		return i;
	}

	int MissedAlias2(wchar_t * p)
	{
		j = 6;
		int i = sizeof(p);

		j = i;

		p += j; //Expect 6305

		return j;
	}

	int MissedAliasPlusAssign(wchar_t * p)
	{
		int i = 6;
		j = sizeof(p);

		i += j;

		p += i; //Expect 6305

		return i;
	}

	int MissedAliasPlusAssign2(wchar_t * p)
	{
		j = 6;
		int i = sizeof(p);

		j += i;

		p += j; //Expect 6305

		return j;
	}

	int MissedAlias_LocalIsCorrect(wchar_t * p)
	{
		int i = 6;
		int j2 = sizeof(p);

		i = j2;

		p += i; //Expect 6305

		return i;
	}

	int MissedAliasPlusAssign_LocalIsCorrect(wchar_t * p)
	{
		int i = 6;
		int j2 = sizeof(p);

		i += j2;

		p += i; //Expect 6305

		return i;
	}

// --

	int cb;
	int SizeOfMissedForMember (int *p)
	{
		int i = 0;
		cb = sizeof(int);

		p+=cb; //Expect 6305
		return i;
	}

	int SizeOfMissedForMember_LocalIsCorrect (int *p)
	{
		int i = 0;
		int cb2 = sizeof(int);

		p+=cb2; //Expect 6305
		return i;
	}

};


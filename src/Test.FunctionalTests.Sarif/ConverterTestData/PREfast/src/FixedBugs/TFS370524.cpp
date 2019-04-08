
// Key event target in the presence of class member alias
// Look for 'this->source', not just 'source'

class MissedAliases
{
	int * source;

	int MissedAlias()
	{
		bool assert = source != 0 ? false : true; // Expects: 'source' may be NULL
		int * target = source;
		int x = *source;

		// suppress 4189
		if (assert && target) return x;
	}

	int MissedAlias2()
	{
		bool assert = source != 0 ? false : true; // Expects: 'this->source' may be NULL
		int * target = source;
		int x = *target;

		// suppress 4189
		if (assert && target) return x;
	}

	int MissedAlias3(int * target)
	{
		bool assert = target != 0 ? false : true; // Expects: 'target' may be NULL
		source = target;
		int x = *source;

		// suppress 4189
		if (assert && target) return x;
	}

};


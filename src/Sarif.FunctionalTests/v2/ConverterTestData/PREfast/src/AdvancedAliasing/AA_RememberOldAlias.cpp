// The result of an old aliasing operation may be clobbered by a 
// later assignment. Remember the old alias as long as the variable
// is "interesting"

void DontRememberOldAlias(int * result)
{
	int data[10];
      
	int index = 9;
	int b = index; // don't care about this alias ('b' not interesting)
	b = 10;

	*result = data[b];
}

void RememberOldAlias(int x, int y, int * result)
{
	int * a = 0;
	int * b = &x;
	int * c = &y;
	bool flag = true;

	for (int i=0; i < 2; i++)
	{
		if (flag) {
			b = a; // care about this alias ('b' is interesting)
			c = b;
			flag = false;
		} else {
			a = &x;
			b = &y;
			*result = *c;
		}
	}
}


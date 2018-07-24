// Sometimes a key event is repeated in different states because of loops.
// Maintain state for each key event and remember what state it was in when fired.
// Four different kinds of loop key events:
//
//		E - Enter		X - Exit
//		C - Continue	S - Skip
//
void C6011_OnlyOnSecondLoop()
{
	int * source = 0;
	bool flag = false;

	for (int i=0; i<3; i++)
	{
		if (!flag)
			flag = true;
		else
			int target = *source;
	}
}

// This should work even if PREfast rolls back because loop condition is a parameter
void C6011_OnlyOnSecondLoopWithParam(int param)
{
	int * source = 0;
	bool flag = false;

	for (int i=0; i<param; i++)
	{
		if (!flag)
			flag = true;
		else
			int target = *source;
	}
}

// Test loop rollback when defects occur before the rollback
void C6011_OnlyOnSecondLoopPlusRollbackDefect(int param)
{
	int * source = 0;
	bool flag = false;
    int x[2] = {0,1}, y[2] = {1,2};

	for (int i=0; i<param; i++)
	{
		if (!flag)
			flag = true;
		else
			int target = *source;
	}

    int j = x[param];
    int k = y[param+1];
}

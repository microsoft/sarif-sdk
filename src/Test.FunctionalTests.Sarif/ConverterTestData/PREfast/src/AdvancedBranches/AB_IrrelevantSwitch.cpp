// It works with switch statements: Only irrelevant if
// no nested key events on some path to the defect in any case.

// In this example, even though the defect occurs in all cases,
// we don't ignore the switch because one of the cases (case 2)
// contains a nested key event that is relevant on some path to the
// defect.

void C6385_MaybeRelevantSwitch(int flag, bool b)
{
	int data[2];
	data[0] = 1; data[1] = 2;

	int index = 2;

	switch(flag)
	{
	case 1:
		break;
	case 2:
		if (b)
			index = 1; // if we change this to 2, switch is ignored
		break;
	case 3:
		break;
	default:
		break;
	}

	int k = data[index];
}



// Make a branch irrelevant if an error occurs on both sides

int IrrelevantBranch_6011(bool flag, int input)
{
	int *source1 = 0, *source2 = 0;

	if (flag) {
		source2 = &input;
	}

	int *target = source1;
	return *target;
}

// This should work even if there are multiple defects 
// interacting with the branch

void C6385_MaybeRelevantBranch(int flag)
{
	int data[10];
	int index = 10;

	if (flag<10) {
		index = 9;
	}

	int k = data[index];
}


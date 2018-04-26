// Loops are more complicated.
// Unconditional loops are always executed the same way (only one path).
// Assume loops are irrelevant unless a useful key event is found in them.


void C6385_IrrelevantLoop()
{
	int data[10];
	int maxSize = 10;

	for (int i=0; i<maxSize; i++) // unconditional loop
		data[i] = i;

	int k = data[maxSize];
}


// Track "interesting" variables and detect when they are aliased.

void C6385_IndexAlias(int inputIndex)
{
	int * contentStores = new int [2];

	contentStores[0] = 0;

	contentStores[1] = 1;

	while (inputIndex > 1)
	{
		int index = inputIndex;

		contentStores[0] = contentStores[index];
	}

	delete [] contentStores;
}

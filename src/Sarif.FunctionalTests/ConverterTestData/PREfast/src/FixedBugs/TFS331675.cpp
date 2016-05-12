
// Branch KeyEvent "Skip" instead of "Enter"

// When loop is initialized with unknown parameter, branching
// heuristics are more complex...
void SimplifiedIncorrect(int start)
{
    int * p = 0;
    int i, j;

    for (i=start; i<3; i++)
    {
        if (i > 0) // should say "Enter this branch" if executing the next branch
        {
            if (i == 2)
            {
                j = *p;
            }
        }
    }
}

// When loop is initialized with fixed value, branching
// heuristics are straightforward and key events are correct...
void SimplifiedCorrect()
{
    int * p = 0;
    int i, j;

    for (i=1; i<3; i++)
    {
        if (i > 0)
        {
            if (i == 2)
            {
                j = *p;
            }
        }
    }
}

//
// CHANGE Due To: http://vstfdevdiv:8080/web/wi.aspx?id=333169
//   - This code sample should no longer emit a warning, as it was a false positive
//
// Simplified version of original bug
#include <Windows.h>

void Usage(){}

void OriginalSimplified(
    _In_ int argc,
    _In_reads_(argc) LPSTR  *argv
    )
{
    int i;

    if ( argc < 2 ) // give usage if invoked with no parms
        Usage();

    for (i=0; i<argc; i++) {
        if (argv[i][0] == '-') {
            switch(argv[i][1]) {
            case 'R':
                if (i+1 >= argc) {
                    Usage();
                }
                else {
                    &argv[i+1][0];
                }
                i++;
                break;
            default:
                Usage();
            }
        }
    }
}

/*************************************************************************************
 * Copyright (c) Microsoft Corporation.  All rights reserved. 
 *	
 * Description : Doc Sample for warning C6258
 * 259 - Labeled code is unreachable: <expression> & <constant> in switch-expr limits case values to a maximum of <constant>.
 
 * Command line : cl /W4 /analyze /c <file>
 *
 * Author : ravkaur
 *
 * Date created : 04-04-2005
 *
 *************************************************************************************/
#include <stdlib.h>

void f_d()
{
     switch (rand () & 3) {
        case 3:
            /* Reachable */
            break;
        case 4:	//@@@Expects:6259
            /* Not reachable */
            break;
        default:
            break;
    }
}


void f()
{
     switch (rand () & 3) {
        case 3:
            /* Reachable */
            break;
        default:
            break;
    }
}


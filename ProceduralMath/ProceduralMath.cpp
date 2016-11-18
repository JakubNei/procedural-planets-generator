// ProceduralMath.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ProceduralMath.h"


// This is an example of an exported variable
//PROCEDURALMATH_API int nProceduralMath=0;


// This is an example of an exported function.
PROCEDURALMATH_API double GetHeight(MathInstance* instance, double x, double y, double z, int detailLevel)
{
    return 42;
}

// This is the constructor of a class that has been exported.
// see ProceduralMath.h for the class definition
/*CProceduralMath::CProceduralMath()
{
    return;
}*/

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the PROCEDURALMATH_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// PROCEDURALMATH_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef PROCEDURALMATH_EXPORTS
#define PROCEDURALMATH_API __declspec(dllexport)
#else
#define PROCEDURALMATH_API __declspec(dllimport)
#endif

// This class is exported from the ProceduralMath.dll
/*class PROCEDURALMATH_API CProceduralMath {
public:
	CProceduralMath(void);
	// TODO: add your methods here.
};
*/

struct MathInstance {
	int weight;
	double price;
};


//extern PROCEDURALMATH_API int nProceduralMath;
extern "C" {
	PROCEDURALMATH_API MathInstance* MakeInstance()
	{
		return new MathInstance();
	}
	PROCEDURALMATH_API void DestroyInstance(MathInstance* instance)
	{
		delete instance;
	}
	PROCEDURALMATH_API double GetHeight(MathInstance* instance, double x, double y, double z, int detailLevel);
}

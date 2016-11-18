// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>

#define _USE_MATH_DEFINES
#include <string.h>
#include <string>
#include <vector>
#include <list>
#include <map>
#include <stdlib.h>
#include <time.h>
#include <fstream>
#include <sstream>
#include <thread>
#include <iostream>
//#include <atomic>
#include <thread>
#include <mutex>
#include <limits>
#include <random>

#include "Libraries\glm\glm\gtc\matrix_transform.hpp"
#include "Libraries\glm\glm\glm.hpp"

//#include <glm/glm.hpp>
//#include <glm/gtc/matrix_transform.hpp>
//#include <glm/gtx/quaternion.hpp>


typedef unsigned char uchar;
typedef unsigned short ushort;
typedef unsigned char byte;
typedef unsigned long ulong;
//typedef unsigned int uint;


#define foreach for
//#define null NULL
#define var auto

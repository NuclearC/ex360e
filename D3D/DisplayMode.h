#ifndef __DISPLAYMODE_H__
#define __DISPLAYMODE_H__

#include <stdint.h>
#include "SurfaceFormat.h"

typedef struct
{
	int32_t width;
	int32_t height;
	int32_t refreshRate;
	SurfaceFormat::SurfaceFormat format;
}DisplayMode;

#endif
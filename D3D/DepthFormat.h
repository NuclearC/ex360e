#ifndef __DEPTHFORMAT_H__
#define __DEPTHFORMAT_H__

namespace DepthFormat
{
	typedef enum DepthFormat
	{
		Depth15Stencil1 = 0x38,
		Depth16 = 0x36,
		Depth24 = 0x33,
		Depth24Stencil4 = 50,
		Depth24Stencil8 = 0x30,
		Depth24Stencil8Single = 0x31,
		Depth32 = 0x34,
	    Unknown = -1
	}DepthFormat;
};


#endif
#ifndef __SURFACEFORMAT_H__
#define __SURFACEFORMAT_H__

#include <d3d9.h>

namespace SurfaceFormat
{
typedef enum
{
    Alpha8 = 15,
    Bgr233 = 0x10,
    Bgr24 = 0x11,
    Bgr32 = 2,
    Bgr444 = 13,
    Bgr555 = 11,
    Bgr565 = 9,
    Bgra1010102 = 3,
    Bgra2338 = 14,
    Bgra4444 = 12,
    Bgra5551 = 10,
    Color = 1,
    Depth15Stencil1 = 0x38,
    Depth16 = 0x36,
    Depth24 = 0x33,
    Depth24Stencil4 = 50,
    Depth24Stencil8 = 0x30,
    Depth24Stencil8Single = 0x31,
    Depth32 = 0x34,
    Dxt1 = 0x1c,
    Dxt2 = 0x1d,
    Dxt3 = 30,
    Dxt4 = 0x1f,
    Dxt5 = 0x20,
    HalfSingle = 0x19,
    HalfVector2 = 0x1a,
    HalfVector4 = 0x1b,
    Luminance16 = 0x22,
    Luminance8 = 0x21,
    LuminanceAlpha16 = 0x24,
    LuminanceAlpha8 = 0x23,
    Multi2Bgra32 = 0x2f,
    NormalizedAlpha1010102 = 0x29,
    NormalizedByte2 = 0x12,
    NormalizedByte2Computed = 0x2a,
    NormalizedByte4 = 0x13,
    NormalizedLuminance16 = 0x27,
    NormalizedLuminance32 = 40,
    NormalizedShort2 = 20,
    NormalizedShort4 = 0x15,
    Palette8 = 0x25,
    PaletteAlpha16 = 0x26,
    Rg32 = 7,
    Rgb32 = 5,
    Rgba1010102 = 6,
    Rgba32 = 4,
    Rgba64 = 8,
    Single = 0x16,
    xUnknown = -1,
    Vector2 = 0x17,
    Vector4 = 0x18,
    VideoGrGb = 0x2d,
    VideoRgBg = 0x2e,
    VideoUyVy = 0x2c,
    VideoYuYv = 0x2b
}SurfaceFormat;

	SurfaceFormat ToXbox(D3DFORMAT fmt)
	{
		switch(fmt)
		{
			case D3DFMT_D24X8:
				return Depth24;
			break;
			case D3DFMT_X8B8G8R8:
				return Bgr32;
				break;

		}
	}

	D3DFORMAT ToPC(SurfaceFormat fmt)
	{
		switch(fmt)
		{
			case Alpha8:
				return D3DFMT_A8;
			break;
			case Color:
				return D3DFMT_A8R8G8B8;
				break;
			case Depth24:
				return D3DFMT_D24X8;
				break;
			case Depth24Stencil8:
				return D3DFMT_D24X8;
				break;
			case Bgr32:
				return D3DFMT_X8B8G8R8;
				break;

		}
	}

}

#endif
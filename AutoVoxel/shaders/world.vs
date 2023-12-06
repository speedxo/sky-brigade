#version 410 core

#define WIDTH 32
#define DEPTH 32

layout(location = 0) in uint vPackedData0;
layout(location = 1) in uint vPackedData1;

uniform mat4 uCameraView;
uniform mat4 uCameraProjection;
uniform vec2 uChunkPosition;

layout(location = 0) out vec2 oTexCoords;
layout(location = 1) out vec3 oNormal;

#define ATLAS_SIZE 256.0
#define SINGLE_TILE_SIZE (16.0 / ATLAS_SIZE)
#define ATLAS_OFFSET 256.0 / 16.0

#define UVCoordinateTopLeft 0
#define UVCoordinateBottomLeft 1
#define UVCoordinateTopRight 2
#define UVCoordinateBottomRight 3

#define CubeFaceFront 0
#define CubeFaceBack 1
#define CubeFaceLeft 2
#define CubeFaceRight 3
#define CubeFaceTop 4
#define CubeFaceBottom 5

vec3 unpackPosition(uint packedData) 
{
    vec3 result;

    result.x = float(packedData & 0x3F); // 0 - 4 = x
    result.y = float((packedData >> 6) & 0xFF); // 5 - 9 = y
    result.z = float((packedData >> 14) & 0x3F); // 10 - 14 = z

    return result;
}

vec3 convertNormal(uint normal) 
{
    if (normal == CubeFaceFront) return vec3(0, 0, 1);
    if (normal == CubeFaceBack) return vec3(0, 0, -1);
    if (normal == CubeFaceLeft) return vec3(-1, 0, 0);
    if (normal == CubeFaceRight) return vec3(1, 0, 0);
    if (normal == CubeFaceTop) return vec3(0, 1, 0);
    if (normal == CubeFaceBottom) return vec3(0, -1, 0);

    return vec3(0, 0, 0); 
}

vec2 convertUV(uint uv) 
{
    if (uv == UVCoordinateTopLeft) return vec2(0, 1);
    if (uv == UVCoordinateBottomLeft) return vec2(0, 0);
    if (uv == UVCoordinateTopRight) return vec2(1, 1);
    if (uv == UVCoordinateBottomRight) return vec2(1, 0);

    return vec2(0, 0); 
}

vec3 unpackNormal()
{
    return convertNormal((vPackedData0 >> 20) & 0x1F);
}

vec2 unpackTexCoord()
{
    float tileId = float(vPackedData1 & 0xFF);

	return convertUV((vPackedData0 >> 25) & 0x3) * SINGLE_TILE_SIZE + vec2(mod(tileId, ATLAS_OFFSET), tileId / ATLAS_OFFSET) * SINGLE_TILE_SIZE;
}

void main()
{
	oTexCoords = unpackTexCoord();
    oNormal = unpackNormal();

	gl_Position = uCameraProjection * uCameraView * vec4(unpackPosition(vPackedData0) + vec3(uChunkPosition.x * (WIDTH), 0, uChunkPosition.y * (DEPTH)), 1.0);
}
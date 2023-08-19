#version 410 core

#define MAX_SPRITES 750

layout (location = 0) in vec2 vPos;
layout (location = 1) in vec2 vTexCoords;
layout (location = 2) in int vId;

uniform mat4 uView;
uniform mat4 uProjection;
uniform vec2 uSingleFrameSize;

layout(std140) struct SpriteData {
    mat4 modelMatrix;
    int spriteOffset;
    int spriteId;
    int _padding2;
    int _padding3;
};

layout(std140) uniform SpriteUniforms {
    SpriteData data[MAX_SPRITES];
} spriteData;

out vec2 texCoords;

void main()
{
    texCoords = vTexCoords + vec2(uSingleFrameSize.x * float(spriteData.data[vId].spriteOffset), 0);

    // Retrieve the model matrix and ID for the current sprite
    mat4 modelMatrix = spriteData.data[vId].modelMatrix;

    // Transform the vertex position
    gl_Position = uProjection * uView * modelMatrix * vec4(vPos, 0.0, 1.0);
}

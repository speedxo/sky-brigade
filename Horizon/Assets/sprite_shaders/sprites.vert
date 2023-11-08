#version 460

#define MAX_SPRITES 750

layout(location = 0) in vec2 vPos;
layout(location = 1) in vec2 vTexCoords;

uniform mat4 uView;
uniform mat4 uProjection;
uniform vec2 uSingleFrameSize;

struct SpriteData {
  mat4 modelMatrix;
  vec2 spriteOffset;
};

layout(std430) buffer SpriteUniforms 
{
  SpriteData data[MAX_SPRITES];
} spriteData;

out vec2 texCoords;

void main() {
  int vertexId = gl_VertexID / 4;

  // Retrieve the sprite's texture offset and flipping flag
  vec2 spriteOffset = spriteData.data[vertexId].spriteOffset;
  //bool isFlipped = spriteData.data[vId].isFlipped;

  // Calculate the base texture coordinates
  vec2 baseTexCoords = vTexCoords + spriteOffset * uSingleFrameSize;

  // Apply horizontal flipping if necessary
  //texCoords =
  //    isFlipped ? vec2(1.0 - baseTexCoords.x, baseTexCoords.y) : baseTexCoords;
  texCoords = baseTexCoords;
  // Retrieve the model matrix and ID for the current sprite
  mat4 modelMatrix = spriteData.data[vertexId].modelMatrix;

  // Transform the vertex position
  gl_Position = uProjection * uView * modelMatrix * vec4(vPos, 0.0, 1.0);
}

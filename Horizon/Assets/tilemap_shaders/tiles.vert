#version 410 core

layout(location = 0) in vec2 vPos;
layout(location = 1) in vec2 vTexCoords;

layout(location = 2) in vec2 iPos;
layout(location = 3) in vec2 iTexCoords;
layout(location = 4) in vec3 iColor;

uniform mat4 uView;
uniform mat4 uProjection;

out vec2 texCoords;
out vec3 color;

void main() {
  texCoords = vTexCoords + iTexCoords;
  color = iColor;
  gl_Position = uProjection * uView * vec4(vPos + iPos, 0.0, 1.0);
}

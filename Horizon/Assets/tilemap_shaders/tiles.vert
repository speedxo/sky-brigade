#version 410 core

layout(location = 0) in vec2 vPos;
layout(location = 1) in vec2 vTexCoords;

layout(location = 2) in vec2 iPos;
layout(location = 3) in vec2 iTexCoords;
layout(location = 4) in vec3 iColor;

uniform mat4 uView;
uniform mat4 uProjection;
uniform int uDiscard;

out vec2 texCoords;
out vec3 color;
out float shouldDiscard;

void main() {
  texCoords = vTexCoords + iTexCoords;
  color = iColor;
  gl_Position = uProjection * uView * vec4(vPos + iPos, 0.0, 1.0);
  
  switch (uDiscard)
  {
    case 1: shouldDiscard = gl_Position.y < 0.00 ? 1.0 : 0.0; break;
    case 2: shouldDiscard = gl_Position.y > -0.075 ? 1.0 : 0.0; break;
    default: shouldDiscard = 0; break;
  }
}

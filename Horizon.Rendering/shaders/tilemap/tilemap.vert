﻿#version 410 core

layout(location = 0) in vec2 vPos;
layout(location = 1) in vec2 vTexCoords;

layout(location = 2) in vec2 iPos;
layout(location = 3) in vec2 iTexCoords;
layout(location = 4) in vec3 iColor;

uniform mat4 uMvp;
uniform int uDiscard;
uniform float uClipOffset = 0.15f;

out vec2 texCoords;
out vec3 color;
out float shouldDiscard;
out vec3 fragPos;

void main() {
  vec4 worldPos = vec4(vPos + iPos, 0.0, 1.0);
  fragPos = (uMvp * vec4(vPos + iPos, 0.0, 1.0)).xyz;

  texCoords = vTexCoords + iTexCoords;
  color = iColor;
  gl_Position = uMvp * worldPos;
  
  switch (uDiscard)
  {
    case 1: shouldDiscard = gl_Position.y < 0.0 + uClipOffset ? 1.0 : 0.0; break;
    case 2: shouldDiscard = gl_Position.y > 0.0 - uClipOffset ? 1.0 : 0.0; break;
    case 0: default: shouldDiscard = 0; 
  }
}

#version 410 core

in float alive;
in vec2 fragPos;

layout(location = 0) out vec4 AlbedoColor;
layout(location = 1) out vec4 NormalFragPosColor;

uniform vec3 uStartColor;
uniform vec3 uEndColor;

void main() {
  if (alive < 0.0) discard; 
  AlbedoColor = vec4(mix(uEndColor, uStartColor, alive), alive + 0.1);
  NormalFragPosColor = vec4(vec2(0.0), fragPos);
}
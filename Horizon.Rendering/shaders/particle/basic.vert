#version 410 core

layout(location = 0) in vec2 vPos;
layout(location = 1) in vec2 vOffset;
layout(location = 2) in float vAlive;

uniform mat4 uMvp;
uniform mat4 uModel;

out float alive;

void main() {
  alive = vAlive;
  gl_Position = uMvp * vec4(vPos + vOffset, 0.0, 1.0);
}
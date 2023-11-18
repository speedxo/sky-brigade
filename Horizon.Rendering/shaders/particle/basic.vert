#version 410 core

layout(location = 0) in vec2 vPos;
layout(location = 1) in vec2 vOffset;
layout(location = 2) in float vAlive;

uniform mat4 uCameraView;
uniform mat4 uCameraProjection;

uniform mat4 uModel;

out float alive;
out vec2 fragPos;

void main() {
  alive = vAlive;
  gl_Position = uCameraProjection * uCameraView * vec4(vPos + vOffset, 0.0, 1.0);
  fragPos = vPos + vOffset;
}
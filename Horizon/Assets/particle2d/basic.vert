#version 410 core

layout(location = 0) in vec2 vPos;
layout(location = 1) in vec2 vOffset;

uniform mat4 uView;
uniform mat4 uModel;
uniform mat4 uProjection;

void main() {
  gl_Position = uProjection * uView * uModel * vec4(vPos + vOffset, 0.0, 1.0);
}
#version 410 core

layout(location = 0) out vec4 AlbedoColor;
layout(location = 1) out vec4 NormalFragPosColor;

layout(location = 0) in vec2 texCoords;
layout(location = 1) in vec2 fragPos;

uniform sampler2D uTexture;

void main() {
  vec4 tex = texture(uTexture, texCoords);
  if (tex.a < 0.1) discard;
  AlbedoColor = tex;
  NormalFragPosColor = vec4(vec2(0.0), fragPos);
}

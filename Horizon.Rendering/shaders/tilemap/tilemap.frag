#version 410 core

layout(location = 0) out vec4 AlbedoColor;
layout(location = 1) out vec4 NormalFragPosColor;

in vec2 texCoords;
in vec3 color;
in float shouldDiscard;
in vec2 fragPos;

uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;

uniform bool uWireframeEnabled;

void main() {
  AlbedoColor = texture(uTextureAlbedo, texCoords) * vec4(color, 1.0);

  if (shouldDiscard == 1.0 || AlbedoColor.a < 0.1)
    discard;
  
  NormalFragPosColor = vec4(texture(uTextureNormal, texCoords).xy, fragPos);
}

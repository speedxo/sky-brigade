#version 410 core

layout(location = 0) out vec4 AlbedoColor;
layout(location = 1) out vec4 NormalColor;
layout(location = 2) out vec4 FragPosColor;

in vec2 texCoords;
in vec3 color;
in float shouldDiscard;
in vec3 fragPos;

uniform sampler2D uTextureAlbedo;
uniform sampler2D uTextureNormal;

uniform bool uWireframeEnabled;

void main() {
  if (shouldDiscard == 1.0f)
    discard;
    
  AlbedoColor = texture(uTextureAlbedo, texCoords) * vec4(color, 1.0);
  NormalColor = texture(uTextureNormal, texCoords);
  FragPosColor = vec4(fragPos, 1.0);
}

#version 410 core

out vec4 FragColor;

in vec2 texCoords;
in vec3 color;
in float shouldDiscard;

uniform sampler2D uTexture;
uniform bool uWireframeEnabled;

void main() {
  if (shouldDiscard == 1.0f)
    discard;
    
  FragColor = texture(uTexture, texCoords);
  if (uWireframeEnabled)
    FragColor = vec4(1.0f);

  FragColor.rgb *= color;
}

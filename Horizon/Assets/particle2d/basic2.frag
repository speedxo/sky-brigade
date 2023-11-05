#version 460 core

in vec4 partColor;

out vec4 FragColor;

void main() {
  if (partColor.a == 0.0) discard;
  FragColor = partColor;
}
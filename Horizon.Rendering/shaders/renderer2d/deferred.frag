#version 410 core

layout(location = 0) in vec2 texCoords;

uniform sampler2D uTexAlbedo;
uniform sampler2D uTexNormal;
uniform sampler2D uTexFragPos;

uniform vec2 viewPos;

// struct Light {
//     vec2 Position;
//     vec3 Color;
//     float Radius;
// };

// const int NR_LIGHTS = 32;
// uniform Light lights[NR_LIGHTS];

out vec4 FragColor;

void main() {
    // retrieve data from G-buffer
    vec3 Albedo = texture(uTexAlbedo, texCoords).rgb;
    vec2 Normal = texture(uTexNormal, texCoords).rb;
    vec2 FragPos = texture(uTexFragPos, texCoords).rg;
    
    //then calculate lighting as usual
    //vec3 lighting = Albedo;
    // vec2 lightDir = normalize(lights[0].Position - FragPos);
    // float diff = max(dot(Normal, lightDir), 0.0);

    //lighting *= diff;

    FragColor = vec4(Albedo, 1.0);
}

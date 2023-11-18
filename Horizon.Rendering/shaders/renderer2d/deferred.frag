#version 410 core

#include "../aces.glsl"
#include "../desaturate.glsl"

layout(location = 0) in vec2 texCoords;

uniform sampler2D uTexAlbedo;
uniform sampler2D uTexNormalFragPos;
uniform float uAspectRatio;
uniform float uTime;

uniform vec2 viewPos;

struct Light {
    vec2 Position;
    vec3 Color;
    float Radius;
};

const int NR_LIGHTS = 32;
uniform Light lights[NR_LIGHTS];

out vec4 FragColor;

void main() {
    // retrieve data from G-buffer
    vec3 Albedo = texture(uTexAlbedo, texCoords).rgb;
    vec4 NormalFragPos = texture(uTexNormalFragPos, texCoords);
    vec2 Normal = NormalFragPos.rg * 2.0 - 1.0;
    vec2 FragPos = NormalFragPos.ba;
    
    // then calculate lighting as usual
    vec3 lighting = Albedo * 0.2; // hard-coded ambient component
    
    {
        vec2 viewDir = normalize(viewPos - FragPos);
        for(int i = 0; i < NR_LIGHTS; ++i)
        {
            if (length(lights[i].Color) < 0.1) continue;
            float dist = length(lights[i].Position - FragPos);
            if (dist < lights[i].Radius)
            {
                // diffuse
                vec2 lightDir = normalize(lights[i].Position - FragPos);
                vec3 diffuse = lights[i].Color;

                if (length(NormalFragPos.rg) > 0.0)
                    diffuse *= abs(dot(Normal, lightDir));
                
                float constant = 1.0;
                float linear = 0.014;
                float quadratic = 0.00007;

                float attenuation = 1.0 / (constant + linear * dist + 
                    quadratic * (dist * dist)) * (max((1.0 - dist / lights[i].Radius), 0.0));

                lighting += diffuse * attenuation * desaturate(Albedo, 1.0 - attenuation);
            }
        }
    }
    
    FragColor = vec4(aces(lighting), 1.0);
}

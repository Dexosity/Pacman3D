#version 330
 
uniform sampler2D s_texture;

in vec2 v_TexCoord;
in vec3 Normal;

in vec3 FragPos;
in vec3[5] posLight;
in vec3 posEye;

out vec4 Color;
 
vec3 CalculateAllLightSources(vec3 normal, vec3 frag, vec3 eye, vec3 light)
{
	vec3 lightColor = vec3(0.8, 0.8, 1.0);
	
	vec3 LightDirection = normalize(light - frag);
	vec3 EyeDirection = normalize(eye - frag);

	vec3 ambient = 0.12 * lightColor;

	float diff = max(dot(normal, LightDirection), 0.0);
	vec3 diffuse = diff * lightColor;

	vec3 reflectDirection = reflect(-LightDirection, normal);
	float spec = pow(max(dot(EyeDirection, reflectDirection), 0.0), 128);
	vec3 specular = 0.5 * spec * lightColor;

	return (ambient + diffuse + specular);
}

void main()
{
	
	vec3 norm = normalize(Normal);

	vec3 result = vec3(0.0);

	for(int i = 0; i < 5; i++)
	{
		result += CalculateAllLightSources(norm, FragPos, posEye, posLight[i]) * 0.5;
	}
	

	Color = vec4(result, 1) * vec4(vec3(texture2D(s_texture, v_TexCoord)), 1.0);
}

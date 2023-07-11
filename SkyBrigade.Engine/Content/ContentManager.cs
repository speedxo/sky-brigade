using System;
using System.IO;
using SkyBrigade.Engine.Logging;
using SkyBrigade.Engine.OpenGL;

namespace SkyBrigade.Engine.Content
{
	public class ContentManager : IDisposable
	{
		private Dictionary<string, Texture> namedTextures;
		private Dictionary<string, Shader> namedShaders;

		private Dictionary<string, Texture> unnamedTextures;
        private Dictionary<string, Shader> unnamedShaders;

        public ContentManager()
		{
			namedTextures = new Dictionary<string, Texture>();
			namedShaders = new Dictionary<string, Shader>();

			unnamedShaders = new Dictionary<string, Shader>();
			unnamedTextures = new Dictionary<string, Texture>();
		}

		/*	The reason we use custom or manual code to get and set different 
		 *	type of content is to have the ability to log and debug each step.
		 *	
		 *	Later on we will add a logging system and keping track of when
		 *	unmanaged objects are created, accessed or destroyed is userful for
		 *  debugging and fixing bugs that occur on other systems.
		 */

		#region Textures

		public Texture LoadTexture(string path)
		{
			if (!unnamedTextures.ContainsKey(path)) unnamedTextures.Add(path, new Texture(GameManager.Instance.Gl, path));
			else GameManager.Instance.Logger.Log(LogLevel.Warning, $"An attempt to load Texture({path}) was made even though an instance of Texture({path}) already exists, a reference to the already loaded texture will be returned.");
				
            return unnamedTextures[path];
        }

        public Texture GenerateNamedTexture(string name, Texture texture)
		{
			if (!namedTextures.ContainsKey(name)) namedTextures.Add(name, texture);
			else throw new ArgumentException("Key already in use. stupid.");

			
			return namedTextures[name];
		}

		public Texture GenerateNamedTexture(string name, string path) => GenerateNamedTexture(name, new Texture(GameManager.Instance.Gl, path));
		public Texture GenerateNamedTexture(string name, Span<byte> data, uint w, uint h) => GenerateNamedTexture(name, new Texture(GameManager.Instance.Gl, data, w, h));
		public Texture GetTexture(string name) => namedTextures.ContainsKey(name) ? namedTextures[name] : throw new Exception($"Key {name} not found in stored textures.");
        #endregion
        #region Shaders
		public Shader LoadShader(string vertexPath, string fragmentPath)
		{
			// todo: anything but _this_
			string uniqueKey = vertexPath + fragmentPath;

			if (!unnamedShaders.ContainsKey(uniqueKey)) unnamedShaders.Add(uniqueKey, new Shader(GameManager.Instance.Gl, vertexPath, fragmentPath));
            else Console.WriteLine($"An attempt to load Shader({vertexPath}, {fragmentPath}) was made even though an instance of Shader({vertexPath}, {fragmentPath}) already exists, a reference to the already loaded shader will be returned.");

			return unnamedShaders[uniqueKey];
        }
        public Shader GenerateNamedShader(string name, Shader shader)
		{
            if (namedShaders.ContainsKey(name)) throw new ArgumentException("Key already in use. stupid.");

            namedShaders.Add(name, shader);
            return namedShaders[name];

        }
		public Shader GenerateNamedShader(string name, string vertexPath, string fragmentPath) => GenerateNamedShader(name, new Shader(GameManager.Instance.Gl, vertexPath, fragmentPath));
		public Shader GetShader(string name) => namedShaders.ContainsKey(name) ? namedShaders[name] : throw new Exception($"Key {name} not found in stored shaders.");
        #endregion

		/*  Make sure we cleanup nicely. 
		 *  update: yea right
		 *  TODO: fix
		 */
        public void Dispose()
        {
			foreach (var item in namedShaders.Values)
				item.Dispose();
            foreach (var item in namedTextures.Values)
                item.Dispose();
            foreach (var item in unnamedTextures.Values)
                item.Dispose();
            foreach (var item in unnamedShaders.Values)
                item.Dispose();
        }
    }
}


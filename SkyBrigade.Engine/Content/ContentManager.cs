using Horizon.GameEntity;
using Horizon.Logging;
using Horizon.OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Horizon.Content
{
    public class ContentManager : Entity, IDisposable
    {
        private Dictionary<string, Texture> namedTextures;
        private Dictionary<string, Shader> namedShaders;
        private Dictionary<string, Texture> unnamedTextures;
        private Dictionary<string, Shader> unnamedShaders;
        private List<Texture> unmanagedTextures;

        public ContentManager()
        {
            namedTextures = new Dictionary<string, Texture>();
            namedShaders = new Dictionary<string, Shader>();

            unnamedShaders = new Dictionary<string, Shader>();
            unnamedTextures = new Dictionary<string, Texture>();

            unmanagedTextures = new List<Texture>();
        }

        public int TotalTextures { get => namedTextures.Count + unnamedTextures.Count + unmanagedTextures.Count; }
        public int TotalShaders { get => namedShaders.Count + unnamedShaders.Count; }

        public IEnumerable<Texture> GetTextures()
        {
            foreach (var texture in namedTextures.Values)
            {
                yield return texture;
            }

            foreach (var texture in unnamedTextures.Values)
            {
                yield return texture;
            }

            foreach (var texture in unmanagedTextures)
            {
                yield return texture;
            }
        }

        public IEnumerable<Shader> GetShaders()
        {
            foreach (var shader in namedShaders.Values)
            {
                yield return shader;
            }

            foreach (var shader in unnamedShaders.Values)
            {
                yield return shader;
            }
        }



        #region Textures

        public Texture LoadTexture(string path)
        {
            string internedPath = string.Intern(path);

            if (!unnamedTextures.TryGetValue(internedPath, out var texture))
            {
                texture = new Texture(internedPath);
                unnamedTextures.TryAdd(texture.Path, texture);
            }
            else
            {
                GameManager.Instance.Logger.Log(LogLevel.Warning, $"An attempt to load Texture({path}) was made even though an instance of Texture({path}) already exists, a reference to the already loaded texture will be returned.");
            }

            return texture;
        }

        public Texture GenerateNamedTexture(string name, Texture texture)
        {
            string internedName = string.Intern(name);

            namedTextures.TryAdd(internedName, texture);
            texture.Name = name;

            return texture;
        }

        public Texture GenerateNamedTexture(string name, string path)
        {
            string internedName = string.Intern(name);
            string internedPath = string.Intern(path);

            if (File.Exists(internedPath))
                return GenerateNamedTexture(internedName, new Texture(internedPath));

            return GenerateNamedTexture(internedName, GetTexture("debug"));
        }

        public Texture GenerateNamedTexture(string name, Span<byte> data, uint w, uint h)
        {
            string internedName = string.Intern(name);
            return GenerateNamedTexture(internedName, new Texture(data, w, h));
        }

        public Texture GetTexture(string name)
        {
            string internedName = string.Intern(name);
            return namedTextures.TryGetValue(internedName, out var texture) ? texture : throw new Exception($"Key {internedName} not found in stored textures.");
        }

        #endregion Textures

        #region Shaders
        public Shader LoadShader(string vertexPath, string fragmentPath)
        {
            string internedVertexPath = string.Intern(vertexPath);
            string internedFragmentPath = string.Intern(fragmentPath);
            string uniqueKey = internedVertexPath + internedFragmentPath;

            if (!unnamedShaders.TryGetValue(uniqueKey, out var shader))
            {
                shader = Shader.CompileShader(internedVertexPath, internedFragmentPath);
                unnamedShaders.TryAdd(uniqueKey, shader);
            }
            else
            {
                GameManager.Instance.Logger.Log(LogLevel.Error, $"An attempt to load Shader({internedVertexPath}, {internedFragmentPath}) was made even though an instance of Shader({internedVertexPath}, {internedFragmentPath}) already exists, a reference to the already loaded shader will be returned.");
            }

            return shader;
        }

        public Shader LoadShaderFromSource(string vertSource, string fragSource)
        {;
            string uniqueKey = (vertSource.GetHashCode() + fragSource.GetHashCode()).ToString().GetHashCode().ToString();

            if (!unnamedShaders.TryGetValue(uniqueKey, out var shader))
            {
                shader = Shader.CompileShaderFromSource(vertSource, fragSource);
                unnamedShaders.TryAdd(uniqueKey, shader);
            }
            else
            {
                GameManager.Instance.Logger.Log(LogLevel.Error, $"Whoah partner, you just tried to compile the same shader twice?");
            }

            return shader;
        }

        public Shader GenerateNamedShader(string name, Shader shader)
        {
            string internedName = string.Intern(name);

            namedShaders.TryAdd(internedName, shader);

            return shader;
        }

        public Shader GenerateNamedShader(string name, string vertexPath, string fragmentPath)
        {
            string internedName = string.Intern(name);
            string internedVertexPath = string.Intern(vertexPath);
            string internedFragmentPath = string.Intern(fragmentPath);

            return GenerateNamedShader(internedName, Shader.CompileShader(internedVertexPath, internedFragmentPath));
        }

        public Shader GetShader(string name)
        {
            string internedName = string.Intern(name);
            return namedShaders.TryGetValue(internedName, out var shader) ? shader : throw new Exception($"Key {internedName} not found in stored shaders.");
        }


        #endregion Shaders

        // Implement IDisposable pattern
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (var item in namedShaders.Values)
                        item.Dispose();
                    namedShaders.Clear();

                    foreach (var item in namedTextures.Values)
                        item.Dispose();
                    namedTextures.Clear();

                    foreach (var item in unnamedTextures.Values)
                        item.Dispose();
                    unnamedTextures.Clear();

                    foreach (var item in unnamedShaders.Values)
                        item.Dispose();
                    unnamedShaders.Clear();

                    foreach (var item in unmanagedTextures)
                        item.Dispose();
                    unmanagedTextures.Clear();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Implement finalizer as backup for manual disposal
        ~ContentManager()
        {
            Dispose(false);
        }

        public void DeleteTexture(string name)
        {
            if (!namedTextures.Remove(name, out var texture))
                GameManager.Instance.Logger.Log(LogLevel.Error, $"Attempt to delete nonexistent Texture({name})");
            else texture?.Dispose();
        }

        public void DeleteShader(string name)
        {
            if (!namedShaders.Remove(name, out var shader))
                GameManager.Instance.Logger.Log(LogLevel.Error, $"Attempt to delete nonexistent Shader({name})");
            else shader?.Dispose();
        }

        public Texture AddUnmanagedTexture(uint texture)
        {
            unmanagedTextures.Add(new Texture(texture));

            return unmanagedTextures.Last();
        }

        public Texture AddTexture(Texture texture)
        {
            unnamedTextures.TryAdd(texture.Handle.ToString(), texture);
            return unnamedTextures[texture.Handle.ToString()];
        }
    }
}

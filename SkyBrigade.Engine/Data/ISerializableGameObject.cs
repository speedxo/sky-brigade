namespace SkyBrigade.Engine.Data;

public interface ISerializableGameObject
{
    void Save(string path);

    void Load(string path);
}
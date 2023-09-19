using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;
using ImGuiNET;
using System.Collections;
using System.Numerics;
using System.Reflection;

namespace Horizon.Debugging.Debuggers
{
    public class SceneEntityDebugger : DebuggerComponent
    {
        private const float V_speed = 0.05f;
        public bool DebugInstance = false;

        public override void Initialize()
        {
            Name = "Scene Tree";
        }

        public override void Draw(float dt, RenderOptions? options = null)
        {
            if (!Visible)
                return;

            if (ImGui.Begin(Name))
            {
                Entity mainNode = DebugInstance
                    ? GameManager.Instance
                    : GameManager.Instance.GameScreenManager.GetCurrentInstance();

                ImGui.Text($"Total Entities: {mainNode.TotalEntities}");
                ImGui.Text($"Total Components: {mainNode.TotalComponents}");

                ImGui.Separator();
                ImGui.NewLine();

                DrawEntityTree(mainNode);

                ImGui.End();
            }
        }

        private void DrawEntityTree(IEntity? entity)
        {
            if (entity is null)
                return;

            ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f);

            ImGui.OpenPopupOnItemClick($"EntityContextMenu_{entity.Name}{entity.ID}");

            if (
                ImGui.TreeNodeEx(
                    entity.Name ??= entity.GetType().Name,
                    ImGuiTreeNodeFlags.DefaultOpen
                )
            )
            {
                if (ImGui.BeginPopupContextItem($"EntityContextMenu_{entity.Name}{entity.ID}"))
                {
                    if (ImGui.MenuItem("Remove Entity"))
                    {
                        entity?.Parent?.Entities.Remove(entity);
                    }
                    ImGui.EndPopup();
                }

                ImGui.Columns(2, "EntityColumns", false);
                ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() * 0.4f);

                ImGui.Text("Property");
                ImGui.NextColumn();
                ImGui.Text("Value");
                ImGui.NextColumn();

                ImGui.Separator();

                DrawProperties(entity, 1);

                ImGui.Columns(1);

                foreach (IGameComponent? component in entity!.Components.Values)
                {
                    if (component == null)
                        continue;

                    if (ImGui.TreeNodeEx(component.Name ??= component.GetType().Name))
                    {
                        if (ImGui.BeginPopupContextItem($"{component.Name}"))
                        {
                            if (ImGui.MenuItem("Delete"))
                            {
                                entity.Components.Remove(component.GetType());
                                // TODO: some king of disposing.tho
                                ImGui.CloseCurrentPopup();
                            }

                            ImGui.EndPopup();
                        }

                        DrawProperties(component, 1);
                        ImGui.TreePop();
                    }
                }

                for (int i = 0; i < entity.Entities.Count; i++)
                    DrawEntityTree(entity.Entities[i]);

                ImGui.TreePop();
            }

            ImGui.PopStyleVar();
            ImGui.NewLine();
        }

        public static void DrawProperties(object? component, int depth = 0, int maxDepth = 3)
        {
            if (component is null || depth > maxDepth)
            {
                ImGui.Text("Depth Limit Reached.");
                return;
            }

            Type type = component.GetType();
            var properties = type
                .GetProperties()
                .Where(p => p.GetIndexParameters().Length == 0)
                .ToArray();
            try
            {
                foreach (var property in properties)
                {
                    if (property is null || property.IsCollectible)
                        continue;

                    object? value = property.GetValue(component);

                    if (property.GetMethod?.IsStatic == true)
                    {
                        continue; // Skip static properties
                    }

                    if (property.PropertyType.IsArray)
                    {
                        DrawArrayProperty(property.Name, (Array)value!);
                    }
                    else if (
                        property.PropertyType.IsGenericType
                        && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                    )
                    {
                        if (value is null)
                            continue;

                        DrawListProperty(property.Name, value);
                    }
                    else if (
                        property.PropertyType.IsGenericType
                        && property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                    )
                    {
                        if (value is null)
                            continue;

                        DrawDictionaryProperty(property.Name, value);
                    }
                    else if (property.PropertyType.IsValueType)
                    {
                        if (
                            property.PropertyType.Namespace is null
                            || property.PropertyType.Namespace.StartsWith("System")
                        )
                            continue;

                        if (ImGui.TreeNodeEx($"{property.Name} (Value Type)"))
                        {
                            DrawProperties(value, depth + 1);
                            ImGui.TreePop();
                        }
                    }
                    else if (!property.CanWrite || value == null)
                    {
                        DrawPropertyRow(property.Name, $"{value}");
                    }
                    else if (value is int intValue)
                    {
                        if (ImGui.DragInt(property.Name, ref intValue, V_speed))
                            property.SetValue(component, intValue);
                    }
                    else if (value is string stringValue)
                    {
                        DrawPropertyRow(property.Name, $"\"{stringValue}\"");
                    }
                    else if (value is float floatValue)
                    {
                        if (ImGui.DragFloat(property.Name, ref floatValue, V_speed))
                        {
                            property.SetValue(component, floatValue);
                        }
                    }
                    else if (value is Vector2 vector2Value)
                    {
                        if (ImGui.DragFloat2(property.Name, ref vector2Value, V_speed))
                        {
                            property.SetValue(component, vector2Value);
                        }
                    }
                    else if (value is Vector3 vector3Value)
                    {
                        if (ImGui.DragFloat3(property.Name, ref vector3Value, V_speed))
                        {
                            property.SetValue(component, vector3Value);
                        }
                    }
                    else if (value is Vector4 vector4Value)
                    {
                        if (ImGui.DragFloat4(property.Name, ref vector4Value, V_speed))
                        {
                            property.SetValue(component, vector4Value);
                        }
                    }
                    else if (value is bool boolValue)
                    {
                        if (ImGui.Checkbox(property.Name, ref boolValue))
                        {
                            property.SetValue(component, boolValue);
                        }
                    }
                    else
                    {
                        DrawPropertyRow(property.Name, $"{GetFriendlyName(value)}");
                    }
                }
            }
            catch { 
                ImGui.Text("EISH MY MAN");
                throw;
            }
        }

        private static void DrawPropertyRow(string propertyName, string propertyValue)
        {
            ImGui.Columns(2, propertyName, false);
            ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() * 0.4f);

            ImGui.Text(propertyName);
            ImGui.NextColumn();
            ImGui.Text(propertyValue);
            ImGui.NextColumn();

            ImGui.Columns(1);
        }

        private static void DrawListProperty(string name, object listObj)
        {
            if (listObj is IList list)
            {
                if (list.Count < 1)
                    return;

                if (ImGui.TreeNode($"{name} (List)"))
                {
                    ImGui.Columns(2, name, false);
                    ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() * 0.4f);

                    ImGui.Text(name);
                    ImGui.NextColumn();
                    ImGui.Text("(List)");

                    ImGui.NextColumn();

                    for (int i = 0; i < list.Count; i++)
                    {
                        object? element = list[i];

                        if (element != null)
                        {
                            ImGui.Columns(2, $"Element {i}", false);
                            ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() * 0.4f);

                            ImGui.Text($"Element {i}");
                            ImGui.NextColumn();

                            if (element is int intValue)
                            {
                                if (ImGui.DragInt("", ref intValue, V_speed))
                                {
                                    list[i] = intValue;
                                }
                            }
                            else if (element is float floatValue)
                            {
                                if (ImGui.DragFloat("", ref floatValue, V_speed))
                                {
                                    list[i] = floatValue;
                                }
                            }
                            else if (element is Vector2 vectorValue2)
                            {
                                if (ImGui.DragFloat2("", ref vectorValue2, V_speed))
                                {
                                    list[i] = vectorValue2;
                                }
                            }
                            else if (element is Vector3 vectorValue3)
                            {
                                if (ImGui.DragFloat3("", ref vectorValue3, V_speed))
                                {
                                    list[i] = vectorValue3;
                                }
                            }
                            else if (element is Vector4 vectorValue4)
                            {
                                if (ImGui.DragFloat4("", ref vectorValue4, V_speed))
                                {
                                    list[i] = vectorValue4;
                                }
                            }
                            else if (element is bool boolValue)
                            {
                                if (ImGui.Checkbox("", ref boolValue))
                                {
                                    list[i] = boolValue;
                                }
                            }
                            else
                            {
                                ImGui.Text($"{GetFriendlyName(element)}");
                            }

                            ImGui.Columns(1);
                        }
                    }
                    ImGui.Columns(1);
                    ImGui.TreePop();
                }
            }
        }

        private static void DrawArrayProperty(string name, Array array)
        {
            if (array.Length < 1)
                return;

            ImGui.Columns(2, name, false);
            ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() * 0.4f);

            ImGui.Text(name);
            ImGui.NextColumn();
            ImGui.Text("(Array)");
            ImGui.NextColumn();

            for (int i = 0; i < array.Length; i++)
            {
                object? element = array.GetValue(i);

                if (element != null)
                {
                    ImGui.Columns(2, $"Element {i}", false);
                    ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() * 0.4f);

                    ImGui.Text($"Element {i}");
                    ImGui.NextColumn();

                    if (element is int intValue)
                    {
                        if (ImGui.DragInt("", ref intValue, V_speed))
                        {
                            array.SetValue(intValue, i);
                        }
                    }
                    else if (element is float floatValue)
                    {
                        if (ImGui.DragFloat("", ref floatValue, V_speed))
                        {
                            array.SetValue(floatValue, i);
                        }
                    }
                    else if (element is Vector2 vectorValue2)
                    {
                        if (ImGui.DragFloat2("", ref vectorValue2, V_speed))
                        {
                            array.SetValue(vectorValue2, i);
                        }
                    }
                    else if (element is Vector3 vectorValue3)
                    {
                        if (ImGui.DragFloat3("", ref vectorValue3, V_speed))
                        {
                            array.SetValue(vectorValue3, i);
                        }
                    }
                    else if (element is bool boolValue)
                    {
                        if (ImGui.Checkbox("", ref boolValue))
                        {
                            array.SetValue(boolValue, i);
                        }
                    }
                    else
                        ImGui.Text($"{GetFriendlyName(element)}");

                    ImGui.Columns(1);
                }
            }

            ImGui.Columns(1);
        }

        public static string GetFriendlyName(object? obj)
        {
            if (obj == null)
                return string.Empty;

            var type = obj.GetType();

            return $"{(type.IsClass ? type.Name : obj.ToString())}";
        }

        private static void DrawDictionaryProperty(string name, object dictionaryObj)
        {
            if (dictionaryObj is IDictionary dictionary)
            {
                if (dictionary.Count < 1)
                    return;

                if (ImGui.TreeNodeEx($"{name} (Dictionary)"))
                {
                    ImGui.Columns(2, name, false);
                    ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() * 0.4f);

                    ImGui.Text("Key");
                    ImGui.NextColumn();
                    ImGui.Text("Value");
                    ImGui.NextColumn();

                    ImGui.Separator();

                    foreach (DictionaryEntry entry in dictionary)
                    {
                        ImGui.Columns(2, $"{entry.Key?.GetType().Name}", false);
                        ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() * 0.4f);

                        ImGui.Text($"{GetFriendlyName(entry.Key)}");
                        ImGui.NextColumn();

                        ImGui.Text($"{GetFriendlyName(entry.Value)}");
                        ImGui.NextColumn();

                        ImGui.Columns(1);
                    }

                    ImGui.TreePop();
                }
            }
        }

        public override void Update(float dt) { }
    }
}

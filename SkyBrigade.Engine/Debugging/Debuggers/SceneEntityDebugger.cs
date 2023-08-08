using System;
using System.Collections;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Debugging.Debuggers
{
    public class SceneEntityDebugger : IGameComponent
    {
        public string Name { get; set; }
        public bool DebugInstance = false;

        public Entity Parent { get; set; }
        private Debugger Debugger { get; set; }

        // TODO: use string interns here because there are thousands of string allocations/second here
        public void Initialize()
        {
            Debugger = Parent as Debugger;
        }

        public bool Visible = false;

        public void Draw(float dt, RenderOptions? options = null)
        {
            if (!Visible) return;

            if (ImGui.Begin("Scene Tree"))
            {
                if (ImGui.TreeNode("Scene"))
                {
                    (DebugInstance ? GameManager.Instance.Entities : GameManager.Instance.GameScreenManager.GetCurrentInstance().Entities).ForEach(DrawEntityTree);

                    ImGui.TreePop();
                }

                ImGui.End();
            }
        }

        static void DrawEntityTree(IEntity? entity)
        {
            if (entity is null) return;

            ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f);

            if (ImGui.TreeNodeEx(entity.Name, ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Columns(2, "EntityColumns", false);
                ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() * 0.4f);

                ImGui.Text("Property");
                ImGui.NextColumn();
                ImGui.Text("Value");
                ImGui.NextColumn();

                ImGui.Separator();

                DrawProperties(entity, 1);

                ImGui.Columns(1);

                foreach (IGameComponent? component in entity.Components.Values)
                {
                    if (component == null) continue;

                    if (ImGui.TreeNodeEx(component.Name))
                    {
                        DrawProperties(component, 1);

                        ImGui.TreePop();
                    }
                }

                foreach (IEntity? child in entity.Entities)
                    DrawEntityTree(child);

                ImGui.TreePop();
            }

            ImGui.PopStyleVar();
            ImGui.NewLine();
        }


        static void DrawProperties(object component, int depth = 0, int maxDepth = 3)
        {
            if (depth > maxDepth)
            {
                ImGui.Text("Depth Limit Reached.");
                return;
            }

            Type type = component.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (var property in properties)
            {
                object? value = property.GetValue(component);

                if (property.GetMethod?.IsStatic == true)
                {
                    continue; // Skip static properties
                }

                if (property.PropertyType.IsArray)
                {
                    DrawArrayProperty(property.Name, value as Array);
                }
                else if (property.PropertyType.IsGenericType &&
                         property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    DrawListProperty(property.Name, value);
                }
                else if (property.PropertyType.IsGenericType &&
                         property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    DrawDictionaryProperty(property.Name, value);
                }
                else if (property.PropertyType.IsValueType && !property.PropertyType.Namespace.StartsWith("System"))
                {
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
                    if (ImGui.DragInt(property.Name, ref intValue, 0.1f))
                        property.SetValue(component, intValue);
                }
                else if (value is string stringValue)
                {
                    DrawPropertyRow(property.Name, $"\"{stringValue}\"");
                }
                else if (value is float floatValue)
                {
                    if (ImGui.DragFloat(property.Name, ref floatValue, 0.1f))
                    {
                        property.SetValue(component, floatValue);
                    }
                }
                else if (value is Vector2 vector2Value)
                {
                    if (ImGui.DragFloat2(property.Name, ref vector2Value, 0.1f))
                    {
                        property.SetValue(component, vector2Value);
                    }
                }
                else if (value is Vector3 vector3Value)
                {
                    if (ImGui.DragFloat3(property.Name, ref vector3Value, 0.1f))
                    {
                        property.SetValue(component, vector3Value);
                    }
                }
                else if (value is Vector4 vector4Value)
                {
                    if (ImGui.DragFloat4(property.Name, ref vector4Value, 0.1f))
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

        static void DrawPropertyRow(string propertyName, string propertyValue)
        {
            ImGui.Columns(2, propertyName, false);
            ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() * 0.4f);

            ImGui.Text(propertyName);
            ImGui.NextColumn();
            ImGui.Text(propertyValue);
            ImGui.NextColumn();

            ImGui.Columns(1);
        }


        static void DrawListProperty(string name, object listObj)
        {
            if (listObj is IList list)
            {
                if (list.Count < 1) return;

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
                            if (ImGui.DragInt("", ref intValue, 0.1f))
                            {
                                list[i] = intValue;
                            }
                        }
                        else if (element is float floatValue)
                        {
                            if (ImGui.DragFloat("", ref floatValue, 0.1f))
                            {
                                list[i] = floatValue;
                            }
                        }
                        else if (element is Vector2 vectorValue2)
                        {
                            if (ImGui.DragFloat2("", ref vectorValue2, 0.1f))
                            {
                                list[i] = vectorValue2;
                            }
                        }
                        else if (element is Vector3 vectorValue3)
                        {
                            if (ImGui.DragFloat3("", ref vectorValue3, 0.1f))
                            {
                                list[i] = vectorValue3;
                            }
                        }
                        else if (element is Vector4 vectorValue4)
                        {
                            if (ImGui.DragFloat4("", ref vectorValue4, 0.1f))
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
            }
        }

        static void DrawArrayProperty(string name, Array array)
        {
            if (array.Length < 1) return;

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
                        if (ImGui.DragInt("", ref intValue, 0.1f))
                        {
                            array.SetValue(intValue, i);
                        }
                    }
                    else if (element is float floatValue)
                    {
                        if (ImGui.DragFloat("", ref floatValue, 0.1f))
                        {
                            array.SetValue(floatValue, i);
                        }
                    }
                    else if (element is Vector2 vectorValue2)
                    {
                        if (ImGui.DragFloat2("", ref vectorValue2, 0.1f))
                        {
                            array.SetValue(vectorValue2, i);
                        }
                    }
                    else if (element is Vector3 vectorValue3)
                    {
                        if (ImGui.DragFloat3("", ref vectorValue3, 0.1f))
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
                    else ImGui.Text($"{GetFriendlyName(element)}");


                    ImGui.Columns(1);
                }
            }

            ImGui.Columns(1);
        }
        public static string GetFriendlyName(object? obj)
        {
            if (obj == null) return string.Empty;

            var type = obj.GetType();

            return $"{(type.IsClass ? type.Name : obj.ToString())}";
        }
        static void DrawDictionaryProperty(string name, object dictionaryObj)
        {
            if (dictionaryObj is IDictionary dictionary)
            {
                if (dictionary.Count < 1) return;

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

        public void Update(float dt)
        {

        }
    }
}


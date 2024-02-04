using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TerrainParts.Editor
{
    [CustomPropertyDrawer(typeof(TerrainPartsBasicData))]
    public class TerrainPartsBasicDataDrawer : PropertyDrawer
    {
        private struct ToolCategoryChangedArgs
        {
            public readonly ToolCategory TargetToolCategory;
            public readonly SerializedProperty SerializedProperty;

            public ToolCategoryChangedArgs(ToolCategory targetToolCategory, SerializedProperty serializedProperty)
            {
                TargetToolCategory = targetToolCategory;
                SerializedProperty = serializedProperty;
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            const float indentSize = 15f;

            var container = new VisualElement();

            var toolCategoryFlags = (ToolCategory)property.FindPropertyRelative("ToolCategory").enumValueFlag;
            var enableHeightToggle = new Toggle("Edit Height");
            enableHeightToggle.value = toolCategoryFlags.HasFlagAll(ToolCategory.Height);
            enableHeightToggle.RegisterCallback<ChangeEvent<bool>, ToolCategoryChangedArgs>(OnToolCategoryChanged, new ToolCategoryChangedArgs(ToolCategory.Height, property));
            container.Add(enableHeightToggle);
            var heightPropertyElement = new VisualElement();
            heightPropertyElement.style.display = enableHeightToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
            heightPropertyElement.style.paddingLeft = indentSize;
            heightPropertyElement.Add(new PropertyField(property.FindPropertyRelative("HeightWriteCondition"), "WriteCondition"));
            enableHeightToggle.RegisterCallback<ChangeEvent<bool>>(v => heightPropertyElement.style.display = v.newValue ? DisplayStyle.Flex : DisplayStyle.None);
            container.Add(heightPropertyElement);

            var enableTextureToggle = new Toggle("Edit Texture");
            enableTextureToggle.value = toolCategoryFlags.HasFlagAll(ToolCategory.Texture);
            enableTextureToggle.RegisterCallback<ChangeEvent<bool>, ToolCategoryChangedArgs>(OnToolCategoryChanged, new ToolCategoryChangedArgs(ToolCategory.Texture, property));
            container.Add(enableTextureToggle);
            var texturePropertyElement = new VisualElement();
            texturePropertyElement.style.display = enableHeightToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
            texturePropertyElement.style.paddingLeft = indentSize;
            texturePropertyElement.Add(new PropertyField(property.FindPropertyRelative("TextureLayerIndex"), "LayerIndex"));
            texturePropertyElement.Add(new PropertyField(property.FindPropertyRelative("TextureLayerStrength"), "Strength"));
            enableTextureToggle.RegisterCallback<ChangeEvent<bool>>(v => texturePropertyElement.style.display = v.newValue ? DisplayStyle.Flex : DisplayStyle.None);
            container.Add(texturePropertyElement);


            var enableHoleToggle = new Toggle("Edit Hole");
            enableHoleToggle.value = toolCategoryFlags.HasFlagAll(ToolCategory.Hole);
            enableHoleToggle.RegisterCallback<ChangeEvent<bool>, ToolCategoryChangedArgs>(OnToolCategoryChanged, new ToolCategoryChangedArgs(ToolCategory.Hole, property));
            container.Add(enableHoleToggle);
            var holePropertyElement = new VisualElement();
            holePropertyElement.style.display = enableHoleToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
            holePropertyElement.style.paddingLeft = indentSize;
            holePropertyElement.Add(new PropertyField(property.FindPropertyRelative("HoleThreshold"), "HoleThreshold"));
            enableHoleToggle.RegisterCallback<ChangeEvent<bool>>(v => holePropertyElement.style.display = v.newValue ? DisplayStyle.Flex : DisplayStyle.None);
            container.Add(holePropertyElement);

            var enableTreeToggle = new Toggle("Edit Tree");
            enableTreeToggle.value = toolCategoryFlags.HasFlagAll(ToolCategory.Tree);
            enableTreeToggle.RegisterCallback<ChangeEvent<bool>, ToolCategoryChangedArgs>(OnToolCategoryChanged, new ToolCategoryChangedArgs(ToolCategory.Tree, property));
            container.Add(enableTreeToggle);
            var treePropertyElement = new VisualElement();
            treePropertyElement.style.display = enableTreeToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
            treePropertyElement.style.paddingLeft = indentSize;
            treePropertyElement.Add(new PropertyField(property.FindPropertyRelative("TreePrototypeDataList"), "TreePrototypeData"));
            enableTreeToggle.RegisterCallback<ChangeEvent<bool>>(v => treePropertyElement.style.display = v.newValue ? DisplayStyle.Flex : DisplayStyle.None);
            container.Add(treePropertyElement);

            var enableDetailToggle = new Toggle("Edit Detail");
            enableDetailToggle.value = toolCategoryFlags.HasFlagAll(ToolCategory.Detail);
            enableDetailToggle.RegisterCallback<ChangeEvent<bool>, ToolCategoryChangedArgs>(OnToolCategoryChanged, new ToolCategoryChangedArgs(ToolCategory.Detail, property));
            container.Add(enableDetailToggle);
            var detailPropertyElement = new VisualElement();
            detailPropertyElement.style.display = enableDetailToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
            detailPropertyElement.style.paddingLeft = indentSize;
            detailPropertyElement.Add(new PropertyField(property.FindPropertyRelative("DetailPrototypeDataList"), "DetailPrototypeData"));
            enableDetailToggle.RegisterCallback<ChangeEvent<bool>>(v => detailPropertyElement.style.display = v.newValue ? DisplayStyle.Flex : DisplayStyle.None);
            container.Add(detailPropertyElement);

            var layerSettingsGuid = EditorUserSettings.GetConfigValue(TerrainPartsEditorDefines.UserLayerSettingsKey);

            if (TryGetLayerSettings(out var layerSettings) && layerSettings.GetLayerNameCount() > 0)
            {
                var layerCount = layerSettings.GetLayerNameCount();
                var layerNames = new List<string>();
                for (var i = 0; i < layerCount; i++)
                {
                    layerNames.Add(layerSettings.GetLayerName(i));
                }
                var popupField = new PopupField<string>("Layer", layerNames, property.FindPropertyRelative("Layer").intValue);
                popupField.RegisterValueChangedCallback(e =>
                {
                    property.serializedObject.Update();
                    property.FindPropertyRelative("Layer").intValue = layerNames.IndexOf(e.newValue);
                    property.serializedObject.ApplyModifiedProperties();
                });
                container.Add(popupField);
            }
            else
            {
                container.Add(new PropertyField(property.FindPropertyRelative("Layer")));
            }

            container.Add(new PropertyField(property.FindPropertyRelative("OrderInLayer")));

            return container;
        }

        private void OnToolCategoryChanged(ChangeEvent<bool> boolEvent, ToolCategoryChangedArgs args)
        {
            args.SerializedProperty.serializedObject.Update();
            var toolCategoryFlags = (ToolCategory)args.SerializedProperty.FindPropertyRelative("ToolCategory").enumValueFlag;
            if (boolEvent.newValue)
            {
                toolCategoryFlags |= args.TargetToolCategory;
            }
            else
            {
                toolCategoryFlags &= ~args.TargetToolCategory;
            }
            args.SerializedProperty.FindPropertyRelative("ToolCategory").enumValueFlag = (int)toolCategoryFlags;
            args.SerializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private static bool TryGetLayerSettings(out TerrainPartsLayerSettings result)
        {
            var layerSettingsGuid = EditorUserSettings.GetConfigValue(TerrainPartsEditorDefines.UserLayerSettingsKey);
            if (string.IsNullOrEmpty(layerSettingsGuid))
            {
                result = null;
                return false;
            }

            var path = AssetDatabase.GUIDToAssetPath(layerSettingsGuid);
            if (string.IsNullOrEmpty(path))
            {
                result = null;
                return false;
            }

            result = AssetDatabase.LoadAssetAtPath<TerrainPartsLayerSettings>(path);
            return result != null;
        }
    }
}

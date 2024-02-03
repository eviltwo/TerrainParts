using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TerrainParts.Editor
{
    [CustomPropertyDrawer(typeof(TreePainterSettings.TreeDetail))]
    public class TreeDetailDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            const float indentSize = 15f;

            var container = new VisualElement();

            var foldout = new Foldout() { text = property.displayName };
            container.Add(foldout);

            var heightRandomToggle = new PropertyField(property.FindPropertyRelative("_isHeightRandom"));
            foldout.Add(heightRandomToggle);

            var rangedHeightElement = new VisualElement();
            var currentHeightRandom = property.FindPropertyRelative("_isHeightRandom").boolValue;
            rangedHeightElement.style.display = currentHeightRandom ? DisplayStyle.Flex : DisplayStyle.None;
            rangedHeightElement.style.paddingLeft = indentSize;
            rangedHeightElement.Add(new PropertyField(property.FindPropertyRelative("_heightRandomMin"), "Min"));
            rangedHeightElement.Add(new PropertyField(property.FindPropertyRelative("_heightRandomMax"), "Max"));
            heightRandomToggle.RegisterCallback<ChangeEvent<bool>>(v => rangedHeightElement.style.display = v.newValue ? DisplayStyle.Flex : DisplayStyle.None);
            foldout.Add(rangedHeightElement);
            var fixedHeightElement = new VisualElement();
            fixedHeightElement.style.display = currentHeightRandom ? DisplayStyle.None : DisplayStyle.Flex;
            fixedHeightElement.style.paddingLeft = indentSize;
            fixedHeightElement.Add(new PropertyField(property.FindPropertyRelative("_heightRandomMin"), "Height"));
            heightRandomToggle.RegisterCallback<ChangeEvent<bool>>(v => fixedHeightElement.style.display = v.newValue ? DisplayStyle.None : DisplayStyle.Flex);
            foldout.Add(fixedHeightElement);

            var lockWidthToHeightToggle = new PropertyField(property.FindPropertyRelative("_isLockWidthToHeight"));
            var currentLockWidthToHeight = property.FindPropertyRelative("_isLockWidthToHeight").boolValue;
            foldout.Add(lockWidthToHeightToggle);

            var widthSettingElement = new VisualElement();
            widthSettingElement.style.display = currentLockWidthToHeight ? DisplayStyle.None : DisplayStyle.Flex;
            lockWidthToHeightToggle.RegisterCallback<ChangeEvent<bool>>(v => widthSettingElement.style.display = v.newValue ? DisplayStyle.None : DisplayStyle.Flex);
            foldout.Add(widthSettingElement);

            var widthRandomToggle = new PropertyField(property.FindPropertyRelative("_isWidthRandom"));
            var currentWidthRandom = property.FindPropertyRelative("_isWidthRandom").boolValue;
            widthSettingElement.Add(widthRandomToggle);

            var rangedWidthElement = new VisualElement();
            rangedWidthElement.style.display = currentWidthRandom ? DisplayStyle.Flex : DisplayStyle.None;
            rangedWidthElement.style.paddingLeft = indentSize;
            rangedWidthElement.Add(new PropertyField(property.FindPropertyRelative("_widthRandomMin"), "Min"));
            rangedWidthElement.Add(new PropertyField(property.FindPropertyRelative("_widthRandomMax"), "Max"));
            widthRandomToggle.RegisterCallback<ChangeEvent<bool>>(v => rangedWidthElement.style.display = v.newValue ? DisplayStyle.Flex : DisplayStyle.None);
            widthSettingElement.Add(rangedWidthElement);
            var fixedWidthElement = new VisualElement();
            fixedWidthElement.style.display = currentWidthRandom ? DisplayStyle.None : DisplayStyle.Flex;
            fixedWidthElement.style.paddingLeft = indentSize;
            fixedWidthElement.Add(new PropertyField(property.FindPropertyRelative("_widthRandomMin"), "Width"));
            widthRandomToggle.RegisterCallback<ChangeEvent<bool>>(v => fixedWidthElement.style.display = v.newValue ? DisplayStyle.None : DisplayStyle.Flex);
            widthSettingElement.Add(fixedWidthElement);

            return container;
        }
    }
}

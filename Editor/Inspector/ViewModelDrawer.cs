using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Voyage.ObservationToolkit.Runtime.ViewModel;

namespace Voyage.ObservationToolkit.Editor.Inspector
{
    [CustomPropertyDrawer(typeof(ViewModel<>), true)]
    public class ViewModelDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            // Style setup for cleaner look
            root.style.marginBottom = 2;
            root.style.marginTop = 2;

            // Main Foldout
            var foldout = new Foldout
            {
                text = property.displayName,
                // Binding "value" to "isExpanded" is tricky with PropertyDrawer directly, 
                // so we rely on viewDataKey for persistence or manual binding if needed.
                // However, Foldout automatically handles its own expanded state UI wise.
                // To sync with SerializedProperty.isExpanded, we'd need more logic, 
                // but for visual purposes, Foldout's internal state is often sufficient 
                // unless other code depends on property.isExpanded.
                viewDataKey = property.propertyPath // Persist state based on property path
            };

            // Enhance Foldout header style
            var toggle = foldout.Q<Toggle>();
            if (toggle != null)
            {
                // 1. Color Scheme: Green Style (Referencing the 'Green' row in the image)
                var borderColor = new Color(0.4f, 0.7f, 0.4f, 1f); // Muted Green
                var backgroundColor = new Color(0.92f, 0.98f, 0.92f, 1f); // Very Light Green
                var textColor = new Color(0.1f, 0.3f, 0.1f, 1f); // Dark Green Text

                // 2. Apply Colors
                toggle.style.backgroundColor = backgroundColor;
                toggle.style.color = textColor;
                
                toggle.style.borderTopColor = borderColor;
                toggle.style.borderBottomColor = borderColor;
                toggle.style.borderLeftColor = borderColor;
                toggle.style.borderRightColor = borderColor;
                toggle.style.borderTopWidth = 1;
                toggle.style.borderBottomWidth = 1;
                toggle.style.borderLeftWidth = 1;
                toggle.style.borderRightWidth = 1;
                toggle.style.borderTopLeftRadius = 4;
                toggle.style.borderTopRightRadius = 4;
                toggle.style.borderBottomLeftRadius = 4;
                toggle.style.borderBottomRightRadius = 4;

                // 3. Spacing & Fonts
                toggle.style.marginBottom = 2;
                toggle.style.marginTop = 2;
                toggle.style.marginLeft = 0;
                toggle.style.marginRight = 0;
                toggle.style.paddingLeft = 6;
                toggle.style.paddingRight = 6;
                toggle.style.paddingTop = 4;
                toggle.style.paddingBottom = 4;
                toggle.style.unityFontStyleAndWeight = FontStyle.Bold;
                
                // 4. Center Arrow and Text
                // We need to target the internal input container to center its children (Checkmark + Label)
                var visualInput = toggle.Q(className: "unity-toggle__input");
                if (visualInput != null)
                {
                    visualInput.style.justifyContent = Justify.Center; // Center horizontally
                    visualInput.style.flexGrow = 1;
                }
                
                // Ensure Label doesn't force alignment to left if it was set previously
                var label = toggle.Q<Label>();
                if (label != null)
                {
                    // Reset flexGrow so it doesn't push the arrow to the side
                    label.style.flexGrow = 0; 
                    label.style.marginLeft = 4; // Add some space between arrow and text
                    label.style.unityTextAlign = TextAnchor.MiddleCenter;
                }
            }

            // Container for content with some indentation and background
            var contentContainer = new VisualElement();
            contentContainer.style.paddingLeft = 12; // Standard indent
            contentContainer.style.paddingRight = 2;
            contentContainer.style.paddingTop = 2;
            contentContainer.style.paddingBottom = 4;
            
            // Optional: Add a subtle background to distinguish the ViewModel block
            contentContainer.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.03f));
            contentContainer.style.borderTopLeftRadius = 4;
            contentContainer.style.borderTopRightRadius = 4;
            contentContainer.style.borderBottomLeftRadius = 4;
            contentContainer.style.borderBottomRightRadius = 4;

            foldout.Add(contentContainer); // Add container to foldout content

            // 1. Add _data children (Flattened)
            SerializedProperty dataProp = property.FindPropertyRelative("_data");
            if (dataProp != null)
            {
                var childEnum = dataProp.Copy();
                var endProp = childEnum.GetEndProperty();

                if (childEnum.NextVisible(true)) // Enter _data
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(childEnum, endProp))
                            break;

                        var propField = new PropertyField(childEnum.Copy());
                        propField.Bind(property.serializedObject);
                        contentContainer.Add(propField);
                    }
                    while (childEnum.NextVisible(false));
                }
            }

            // 2. Add other properties of ViewModel
            var otherIter = property.Copy();
            var otherEnd = otherIter.GetEndProperty();

            if (otherIter.NextVisible(true)) // Enter ViewModel
            {
                do
                {
                    if (SerializedProperty.EqualContents(otherIter, otherEnd))
                        break;

                    if (otherIter.name == "_data") continue;

                    var propField = new PropertyField(otherIter.Copy());
                    propField.Bind(property.serializedObject);
                    contentContainer.Add(propField);
                }
                while (otherIter.NextVisible(false));
            }

            root.Add(foldout);
            return root;
        }
    }
}

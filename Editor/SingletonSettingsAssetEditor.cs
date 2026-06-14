using UnityEditor;
using UnityEngine.UIElements;

namespace Singletons.Editor
{
	[CustomEditor(typeof(SingletonProjectSettings))]
	public sealed class SingletonSettingsAssetEditor : UnityEditor.Editor
	{
		public override VisualElement CreateInspectorGUI()
		{
			return new Button(SingletonSettingsProvider.OpenProjectSettings) {
				text = "Edit Project Settings...",
				style = { height = 32 }
			};
		}
	}
}

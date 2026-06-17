using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Object = UnityEngine.Object;

namespace Singletons.Editor
{
	internal class TableRow : IMGUIContainer
	{
		const BindingFlags
			field_flags = BindingFlags.NonPublic
			| BindingFlags.Static
			| BindingFlags.FlattenHierarchy,
			method_flags = BindingFlags.NonPublic
			| BindingFlags.Static
			| BindingFlags.FlattenHierarchy;

		const float
			label_width = 0.4f,
			field_width = 0.4f,
			button_width = 0.1f;

		private readonly Type _type;
		private readonly Func<Object> _getMainInstance;
		private readonly Action<Object> _setMainInstance;

		private bool _hasTempValue;
		private Object _tempValue;

		public event Action ValueChanged;

		public Type Type => _type;
		public bool HasTempValue => _hasTempValue;

		public TableRow(Type type)
		{
			style.minHeight = EditorGUIUtility.singleLineHeight;
			style.marginBottom = EditorGUIUtility.standardVerticalSpacing;
			_type = type;
			var field = type.GetField("_main", field_flags);
			_getMainInstance = () => (Object)field.GetValue(null);

			var paramTypes = new Type[2] { type, typeof(bool) };
			var method = type.GetMethod("SetInstance", method_flags);
			_setMainInstance = obj => method.Invoke(null, new object[] { obj, true });

			onGUIHandler = OnGui;
		}

		private void OnGui()
		{
			var totalPosition = new Rect(Vector2.zero, worldBound.size);

			var labelPosition = new Rect(totalPosition) {
				width = totalPosition.width * label_width
			};
			EditorGUI.LabelField(labelPosition, _type.FullName);

			var mainInstance = _getMainInstance();
			Object currentInstance = HasTempValue ? _tempValue : mainInstance;
			var objFieldPosition = new Rect(totalPosition) {
				x = labelPosition.xMax,
				width = totalPosition.width * field_width
			};
			EditorGUI.BeginChangeCheck();
			Object newInstance = EditorGUI.ObjectField(objFieldPosition, string.Empty, currentInstance, _type, true);
			if (_hasTempValue = newInstance != mainInstance) {
				_tempValue = newInstance;
			}
			if (EditorGUI.EndChangeCheck()) {
				ValueChanged?.Invoke();
			}

			using (new EditorGUI.DisabledGroupScope(!_hasTempValue)) {
				var buttonPosition = new Rect(totalPosition) {
					x = objFieldPosition.xMax,
					width = totalPosition.width * button_width,
				};
				bool pressed = GUI.Button(buttonPosition, "Apply");
				if (pressed) {
					this.ApplyChanges();
				}

				buttonPosition.x = buttonPosition.xMax;
				pressed = GUI.Button(buttonPosition, "Revert");
				if (pressed) {
					RevertChanges();
				}
			}
		}

		public void ApplyChanges()
		{
			if (!HasTempValue)
				return;
			try {
				_setMainInstance(_tempValue);
				_hasTempValue = false;
				_tempValue = null;
			}
			finally {
				ValueChanged?.Invoke();
			}
		}

		public void RevertChanges()
		{
			_hasTempValue = false;
			_tempValue = null;
			ValueChanged?.Invoke();
		}
	}
}

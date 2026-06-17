using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using static Singletons.Editor.SingletonSettingsProvider.Styles;

namespace Singletons.Editor
{
	public class SingletonSettingsProvider : SettingsProvider
	{
		internal static class Styles
		{
			public static readonly Length content_padding = Length.Pixels(10f);
			public static readonly Length indent_size = Length.Pixels(15f);
			public static readonly Length vertical_spacing = Length.Pixels(12f);

			public static readonly Length title_font_size = Length.Pixels(22f);
			public static readonly FontStyle title_font_style = FontStyle.Bold;

			public static readonly Length header_font_size = Length.Pixels(16f);
			public static readonly FontStyle header_font_style = FontStyle.Bold;
			public static readonly Length header_padding_horizontal = Length.Pixels(6);
			public static readonly Length header_padding_vertical = Length.Pixels(2);
			public static readonly Color header_background_color = Color.HSVToRGB(0, 0, 0.18f);

			public static readonly Vector2 dropdown_minimum_size = new(200, 200);
		}

		public const string project_settings_path = "Project/Singletons";

		private const string
			text_add_type_button = "Add Type...",
			dropdown_menu_title = "Select Type",

			tooltip_persistent = "Singletons will persist between scenes.",
			tooltip_override_existing = "Newer instances will replace older ones.",
			tooltip_destroy_others = "Objects that are not set as main will be destroyed automatically.",
			tooltip_auto_generate = "If there is no main instance, a new instance will be created with the default values.",
			tooltip_prefab = "The prefab to use for auto-generation. (Optional)",

			field_name = "field__", label_name = "label__", container_name = "container__";

		private static readonly string
			asset_directory = Path.Combine("Assets", "Settings", "Resources"),
			asset_path = Path.Combine(asset_directory, SingletonProjectSettings.resource_path + ".asset");

		private readonly SingletonProjectSettings _singletonSettingsAsset;
		private readonly SerializedObject _serializedObject;

		public SingletonSettingsProvider() : base(project_settings_path, SettingsScope.Project)
		{
			_singletonSettingsAsset = AssetDatabase.LoadAssetAtPath<SingletonProjectSettings>(asset_path);

			if (_singletonSettingsAsset == null) {
				_singletonSettingsAsset = ScriptableObject.CreateInstance<SingletonProjectSettings>();
				if (!Directory.Exists(asset_directory))
					Directory.CreateDirectory(asset_directory);
				AssetDatabase.CreateAsset(_singletonSettingsAsset, path: asset_path);
				AssetDatabase.SaveAssets();
			}

			_singletonSettingsAsset.Clean();
			_serializedObject = new SerializedObject(_singletonSettingsAsset);
		}

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			keywords = GetSearchKeywordsFromSerializedObject(_serializedObject);
			rootElement.style.paddingLeft = content_padding;
			rootElement.style.paddingRight = content_padding;

			var content = new ScrollView(ScrollViewMode.Vertical) { dataSource = _singletonSettingsAsset };

			content.dataSource = _singletonSettingsAsset;

			var title = new Label(settingsPath.Split('/')[^1]) {
				style = {
					fontSize = title_font_size,
					unityFontStyleAndWeight = title_font_style,
					marginBottom = vertical_spacing,
				}
			};
			rootElement.Add(title);

			var destroyMode = _serializedObject.FindProperty(nameof(SingletonProjectSettings._destroyOthersDuring));
			var destroyModeField = new PropertyField(destroyMode) {
				style = {
					maxWidth = Length.Percent(50f),
					marginBottom = vertical_spacing
				}
			};
			rootElement.Add(destroyModeField);

			var addTypeButton = new Button() {
				name = "button__add-type",
				text = text_add_type_button,
				style = {
					width = Length.Pixels(200f),
				}
			};
			rootElement.Add(addTypeButton);

			var dummy = new IMGUIContainer() {
				name = container_name + "dummy",
				style = {
					flexShrink = 1,
				}
			};
			rootElement.Add(dummy);

			var defaultValues = _serializedObject.FindProperty(nameof(SingletonProjectSettings._defaultSettings));
			content.Add(CreateDefaultValuesField(defaultValues));

			var settings = _serializedObject.FindProperty(nameof(SingletonProjectSettings._singletonSettings));
			var path = PropertyPath.AppendName(default, settings.name);

			for (int i = 0; i < settings.arraySize; i++) {
				var element = settings.GetArrayElementAtIndex(i);
				var field = CreateSettingsField(element, defaultValues);
				field.dataSourcePath = PropertyPath.AppendIndex(path, i);
				content.Add(field);
			}

			rootElement.Add(content);
			rootElement.Bind(_serializedObject);

			addTypeButton.clicked += AddTypeButton_clicked;
			return;

			void AddTypeButton_clicked()
			{
				IEnumerable<Type> types = TypeCache.GetTypesDerivedFrom(typeof(Singleton<>))
					.Where(static type => typeof(Singleton<>).TryMakeGenericType(out _, type));
				var menu = new AdvancedDropdownMenu(dropdown_menu_title) {
					minimumSize = dropdown_minimum_size,
				};
				foreach (var type in types) {
					bool valid;
					try {
						Type singletonType = typeof(Singleton<>).MakeGenericType(type);
						valid = true;
					}
					catch (Exception) {
						valid = false;
					}

					var content = new GUIContent(text: type.FullName.Replace('.', '/'));
					bool enabled = valid && !_singletonSettingsAsset.HasSettings_Internal(type);
					menu.AddItem(content, enabled, AddTypeSettings, userData: type);
				}
				menu.Show(addTypeButton.worldBound, dummy);
			}

			void AddTypeSettings(object userData)
			{
				_singletonSettingsAsset.AddSettings_Internal(userData as Type);
				EditorUtility.SetDirty(_singletonSettingsAsset);
				_serializedObject.Update();
				content.Clear();
				//var defaultValues = _serializedObject.FindProperty("_defaultSettings");
				content.Add(CreateDefaultValuesField(defaultValues));
				//var settings = _serializedObject.FindProperty("_singletonSettings");
				for (int i = 0; i < settings.arraySize; i++) {
					var element = settings.GetArrayElementAtIndex(i);
					var field = CreateSettingsField(element, defaultValues);
					field.dataSourcePath = PropertyPath.AppendIndex(path, i);
					content.Add(field);
				}
				rootElement.Bind(_serializedObject);
			}
		}

		[SettingsProvider]
		public static SettingsProvider GetSettingsProvider() => new SingletonSettingsProvider();
		public static void OpenProjectSettings() => SettingsService.OpenProjectSettings(project_settings_path);

		private static Label CreateHeaderLabel(string name)
		{
			return new Label() {
				name = label_name + name,
				style = {
					fontSize = header_font_size,
					unityFontStyleAndWeight = header_font_style,
					paddingLeft = header_padding_horizontal,
					paddingRight = header_padding_horizontal,
					paddingTop = header_padding_vertical,
					paddingBottom = header_padding_vertical,
					backgroundColor = header_background_color,
				}
			};
		}

		private static VisualElement CreateFlagToggle(SerializedProperty property, string label, SingletonFlags flag)
		{
			var flagInt = (int)flag;
			var field = new Toggle() {
				name = field_name + label.ToLower().Replace(' ', '-'),
				label = label,
				value = (property.enumValueFlag & flagInt) == flagInt,
				tooltip = flag switch {
					SingletonFlags.Persistent => tooltip_persistent,
					SingletonFlags.OverrideExisting => tooltip_override_existing,
					SingletonFlags.DestroyOthers => tooltip_destroy_others,
					SingletonFlags.AutoGeneration => tooltip_auto_generate,
					_ => string.Empty,
				},
			};
			field.AddToClassList(Toggle.alignedFieldUssClassName);
			field.RegisterValueChangedCallback(evt => {
				if (evt.newValue)
					property.enumValueFlag |= (int)flag;
				else
					property.enumValueFlag &= ~(int)flag;
				property.serializedObject.ApplyModifiedProperties();
			});

			return field;
		}

		private static VisualElement CreateDefaultValuesField(SerializedProperty property)
		{
			const string defaultValuesName = "default-values";
			var container = new VisualElement() {
				name = container_name + defaultValuesName,
				style = { marginBottom = vertical_spacing }
			};

			var label = CreateHeaderLabel(defaultValuesName);
			label.text = property.displayName;
			container.Add(label);

			var flagsContainer = new VisualElement() {
				name = container_name + defaultValuesName,
				style = { marginLeft = indent_size }
			};
			container.Add(flagsContainer);

			// Skip the `None` value at index 0.
			for (int i = 1; i < property.enumDisplayNames.Length; i++) {
				var enumFlag = (SingletonFlags)(1 << (i - 1));
				var field = CreateFlagToggle(property, property.enumDisplayNames[i], enumFlag);
				flagsContainer.Add(field);
			}

			return container;
		}

		private static VisualElement CreateSettingsField(SerializedProperty property, SerializedProperty defaultFlagsProperty)
		{
			var container = new VisualElement() {
				name = container_name + "settings",
				style = { marginBottom = vertical_spacing }
			};

			{
				var label = CreateHeaderLabel(nameof(SingletonSettings.type));
				label.SetBinding(nameof(Label.text), new DataBinding() {
					dataSourcePath = PropertyPath.AppendName(default, nameof(SingletonSettings.typeName)),
					bindingMode = BindingMode.ToTarget,
				});
				container.Add(label);
			}

			var flagsProperty = property.FindPropertyRelative(nameof(SingletonSettings.flags));

			var flagsContainer = new VisualElement() {
				name = container_name + flagsProperty.name,
				style = {
					marginLeft = indent_size,
				}
			};
			container.Add(flagsContainer);

			var defaultsToggle = new Toggle() {
				name = field_name + "use-defaults",
				label = "Use Default Settings",
			};
			defaultsToggle.AddToClassList(Toggle.alignedFieldUssClassName);
			defaultsToggle.SetBinding(nameof(Toggle.value), new DataBinding() {
				dataSourcePath = PropertyPath.AppendName(default, nameof(SingletonSettings.UseDefaults)),
				bindingMode = BindingMode.TwoWay,
			});
			defaultsToggle.RegisterValueChangedCallback(evt => {
				if (evt.newValue) {
					flagsProperty.enumValueFlag = defaultFlagsProperty.enumValueFlag;
					property.serializedObject.ApplyModifiedProperties();
				}
			});
			flagsContainer.Add(defaultsToggle);

			flagsContainer.TrackPropertyValue(defaultFlagsProperty, _ => {
				if (defaultsToggle.value) {
					flagsProperty.enumValueFlag = defaultFlagsProperty.enumValueFlag;
					property.serializedObject.ApplyModifiedProperties();
				}
			});

			// Skip the `None` value at index 0.
			for (int i = 1; i < flagsProperty.enumDisplayNames.Length; i++) {
				string label = flagsProperty.enumDisplayNames[i];
				var enumFlag = (SingletonFlags)(1 << (i - 1));
				var field = CreateFlagToggle(flagsProperty, label, enumFlag);
				field.SetBinding(nameof(VisualElement.enabledSelf), new DataBinding() {
					dataSourcePath = PropertyPath.AppendName(default, nameof(SingletonSettings.overrideDefaults)),
					bindingMode = BindingMode.ToTarget,
				});
				field.SetBinding(nameof(Toggle.value), new DataBinding() {
					dataSourcePath = PropertyPath.AppendName(default, enumFlag.ToString()),
					// ToTarget instead of TwoWay because setting the value is handled elsewhere
					bindingMode = BindingMode.ToTarget,
				});

				var fieldContainer = new VisualElement() {
					name = container_name + field.name[field_name.Length..],
					style = { flexDirection = FlexDirection.Row }
				};
				fieldContainer.Add(field);

				if (enumFlag is SingletonFlags.AutoGeneration) {
					var prefabField = new ObjectField() {
						name = field_name + nameof(SingletonSettings.prefab),
						label = "Instantiate Prefab",
						tooltip = tooltip_prefab,
						objectType = typeof(GameObject),
						allowSceneObjects = false,
						bindingPath = property.propertyPath + "." + nameof(SingletonSettings.prefab),
						style = {
							flexGrow = 0.5f, flexShrink = 1,
						}
					};
					prefabField.SetBinding(nameof(VisualElement.visible), new DataBinding() {
						dataSourcePath = PropertyPath.AppendName(default, nameof(SingletonFlags.AutoGeneration)),
						bindingMode = BindingMode.ToTarget,
					});
					fieldContainer.Add(prefabField);
				}

				flagsContainer.Add(fieldContainer);
			}

			return container;
		}
	}
}

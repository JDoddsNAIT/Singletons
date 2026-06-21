using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Singletons.Editor
{
	public class SingletonDebugWindow : EditorWindow
	{
		const string content_container_name = nameof(SingletonDebugWindow) + "_container__content";

		private static readonly Type genericSingleton = typeof(Singleton<>);
		private static readonly ISearchQueryHandler<Type> defaultHandler = new ContainsHandler();

		private ISearchQueryHandler<Type> _queryHandler;
		public event Action OnSaveChanges;
		public event Action OnDiscardChanges;

		[MenuItem("Window/Analysis/Singleton Debugger")]
		public static void ShowWindow()
		{
			ShowWindow(defaultHandler);
		}

		internal static void ShowWindow(ISearchQueryHandler<Type> queryHandler)
		{
			SingletonDebugWindow wnd = GetWindow<SingletonDebugWindow>();
			wnd.titleContent = new GUIContent("Singleton Debugger");
			wnd._queryHandler = queryHandler;
		}

		public void CreateGUI()
		{
			VisualElement root = rootVisualElement;

			var toolbar = new Toolbar();
			root.Add(toolbar);

			var editMenu = new ToolbarMenu() {
				text = "Edit",
			};
			editMenu.menu.AppendAction("Apply All", a => SaveChanges());
			editMenu.menu.AppendAction("Revert All", a => DiscardChanges());
			toolbar.Add(editMenu);

			var spacer = new ToolbarSpacer() {
				style = {
					flexGrow  = 1
				}
			};
			toolbar.Add(spacer);

			var searchBar = new ToolbarSearchField() {
				style = {
					alignSelf = Align.FlexEnd,
				}
			};
			searchBar.RegisterValueChangedCallback(evt => FilterByName(evt.newValue));
			toolbar.Add(searchBar);

			var content = new ScrollView(ScrollViewMode.Vertical) {
				name = content_container_name,
			};
			root.Add(content);

			var types = TypeCache.GetTypesDerivedFrom(genericSingleton)
				.Where(static t => genericSingleton.TryMakeGenericType(out _, t))
				.OrderBy(static t => t.FullName);
			foreach (var type in types) {
				var element = new TableRow(type) {
					style = {
						marginLeft = Length.Pixels(6f),
						marginRight = Length.Pixels(6f),
					}
				};
				OnSaveChanges += element.ApplyChanges;
				OnDiscardChanges += element.RevertChanges;
				element.ValueChanged += CheckForChanges;
				content.Add(element);
			}
			CheckForChanges();
		}

		private void CheckForChanges()
		{
			hasUnsavedChanges = false;
			foreach (var row in GetRows()) {
				hasUnsavedChanges |= row.HasTempValue;
			}
		}

		private void FilterByName(string query)
		{
			Predicate<Type> predicate = null;
			if (!string.IsNullOrEmpty(query)) {
				predicate = _queryHandler?.GetMatchDelegate(query);
			}
			foreach (var row in GetRows()) {
				bool isMatch = predicate?.Invoke(row.Type) ?? true;
				row.style.display = isMatch ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}

		private IEnumerable<TableRow> GetRows()
		{
			var content = rootVisualElement.Q<ScrollView>(name: content_container_name);
			for (int i = 0; i < content.childCount && !hasUnsavedChanges; i++) {
				var element = (TableRow)content[i];
				yield return element;
			}
		}

		public override void SaveChanges()
		{
			base.SaveChanges();
			OnSaveChanges?.Invoke();
		}

		public override void DiscardChanges()
		{
			base.DiscardChanges();
			OnDiscardChanges?.Invoke();
		}
	}

	internal static partial class Utilities
	{
		public static bool TryMakeGenericType(this Type type, out Type result, params Type[] typeArguments)
		{
			try {
				result = type.MakeGenericType(typeArguments);
				return true;
			}
			catch (ArgumentException) {
				result = null;
				return false;
			}
		}

		public static IEnumerable<Type> EnumerateHierarchy(this Type type)
		{
			for (Type node = type; node != null; node = node.BaseType) {
				yield return node;
			}
		}
	}
}

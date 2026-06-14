using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Singletons.Editor
{
	/// <summary>
	/// Represents an advanced dropdown menu, similar to the 'Add Component' menu in the Unity Editor.
	/// </summary>
	public sealed class AdvancedDropdownMenu : AdvancedDropdown
	{
		private const string root = "";

		private readonly Dictionary<string, AdvancedDropdownItem> _tree;

		public new Vector2 minimumSize { get => base.minimumSize; set => base.minimumSize = value; }

		private AdvancedDropdownItem Root => _tree[root];

		public AdvancedDropdownMenu() : this(string.Empty) { }
		public AdvancedDropdownMenu(string title) : base(state: new())
		{
			_tree = DictionaryPool<string, AdvancedDropdownItem>.Get();
			_tree.Add(root, new AdvancedDropdownItem(title));
		}
		public AdvancedDropdownMenu(string title, Vector2 minimumSize) : this(title)
		{
			base.minimumSize = minimumSize;
		}

		~AdvancedDropdownMenu()
		{
			DictionaryPool<string, AdvancedDropdownItem>.Release(_tree);
		}

		/// <summary>
		/// Adds an option to the menu.
		/// </summary>
		/// <param name="content">The menu item's name and icon.</param>
		/// <param name="enabled">Is the item selectable?</param>
		/// <param name="func">The delegate invoke when this item is selected.</param>
		public void AddItem(GUIContent content, bool enabled, GenericMenu.MenuFunction func)
			=> AddItem(content, enabled, _ => func?.Invoke(), null);
		/// <summary>
		/// Adds an option to the menu.
		/// </summary>
		/// <param name="content">The menu item's name and icon.</param>
		/// <param name="enabled">Is the item selectable?</param>
		/// <param name="func">The delegate invoke when this item is selected.</param>
		/// <param name="userData">The argument passed into <paramref name="func"/>.</param>
		public void AddItem(GUIContent content, bool enabled, GenericMenu.MenuFunction2 func, object userData)
		{
			if (content is null)
				throw new System.ArgumentNullException(nameof(content));
			var parts = content.text.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
			if (parts?.Length is null or <= 0)
				return;

			string nodePath;
			var path = new System.Text.StringBuilder();
			AdvancedDropdownItem parent = Root;

			int i;
			for (i = 0; i < parts.Length - 1; i++) {
				if (i > 0)
					path.Append('/');
				path.Append(parts[i]);

				nodePath = path.ToString();
				if (!_tree.TryGetValue(nodePath, out var current)) {
					current = new AdvancedDropdownItem(parts[i]);
					parent.AddChild(current);
					_tree.Add(nodePath, current);
				}
				parent = current;
			}

			nodePath = string.Join('/', parts);
			string name = parts[^1];
			_tree.TryAdd(nodePath, null);
			_tree[nodePath] = new MenuItem(name, func, userData) {
				enabled = enabled,
				icon = content.image as Texture2D,
			};
			parent.AddChild(_tree[nodePath]);
		}
		/// <summary>
		/// Adds a separator to the menu.
		/// </summary>
		/// <param name="path">The path of the menu item to add the separator to.</param>
		public void AddSeparator(string path = root)
		{
			if (_tree.ContainsKey(path)) {
				_tree[path].AddSeparator();
			}
		}

		protected override AdvancedDropdownItem BuildRoot() => Root;

		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			if (item.enabled && item is MenuItem menuItem) {
				menuItem.Func?.Invoke(menuItem.UserData);
			}
		}

		/// <summary>
		/// <see cref="AdvancedDropdown"/> can only be shown during an OnGUI call. Use this method to queue showing the menu until <paramref name="host"/>'s next OnGUI call.
		/// </summary>
		/// <param name="rect">Screen rect to show the menu.</param>
		/// <param name="host">The host container.</param>
		public void Show(Rect rect, IMGUIContainer host)
		{
			Vector2 relativePosition = rect.position - host.worldBound.position;
			rect = new Rect(relativePosition, rect.size);
			host.onGUIHandler += OnGUIHandler;

			void OnGUIHandler()
			{
				host.onGUIHandler -= OnGUIHandler;
				this.Show(rect);
			}
		}

		private sealed class MenuItem : AdvancedDropdownItem
		{
			public readonly object UserData;
			public readonly GenericMenu.MenuFunction2 Func;

			public MenuItem(string name, GenericMenu.MenuFunction2 func, object userData) : base(name)
			{
				UserData = userData;
				Func = func;
			}
		}
	}
}

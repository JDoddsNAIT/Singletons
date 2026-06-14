using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singletons
{
	internal class SingletonProjectSettings : ScriptableObject
	{
		public const string resource_path = nameof(SingletonSettings);

		[NonSerialized] internal Dictionary<string, int> _indexMap = null;

		[SerializeField] internal SingletonFlags _defaultSettings;
		[SerializeField] internal List<SingletonSettings> _singletonSettings = new();

		internal static readonly Queue<Type> typesToAdd = new();
		internal static SingletonProjectSettings _main;

		public void OnEnable()
		{
			_main = this;
			while (typesToAdd.TryDequeue(out var type)) {
				AddSettings_Internal(type);
			}
		}

		public void Clean() => _singletonSettings.RemoveAll(static x => Type.GetType(x.type) == null);

		public static void AddSettingsForType(Type type)
		{
			if (_main == null) {
				typesToAdd.Enqueue(type);
			} else {
				_main.AddSettings_Internal(type);
			}
		}

		public static SingletonSettings GetSettingsForType(Type type)
		{
			if (_main == null)
				_main = GetOrCreate();
			return _main.GetSettings_Internal(type);
		}

		public static bool HasSettingsForType(Type type)
		{
			if (_main == null)
				_main = GetOrCreate();
			return _main.HasSettings_Internal(type);
		}

		internal void AddSettings_Internal(Type type)
		{
			var settings = new SingletonSettings(type) { flags = _defaultSettings };
			if (!IndexMap.ContainsKey(settings.type)) {
				_indexMap.Add(settings.type, _singletonSettings.Count);
				_singletonSettings.Add(settings);
			}
		}

		internal SingletonSettings GetSettings_Internal(Type type)
		{
			SingletonSettings result;
			string typeName = type.AssemblyQualifiedName;
			if (!IndexMap.TryGetValue(typeName, out var index)) {
				result = new SingletonSettings(type) { flags = _defaultSettings };
			} else if ((result = _singletonSettings[index]).UseDefaults) {
				result.flags = _defaultSettings;
			}
			return result;
		}

		internal bool HasSettings_Internal(Type type)
		{
			string typeName = type.AssemblyQualifiedName;
			return _singletonSettings.Any(s => s.type == type.AssemblyQualifiedName);
		}

		public void RemoveSettingsForType(Type type)
		{
			string typeName = type.AssemblyQualifiedName;
			if (IndexMap.TryGetValue(typeName, out var index)) {
				_singletonSettings.RemoveAt(index);
				_indexMap.Remove(typeName);
			}
		}

		internal Dictionary<string, int> IndexMap {
			get {
				if (_indexMap is null) {
					_indexMap = new Dictionary<string, int>(capacity: _singletonSettings.Count);
					for (int i = 0; i < _singletonSettings.Count; i++) {
						_indexMap.Add(_singletonSettings[i].type, i);
					}
				}
				return _indexMap;
			}
		}

		public static SingletonProjectSettings GetOrCreate()
		{
			var result = Resources.Load<SingletonProjectSettings>(resource_path);
			if (result == null) {
				result = CreateInstance<SingletonProjectSettings>();
			}
			return result;
		}
	}
}

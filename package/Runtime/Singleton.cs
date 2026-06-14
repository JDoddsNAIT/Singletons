using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Singletons
{
	/// <summary>
	/// Holds a static instance of <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		private const string
			auto_generate_message = "Created supplementary instance of singleton {0}.",
			no_instance_found_message = "No main instance of {0} exists.",
			instance_overridden_message = "The main instance {0} has been overridden.",
			instance_destroyed_message = "The instance {0} has been destroyed.",
			invalid_instance_message = "No {0} component was found on the generated object. Did you forget to add the component to the root of the prefab?",

			instance_name_format = "{0} (Auto-Generated)";

		static Singleton() => SingletonProjectSettings.AddSettingsForType(type);

		[NonSerialized] protected static T _main;

		private static readonly Type type = typeof(T);
		private static SingletonSettings? _settings = null;
		private static SingletonSettings Settings => _settings ??= SingletonProjectSettings.GetSettingsForType(type);

		/// <summary>Checks if a main instance exists.</summary>
		public static bool Exists => _main != null;
		/// <summary>Compares the main instance with <paramref name="value"/> to see if they are equal.</summary>
		/// <param name="value">The value to compare. Can be null.</param>
		/// <returns></returns>
		public static bool CompareInstance([AllowNull] T value) => _main == value;

		/// <summary>
		/// Gets the main instance of <typeparamref name="T"/>.
		/// </summary>
		/// <returns>The main instance. Does not return null.</returns>
		[return: NotNull]
		public static T GetInstance()
		{
			if (_main == null)
				_main = FindAnyObjectByType<T>();
			if (_main == null)
				_main = GenerateInstance();
			return _main;
		}

		protected static T GenerateInstance()
		{
			if (!Settings.HasFlag(SingletonFlags.AutoGeneration)) {
				throw new NullReferenceException(string.Format(no_instance_found_message, type));
			}
			GameObject obj;
			if (Settings.prefab != null) {
				obj = Instantiate(Settings.prefab);
			} else {
				string name = string.Format(instance_name_format, type);
				obj = new GameObject(name, type);
			}

			if (!obj.TryGetComponent(out T instance))
				throw new InvalidOperationException(string.Format(invalid_instance_message, type));

			Debug.LogFormat(instance, auto_generate_message, type);
			return instance;
		}

		/// <summary>
		/// Attempts to set <paramref name="value"/> as the main instance.
		/// </summary>
		/// <param name="value"></param>
		public static void SetInstance([AllowNull] T value)
		{
			if (_main == value)
				return;

			if (_main != null) {
				if (Settings.HasFlag(SingletonFlags.OverrideExisting)) {
					Debug.LogFormat(context: value, instance_overridden_message, _main);
					DestroyInstance(_main);
				} else {
					DestroyInstance(value);
					return;
				}
			}
			_main = value;
			if (value == null)
				return;

			if (Settings.HasFlag(SingletonFlags.Persistent)) {
				value.transform.SetParent(null);
				DontDestroyOnLoad(value.gameObject);
			}
			_main.Initialize();
		}

		protected static void DestroyInstance(T instance)
		{
			if (!Settings.HasFlag(SingletonFlags.DestroyOthers))
				return;

			Debug.LogFormat(instance_destroyed_message, instance);
			if (Application.isPlaying)
				Destroy(instance);
			else
				DestroyImmediate(instance);
		}

		/// <summary>
		/// Attempts to set this instance as <see cref="Main"/>.
		/// </summary>
		protected virtual void Awake() => SetInstance((T)this);

		protected virtual void OnDestroy()
		{
			if (_main == (T)this) {
				_main = null;
			}
		}

		/// <summary> 
		/// Override this method to create custom logic for when this object is set as the main instance.
		/// </summary>
		protected virtual void Initialize() { }

		public static explicit operator T(Singleton<T> instance)
		{
			if (instance is T component || instance.TryGetComponent(out component))
				return component;
			else
				throw new InvalidCastException();
		}
	}
}

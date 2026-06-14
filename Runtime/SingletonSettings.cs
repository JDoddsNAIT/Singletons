using System;
using Unity.Properties;
using UnityEngine;

namespace Singletons
{
	[Serializable]
	internal struct SingletonSettings : IEquatable<SingletonSettings>
	{
		public string type;
		[CreateProperty(ReadOnly = true)]
		public string typeName;
		public bool overrideDefaults;
		public SingletonFlags flags;
		public GameObject prefab;

		[CreateProperty]
		public bool UseDefaults { readonly get => !overrideDefaults; set => overrideDefaults = !value; }

		/// <inheritdoc cref="SingletonFlags.Persistent"/>
		[CreateProperty]
		public bool Persistent {
			readonly get => flags.HasFlag(SingletonFlags.Persistent);
			set => SetFlag(ref flags, SingletonFlags.Persistent, value);
		}
		/// <inheritdoc cref="SingletonFlags.OverrideExisting"/>
		[CreateProperty]
		public bool OverrideExisting {
			readonly get => flags.HasFlag(SingletonFlags.OverrideExisting);
			set => SetFlag(ref flags, SingletonFlags.OverrideExisting, value);
		}
		/// <inheritdoc cref="SingletonFlags.DestroyOthers"/>
		[CreateProperty]
		public bool DestroyOthers {
			readonly get => flags.HasFlag(SingletonFlags.DestroyOthers);
			set => SetFlag(ref flags, SingletonFlags.DestroyOthers, value);
		}
		/// <inheritdoc cref="SingletonFlags.AutoGeneration"/>
		[CreateProperty]
		public bool AutoGeneration {
			readonly get => flags.HasFlag(SingletonFlags.AutoGeneration);
			set => SetFlag(ref flags, SingletonFlags.AutoGeneration, value);
		}

		public SingletonSettings(Type type) : this()
		{
			this.type = type.AssemblyQualifiedName;
			this.typeName = type.FullName;
		}

		public readonly bool HasFlag(SingletonFlags flag) => flags.HasFlag(flag);

		private static void SetFlag(ref SingletonFlags flags, SingletonFlags flag, bool value)
		{
			if (value) {
				flags |= flag;
			} else {
				flags &= ~flag;
			}
		}

		public override readonly int GetHashCode() => type.GetHashCode();
		public override readonly bool Equals(object obj) => obj is SingletonSettings other && Equals(other);

		public readonly bool Equals(SingletonSettings other) => type == other.type;
	}

	[Flags]
	internal enum SingletonFlags : byte
	{
		None = default,
		/// <summary>
		/// Singletons will persist between scenes.
		/// </summary>
		Persistent = 1 << 0,
		/// <summary>
		/// Newer instances will replace older ones.
		/// </summary>
		OverrideExisting = 1 << 1,
		/// <summary>
		/// Objects that are not set as main will be destroyed automatically.
		/// </summary>
		DestroyOthers = 1 << 2,
		/// <summary>
		/// If there is no main instance, a new instance will be created with the default values.
		/// </summary>
		AutoGeneration = 1 << 3,
	}
}

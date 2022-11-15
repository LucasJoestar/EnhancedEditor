// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedEditor {
	/// <summary>
	/// Multiple constraints used for <see cref="SerializedType{T}"/> assignement.
	/// <br/> Selected constraints are used to authorize specific type assignements.
	/// </summary>
    [Flags]
	public enum SerializedTypeConstraint {
		None = 0,
		BaseType,
		Abstract,
		Null
    }

	/// <summary>
	/// <see cref="System.Type"/> wrapper, used to serialize a type derived from another specific type.
	/// </summary>
	/// <typeparam name="T">Base type this type value should be derived from.</typeparam>
	[Serializable]
	public class SerializedType<T> : ISerializationCallbackReceiver, IComparer<SerializedType<T>> where T : class {
		#region Global Members
		[SerializeField] protected string typeName = string.Empty;

        [SerializeField] protected SerializedTypeConstraint constraints = SerializedTypeConstraint.None;

		private Type type = null;

		/// <summary>
		/// The serialized type reference value.
		/// </summary>
		public Type Type {
			get {
				return type;
			} set {
				SetType(value);
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a new type serialization wrapper.
		/// </summary>
		/// <param name="_constraints">Constraints for this type assignement.</param>
		public SerializedType(SerializedTypeConstraint _constraints = SerializedTypeConstraint.None) : base() {
			constraints = _constraints;
		}

		/// <param name="_type">Default base value of this type.</param>
		/// <inheritdoc cref="SerializedType{T}.SerializedType(SerializedTypeConstraint)"/>
		public SerializedType(Type _type, SerializedTypeConstraint _constraints = SerializedTypeConstraint.None) : this(_constraints) {
			Type = _type;
		}

		/// <summary>
		/// Creates a new type serialization wrapper.
		/// <br/> Initializes it with the first not ignored derived type.
		/// </summary>
		/// <param name="_defaultType">The default value of this type, if no other could be found.</param>
		/// <param name="_ignored">Derived types to ignore when seeking for the default value.</param>
		/// <inheritdoc cref="SerializedType{T}.SerializedType(SerializedTypeConstraint)"/>
		public SerializedType(SerializedTypeConstraint _constraints, Type _defaultType, params Type[] _ignored) : this(_constraints) {
			#if UNITY_EDITOR
			var _types = TypeCache.GetTypesDerivedFrom(typeof(T));

			foreach (var _type in _types) {
				if ((!_type.IsAbstract || _constraints.HasFlag(SerializedTypeConstraint.Abstract)) && !_type.IsDefined(typeof(EtherealAttribute), false)
				 && (_type != _defaultType) && !ArrayUtility.Contains(_ignored, _type)) {
					Type = _type;
					return;
				}
			}
			#endif

			Type = _defaultType;
		}
		#endregion

		#region Operators
		public static implicit operator Type(SerializedType<T> _type) {
			return _type.type;
		}

		public static implicit operator string(SerializedType<T> _type) {
			return _type.typeName;
		}

		public override string ToString() {
			return (type != null) ? type.FullName : "[Null Type]";
		}

        public override bool Equals(object _object) {
			if (_object is SerializedType<T> _type) {
				return Type.Equals(_type.Type);
            }

            return base.Equals(_object);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
        #endregion

        #region Comparer
        int IComparer<SerializedType<T>>.Compare(SerializedType<T> a, SerializedType<T> b) {
			return a.Type.Name.CompareTo(b.Type.Name);
		}
		#endregion

		#region Utility
		internal void SetType(Type _type) {
			Type _base = typeof(T);

			// Null value.
			if (_type == null) {
				if (!constraints.HasFlag(SerializedTypeConstraint.Null)) {
					Debug.LogError("SerializedType - Cannot assign a null value to this type");
					return;
				}

				type = null;
				typeName = string.Empty;
			}

			// Base type & subclass.
			if (_type == _base){
				if (!constraints.HasFlag(SerializedTypeConstraint.BaseType)) {
					Debug.LogError($"SerializedType - This type value cannot be the same as the base type \'{_base.Name}\'");
					return;
				}
			} else if (!_base.IsAssignableFrom(_type)) {
				Debug.LogError($"SerializedType - The type \'{_type.Name}\' does not inherit from \'{_base.Name}\'");
				return;
			}

			// Abstract.
			if (_type.IsAbstract && !constraints.HasFlag(SerializedTypeConstraint.Abstract)) {
				Debug.LogError($"SerializedType - The type \'{_type.Name}\' is abstract and cannot be assigned");
				return;
			}

			// Valid type.
			type = _type;
			typeName = _type.GetReflectionName();
		}
        #endregion

        #region Serialization Callbacks
        void ISerializationCallbackReceiver.OnAfterDeserialize() {
			// Deserialize the type value based on the serialized type name.
			if (!string.IsNullOrEmpty(typeName)) {
				type = Type.GetType(typeName);

				if (type == null) {
					Debug.LogWarning($"The type \'{typeName}\' could not be found to deserialize a type value!");
				}
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize() { }
		#endregion
	}
}

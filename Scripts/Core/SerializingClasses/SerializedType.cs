// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor {
	/// <summary>
	/// <see cref="System.Type"/> wrapper, used to serialize a type derived from another specific type.
	/// </summary>
	/// <typeparam name="T">Base type this type value should be derived from.</typeparam>
	[Serializable]
	public class SerializedType<T> : ISerializationCallbackReceiver where T : class {
		#region Global Members
		[SerializeField] protected string typeName = string.Empty;
		[SerializeField] private bool canBeBaseType = true;

		private Type type = null;

		/// <summary>
		/// The serialized type reference value.
		/// </summary>
		public Type Type {
			get {
				return type;
			} set {
				Type _base = typeof(T);

				if (value == null) {
					type = null;
					typeName = string.Empty;
				} else if ((canBeBaseType && (value == _base)) || value.IsSubclassOf(_base)) {
					type = value;
					typeName = value.GetReflectionName();
                } 
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a new type serialization wrapper.
		/// </summary>
		/// <param name="_canBeBaseType">Whether this type value can be the same
		/// as the base type specified as this class generic argument or not.</param>
		public SerializedType(bool _canBeBaseType = true) : base() {
			canBeBaseType = _canBeBaseType;
		}

		/// <param name="_value">Default base value of this type.</param>
		/// <inheritdoc cref="SerializedType{T}.SerializedType(bool)"/>
		public SerializedType(Type _value, bool _canBeBaseType = true) : this(_canBeBaseType) {
			Type = _value;
		}
        #endregion

        #region Operators
        public static implicit operator Type(SerializedType<T> _type) {
			return _type.type;
		}

		public static implicit operator string(SerializedType<T> _type) {
			return _type.typeName;
		}

		public static implicit operator SerializedType<T>(Type _type) {
			return typeof(SerializedType<>).MakeGenericType(_type);
		}

		public override string ToString() {
			return (type != null) ? type.FullName : "[Null Type]";
		}
		#endregion

		#region Serialization Callbacks
		void ISerializationCallbackReceiver.OnAfterDeserialize() {
			// Deserialize the type value based on the serialized type name.
			if (!string.IsNullOrEmpty(typeName)) {
				type = Type.GetType(typeName);

				if (type == null) {
					Debug.LogWarning($"The type '{typeName}' could not be found to deserialize a type value!");
				}
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize() { }
		#endregion
	}
}

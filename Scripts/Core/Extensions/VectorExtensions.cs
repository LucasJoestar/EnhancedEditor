// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace EnhancedEditor {
	/// <summary>
	/// Multiple extension methods related to the <see cref="Vector2"/> and <see cref="Vector3"/> classes.
	/// </summary>
	public static class VectorExtensions {
        #region Random
        /// <summary>
        /// Get a random value between this vector X & Y components.
        /// </summary>
        /// <param name="_value">Vector to get random value from.</param>
        /// <returns>Random value between this vector X & Y components.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Random(this Vector2 _value) {
			return UnityEngine.Random.Range(_value.x, _value.y);
		}
        #endregion

        #region Null Check
        /// <inheritdoc cref="IsNull(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this Vector2 _vector) {
			return (_vector.x == 0f) && (_vector.y == 0f);
		}

        /// <summary>
        /// Get if a specific vector is null, that is if all its component values are equal to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this Vector3 _vector) {
			return (_vector.x == 0f) && (_vector.y == 0f) && (_vector.z == 0f);
		}

        /// <inheritdoc cref="IsNull(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this Vector2Int _vector) {
            return (_vector.x == 0) && (_vector.y == 0);
        }

        /// <inheritdoc cref="IsNull(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this Vector3Int _vector) {
            return (_vector.x == 0) && (_vector.y == 0) && (_vector.z == 0);
        }
        #endregion

        #region Geometry
        /// <summary>
        /// Subtracts any part of a direction vector parallel to a normal vector,
        /// leaving only the perpendicular component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 PerpendicularSurface(this Vector3 _direction, Vector3 _normal) {
			return _direction - (_normal * Vector3.Dot(_direction, _normal));
		}

        /// <inheritdoc cref="PerpendicularSurface(Vector3, Vector3)"/>
        public static Vector3 PerpendicularSurface(this Vector3 _direction, Vector3 _normal, bool _slide) {
            Vector3 _newDirection = PerpendicularSurface(_direction, _normal);

            if (!_slide) {

                Vector3 _flatDirection = _direction.Flat();
                if (_flatDirection != Vector3.zero) {

                    Quaternion rotation = Quaternion.LookRotation(_direction.Flat().normalized);

                    // Prevent from sliding on undesirable directions.
                    _newDirection   = _newDirection.RotateInverse(rotation);
                    _newDirection.x = 0f;

                    _newDirection = _newDirection.Rotate(rotation);
                }
            }

            return _newDirection;
        }

        /// <summary>
        /// Get this vector flat value, that is without its vertical (Y axis) component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Flat(this Vector3 _vector) {
			return new Vector3(_vector.x, 0f, _vector.z);
        }

        // -------------------------------------------
        // Set
        // -------------------------------------------

        /// <summary>
        /// Sets this vector horizontal (X axis) component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetX(this Vector3 _vector, float _value) {
			return new Vector3(_value, _vector.y, _vector.z);
        }

        /// <summary>
        /// Sets this vector vertical (Y axis) component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetY(this Vector3 _vector, float _value) {
			return new Vector3(_vector.x, _value, _vector.z);
		}

        /// <summary>
        /// Sets this vector forward (Z axis) component.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetZ(this Vector3 _vector, float _value) {
			return new Vector3(_vector.x, _vector.y, _value);
		}

        // ===== Vector2 ===== \\

        /// <inheritdoc cref="SetX(Vector3, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SetX(this Vector2 _vector, float _value)
        {
            return new Vector2(_value, _vector.y);
        }

        /// <inheritdoc cref="SetY(Vector3, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SetY(this Vector2 _vector, float _value)
        {
            return new Vector2(_vector.x, _value);
        }

        // ===== Vector3Int ===== \\

        /// <inheritdoc cref="SetX(Vector3, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int SetX(this Vector3Int _vector, int _value) {
            return new Vector3Int(_value, _vector.y, _vector.z);
        }

        /// <inheritdoc cref="SetY(Vector3, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int SetY(this Vector3Int _vector, int _value) {
            return new Vector3Int(_vector.x, _value, _vector.z);
        }

        /// <inheritdoc cref="SetY(Vector3, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int SetZ(this Vector3Int _vector, int _value) {
            return new Vector3Int(_vector.x, _vector.y, _value);
        }

        // ===== Vector2Int ===== \\

        /// <inheritdoc cref="SetX(Vector3, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int SetX(this Vector2Int _vector, int _value) {
            return new Vector2Int(_value, _vector.y);
        }

        /// <inheritdoc cref="SetY(Vector3, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int SetY(this Vector2Int _vector, int _value) {
            return new Vector2Int(_vector.x, _value);
        }
        #endregion

        #region Maths
        // -------------------------------------------
        // Sum
        // -------------------------------------------

        /// <summary>
        /// Get the sum of all this vector components.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this Vector3 _vector) {
			return _vector.x + _vector.y + _vector.z;
		}

        /// <inheritdoc cref="Sum(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum(this Vector2 _vector) {
			return _vector.x + _vector.y;
		}

        /// <inheritdoc cref="Sum(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum(this Vector3Int _vector) {
            return _vector.x + _vector.y + _vector.z;
        }

        /// <inheritdoc cref="Sum(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum(this Vector2Int _vector) {
            return _vector.x + _vector.y;
        }

        // -------------------------------------------
        // Abs Sum
        // -------------------------------------------

        /// <summary>
        /// Get the absolute sum of all this vector components.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsSum(this Vector3 _vector) {
			return Mathf.Abs(_vector.x) + Mathf.Abs(_vector.y) + Mathf.Abs(_vector.z);
		}

        /// <inheritdoc cref="AbsSum(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsSum(this Vector2 _vector) {
			return Mathf.Abs(_vector.x) + Mathf.Abs(_vector.y);
		}

        /// <inheritdoc cref="AbsSum(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AbsSum(this Vector3Int _vector) {
            return Mathf.Abs(_vector.x) + Mathf.Abs(_vector.y) + Mathf.Abs(_vector.z);
        }

        /// <inheritdoc cref="AbsSum(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AbsSum(this Vector2Int _vector) {
            return Mathf.Abs(_vector.x) + Mathf.Abs(_vector.y);
        }

        // -------------------------------------------
        // Max
        // -------------------------------------------

        /// <summary>
        /// Get the maximum component of this vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(this Vector3 _vector) {
            return Mathf.Max(Mathf.Max(_vector.x, _vector.y), _vector.z);
        }

        /// <inheritdoc cref="Max(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(this Vector2 _vector) {
            return Mathf.Max(_vector.x, _vector.y);
        }

        /// <inheritdoc cref="Max(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(this Vector3Int _vector) {
            return Mathf.Max(Mathf.Max(_vector.x, _vector.y), _vector.z);
        }

        /// <inheritdoc cref="AbsSum(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(this Vector2Int _vector) {
            return Mathf.Max(_vector.x, _vector.y);
        }

        // -------------------------------------------
        // Min
        // -------------------------------------------

        /// <summary>
        /// Get the minimum component of this vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(this Vector3 _vector) {
            return Mathf.Min(Mathf.Min(_vector.x, _vector.y), _vector.z);
        }

        /// <inheritdoc cref="Min(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(this Vector2 _vector) {
            return Mathf.Min(_vector.x, _vector.y);
        }

        /// <inheritdoc cref="Min(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(this Vector3Int _vector) {
            return Mathf.Min(Mathf.Min(_vector.x, _vector.y), _vector.z);
        }

        /// <inheritdoc cref="Min(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(this Vector2Int _vector) {
            return Mathf.Min(_vector.x, _vector.y);
        }

        // -------------------------------------------
        // Divide
        // -------------------------------------------

        /// <summary>
        /// Divides this vector by another vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(this Vector3 _vector, Vector3 _other) {
			return new Vector3(_vector.x / _other.x, _vector.y / _other.y, _vector.z / _other.z);
		}

        /// <inheritdoc cref="Divide(Vector3, Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Divide(this Vector2 _vector, Vector2 _other) {
            return new Vector2(_vector.x / _other.x, _vector.y / _other.y);
        }

        // -------------------------------------------
        // Abs
        // -------------------------------------------

        /// <summary>
        /// Get this vector with all its components as their absolute value.
        /// </summary>
        /// <param name="_vector">The vector to convert.</param>
        /// <returns>This vector with absolute values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Abs(this Vector3 _vector) {
            return new Vector3(Mathf.Abs(_vector.x), Mathf.Abs(_vector.y), Mathf.Abs(_vector.z));
        }

        /// <inheritdoc cref="Abs(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Abs(this Vector2 _vector) {
            return new Vector2(Mathf.Abs(_vector.x), Mathf.Abs(_vector.y));
        }

        /// <inheritdoc cref="Abs(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int Abs(this Vector3Int _vector) {
            return new Vector3Int(Mathf.Abs(_vector.x), Mathf.Abs(_vector.y), Mathf.Abs(_vector.z));
        }

        /// <inheritdoc cref="Abs(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int Abs(this Vector2Int _vector) {
            return new Vector2Int(Mathf.Abs(_vector.x), Mathf.Abs(_vector.y));
        }

        // -------------------------------------------
        // NaN
        // -------------------------------------------

        /// <summary>
        /// Checks if any component of this vector is not a number (see <see cref="float.NaN"/>).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyNaN(this Vector3 vector) {
            return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);
        }

        /// <inheritdoc cref="IsAnyNaN(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyNaN(this Vector2 vector) {
            return float.IsNaN(vector.x) || float.IsNaN(vector.y);
        }

        /// <summary>
        /// Checks if all components of this vector are not a number (see <see cref="float.NaN"/>).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAllNaN(this Vector3 vector) {
            return float.IsNaN(vector.x) && float.IsNaN(vector.y) && float.IsNaN(vector.z);
        }

        /// <inheritdoc cref="IsAnyNaN(Vector3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAllNaN(this Vector2 vector) {
            return float.IsNaN(vector.x) && float.IsNaN(vector.y);
        }
        #endregion

        #region Quaternion
        /// <summary>
        /// Converts this vector to a quaternion.
        /// </summary>
        /// <param name="_euler">Vector to convert.</param>
        /// <returns>Quaternion created based on this vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ToQuaternion(this Vector3 _euler) {
			return Quaternion.Euler(_euler);
		}

        /// <summary>
        /// Rotates this vector by a quaternion.
        /// </summary>
        /// <param name="_vector">Vector to rotate.</param>
        /// <param name="_quaternion">Quaternion used to rotate the vector.</param>
        /// <returns>The rotated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Rotate(this Vector3 _vector, Quaternion _quaternion) {
			return _quaternion * _vector;
		}

        /// <summary>
        /// Rotates inverse this vector by a quaternion.
        /// </summary>
        /// <param name="_vector">Vector to rotate.</param>
        /// <param name="_quaternion">Quaternion used to rotate inverse the vector.</param>
        /// <returns>The rotated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RotateInverse(this Vector3 _vector, Quaternion _quaternion) {
			return Quaternion.Inverse(_quaternion) * _vector;
		}
        #endregion

        #region ToString
        /// <inheritdoc cref="ToStringX(Vector3, int)"/>
        public static string ToStringX(this Vector2 _value, int _decimals) {
			StringBuilder _builder = new StringBuilder("0.");
			for (int i = 0; i < _decimals; i++) {
				_builder.Append('#');
			}

			return _value.ToString(_builder.ToString());
		}

		/// <summary>
		/// Parse a specific vector into a string with a specific amount of decimals.
		/// </summary>
		/// <param name="_value">Vector value to parse.</param>
		/// <param name="_decimals">Total amount of decimals to show.</param>
		/// <returns>This vector value parsed into string.</returns>
		public static string ToStringX(this Vector3 _value, int _decimals) {
			StringBuilder _builder = new StringBuilder("0.");
			for (int i = 0; i < _decimals; i++) {
				_builder.Append('#');
			}

			return _value.ToString(_builder.ToString());
		}
        #endregion

        #region Utility
        /// <summary>
        /// Get if a specific value is contained within the range of this vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this Vector2 _vector, float _value) {
			return (_value >= _vector.x) && (_value <= _vector.y);
		}

        /// <inheritdoc cref="Contains(Vector2, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this Vector2Int _vector, float _value) {
            return (_value >= _vector.x) && (_value <= _vector.y);
        }

        /// <inheritdoc cref="Contains(Vector2, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this Vector2Int _vector, int _value) {
            return (_value >= _vector.x) && (_value <= _vector.y);
        }

        /// <summary>
        /// Get this <see cref="Vector3"/> as a <see cref="Vector2"/>.
        /// </summary>
        /// <param name="_vector"><see cref="Vector3"/> to convert.</param>
        /// <returns><see cref="Vector2"/> converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this Vector3 _vector) {
			return new Vector2(_vector.x, _vector.y);
		}

        /// <summary>
        /// Get this <see cref="Vector2"/> as a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="_vector"><see cref="Vector2"/> to convert.</param>
        /// <returns><see cref="Vector3"/> converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(this Vector2 _vector) {
            return new Vector3(_vector.x, _vector.y, 0f);
        }

        /// <summary>
        /// Get this <see cref="Vector3"/> as a <see cref="Vector2"/>, using its z as its y component.
        /// </summary>
        /// <param name="_vector"><see cref="Vector3"/> to convert.</param>
        /// <returns><see cref="Vector2"/> converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2Flat(this Vector3 _vector) {
            return new Vector2(_vector.x, _vector.z);
        }

        /// <summary>
        /// Get this <see cref="Vector2"/> as a <see cref="Vector3"/>, using the y as the z component.
        /// </summary>
        /// <param name="_vector"><see cref="Vector2"/> to convert.</param>
        /// <returns><see cref="Vector3"/> converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3Depth(this Vector2 _vector) {
            return new Vector3(_vector.x, 0f, _vector.y);
        }
        #endregion
    }
}

﻿using System;
using UnityEngine;

namespace EnhancedEditor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ProgressBarAttribute : PropertyAttribute
    {
        #region Fields
        /*****************************
         *******   CONSTANTS   *******
         ****************************/

        /// <summary>
        /// Default progress bar height (in pixels).
        /// </summary>
        public const float              DefaultHeight =         25;

        /// <summary>
        /// Default progress bar color.
        /// </summary>
        public const SuperColor         DefaultColor =          SuperColor.Sapphire;


        /**************************
         *******   FIELDS   *******
         *************************/

        /// <summary>
        /// Should this progress bar be editable in inspector.
        /// </summary>
        public readonly bool            IsEditable =            false;

        /// <summary>
        /// This progress bar height (in pixels).
        /// </summary>
        public readonly float           Height =                25;

        /// <summary>
        /// Progress bar maximum value.
        /// </summary>
        public readonly float           MaxValue =              0;

        /// <summary>
        /// Label displayed at the center of the progress bar.
        /// </summary>
        public readonly string          Label =                 string.Empty;

        /// <summary>
        /// Name of the variable acting as progress bar maximum value.
        /// </summary>
        public readonly string          MaxValueVariableName =  string.Empty;

        /// <summary>
        /// Color of this progress bar.
        /// </summary>
        public readonly SuperColor      Color =                 SuperColor.Sapphire;
        #endregion

        #region Constructors
        /********************************
         *******   CONSTRUCTORS   *******
         *******************************/

        /// <summary>
        /// Draws a progress bar.
        /// </summary>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        /// <param name="_isEditable">Should this progress bar be editable in inspector.</param>
        private ProgressBarAttribute(SuperColor _color, float _height, bool _isEditable = false)
        {
            Color = _color;
            Height = Mathf.Max(1, _height);
            IsEditable = _isEditable;
        }


        /// <summary>
        /// Draws this field as a progress bar.
        /// </summary>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        /// <param name="_isEditable">Should this progress bar be editable in inspector.</param>
        public ProgressBarAttribute(float _maxValue, SuperColor _color = DefaultColor, float _height = DefaultHeight, bool _isEditable = false) : this(_color, _height, _isEditable)
        {
            MaxValue = Mathf.Max(.1f, _maxValue);
        }

        /// <summary>
        /// Draws this field as a progress bar.
        /// </summary>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_maxValue">Progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        /// <param name="_isEditable">Should this progress bar be editable in inspector.</param>
        public ProgressBarAttribute(string _label, float _maxValue, SuperColor _color = DefaultColor, float _height = DefaultHeight, bool _isEditable = false) : this(_maxValue, _color, _height, _isEditable)
        {
            Label = _label;
        }

        /// <summary>
        /// Draws this field as a progress bar.
        /// </summary>
        /// <param name="_maxValueVariableName">Name of the variable acting as this progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        /// <param name="_isEditable">Should this progress bar be editable in inspector.</param>
        public ProgressBarAttribute(string _maxValueVariableName, SuperColor _color = DefaultColor, float _height = DefaultHeight, bool _isEditable = false) : this(_color, _height, _isEditable)
        {
            MaxValueVariableName = _maxValueVariableName;
        }

        /// <summary>
        /// Draws this field as a progress bar.
        /// </summary>
        /// <param name="_label">Label displayed at the middle of the progress bar.</param>
        /// <param name="_maxValueVariableName">Name of the variable acting as this progress bar maximum value.</param>
        /// <param name="_color">Progress bar color.</param>
        /// <param name="_height">Progress bar height (in pixels).</param>
        /// <param name="_isEditable">Should this progress bar be editable in inspector.</param>
        public ProgressBarAttribute(string _label, string _maxValueVariableName, SuperColor _color = DefaultColor, float _height = DefaultHeight, bool _isEditable = false) : this(_maxValueVariableName, _color, _height, _isEditable)
        {
            Label = _label;
        }
        #endregion
    }
}

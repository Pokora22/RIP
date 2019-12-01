﻿namespace Sisus
{
	public enum PropertyVisibility
	{
		/// <summary>
		/// Only show properties explicitly exposed with attributes
		/// like EditorBrowsable, Browsable(true), SerializeField or ShowInInspector
		/// </summary>
		AttributeExposedOnly = 0,

		/// <summary>
		/// All public auto-generated properties are shown, unless explicitly hidden
		/// with Attributes like HideInInspector or NonSerialized.
		/// Other properties are not shown, unless explicitly exposed with attributes
		/// like EditorBrowsable, Browsable(true), SerializeField or ShowInInspector.
		/// </summary>
		PublicAutoGenerated = 1,

		/// <summary>
		/// All public properties are shown, unless explicitly hidden
		/// with Attributes like HideInInspector or NonSerialized.
		/// </summary>
		AllPublic = 2
	}
}
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using System;
using AD.Types;
using AD.BASE;

namespace AD.Utility
{
	public abstract class ADWriter : IDisposable
	{
		protected HashSet<string> keysToDelete = new HashSet<string>();

		internal bool writeHeaderAndFooter = true;
		internal bool overwriteKeys = true;

		protected int serializationDepth = 0;
		public int serializationDepthLimit = 16;

        #region ES3Writer Abstract Methods

        internal abstract void WriteNull();

		internal virtual void StartWriteFile()
		{
			serializationDepth++;
		}

		internal virtual void EndWriteFile()
		{
			serializationDepth--;
		}

		internal virtual void StartWriteObject(string name)
		{
			serializationDepth++;
		}

		internal virtual void EndWriteObject(string name)
		{
			serializationDepth--;
		}

		internal virtual void StartWriteProperty(string name)
		{
			if (name == null)
				throw new ArgumentNullException("Key or field name cannot be NULL when saving data.");
			Debug.LogFormat("<b>" + name + "</b> (writing property)", null, serializationDepth);
		}

		internal virtual void EndWriteProperty(string name)
		{
		}

		internal virtual void StartWriteCollection()
		{
			serializationDepth++;
		}

		internal virtual void EndWriteCollection()
		{
			serializationDepth--;
		}

		internal abstract void StartWriteCollectionItem(int index);
		internal abstract void EndWriteCollectionItem(int index);

		internal abstract void StartWriteDictionary();
		internal abstract void EndWriteDictionary();
		internal abstract void StartWriteDictionaryKey(int index);
		internal abstract void EndWriteDictionaryKey(int index);
		internal abstract void StartWriteDictionaryValue(int index);
		internal abstract void EndWriteDictionaryValue(int index);

		public abstract void Dispose();

		#endregion

		#region ES3Writer Interface abstract methods

		internal abstract void WriteRawProperty(string name, byte[] bytes);

		internal abstract void WritePrimitive(int value);
		internal abstract void WritePrimitive(float value);
		internal abstract void WritePrimitive(bool value);
		internal abstract void WritePrimitive(decimal value);
		internal abstract void WritePrimitive(double value);
		internal abstract void WritePrimitive(long value);
		internal abstract void WritePrimitive(ulong value);
		internal abstract void WritePrimitive(uint value);
		internal abstract void WritePrimitive(byte value);
		internal abstract void WritePrimitive(sbyte value);
		internal abstract void WritePrimitive(short value);
		internal abstract void WritePrimitive(ushort value);
		internal abstract void WritePrimitive(char value);
		internal abstract void WritePrimitive(string value);
		internal abstract void WritePrimitive(byte[] value);

		#endregion

		protected ADWriter( bool writeHeaderAndFooter, bool overwriteKeys)
		{
			this.writeHeaderAndFooter = writeHeaderAndFooter;
			this.overwriteKeys = overwriteKeys;
		}

		/* User-facing methods used when writing randomly-accessible Key-Value pairs. */
		#region Write(key, value) Methods

		internal virtual void Write(string key, Type type, byte[] value)
		{
			StartWriteProperty(key);
			StartWriteObject(key);
			WriteType(type);
			WriteRawProperty("value", value);
			EndWriteObject(key);
			EndWriteProperty(key);
			MarkKeyForDeletion(key);
		}

		/// <summary>Writes a value to the writer with the given key.</summary>
		/// <param name="key">The key which uniquely identifies this value.</param>
		/// <param name="value">The value we want to write.</param>
		public virtual void Write<T>(string key, object value)
		{
			if (typeof(T) == typeof(object))
				Write(value.GetType(), key, value);
			else
				Write(typeof(T), key, value);
		}

		/// <summary>Writes a value to the writer with the given key, using the given type rather than the generic parameter.</summary>
		/// <param name="key">The key which uniquely identifies this value.</param>
		/// <param name="value">The value we want to write.</param>
		/// <param name="type">The type we want to use for the header, and to retrieve an ES3Type.</param>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public virtual void Write(Type type, string key, object value)
		{
			StartWriteProperty(key);
			StartWriteObject(key);
			WriteType(type);
			WriteProperty("value", value, ADType.GetOrCreateADType(type));
			EndWriteObject(key);
			EndWriteProperty(key);
			MarkKeyForDeletion(key);
		}

		#endregion

		#region Write(value) & Write(value, ES3Type) Methods

		/// <summary>Writes a value to the writer. Note that this should only be called within an ES3Type.</summary>
		/// <param name="value">The value we want to write.</param>
		/// <param name="memberReferenceMode">Whether we want to write UnityEngine.Object fields and properties by reference, by value, or both.</param>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public virtual void Write(object value, ES3.ReferenceMode memberReferenceMode = ES3.ReferenceMode.ByRef)
		{
			if (value == null) { WriteNull(); return; }

			var type = ADType.GetOrCreateADType(value.GetType());
			Write(value, type, memberReferenceMode);
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public virtual void Write(object value, ADType type, ES3.ReferenceMode memberReferenceMode = ES3.ReferenceMode.ByRef)
		{
			// Note that we have to check UnityEngine.Object types for null by casting it first, otherwise
			// it will always return false.
			if (value == null || (ReflectionExtension.IsAssignableFrom(typeof(UnityEngine.Object), value.GetType()) && value as UnityEngine.Object == null))
			{
				WriteNull();
				return;
			}

			// Deal with System.Objects
			if (type == null || type.type == typeof(object))
			{
				var valueType = value.GetType();
				type = ADType.GetOrCreateADType(valueType);

				if (type == null)
					throw new NotSupportedException("Types of " + valueType + " are not supported");

				if (!type.isCollection && !type.isDictionary)
				{
					StartWriteObject(null);
					WriteType(valueType);

					type.Write(value, this);

					EndWriteObject(null);
					return;
				}
			}

			if (type == null)
				throw new ArgumentNullException("ES3Type argument cannot be null.");
			if (type.isUnsupported)
			{
				if (type.isCollection || type.isDictionary)
					throw new NotSupportedException(type.type + " is not supported because it's element type is not supported. Please see the Supported Types guide for more information: https://docs.moodkie.com/easy-save-3/es3-supported-types/");
				else
					throw new NotSupportedException("Types of " + type.type + " are not supported. Please see the Supported Types guide for more information: https://docs.moodkie.com/easy-save-3/es3-supported-types/");
			}

			if (type.isPrimitive || type.isEnum)
				type.Write(value, this);
			else if (type.isCollection)
			{
				StartWriteCollection();
				((ADCollectionType)type).Write(value, this, memberReferenceMode);
				EndWriteCollection();
			}
			else if (type.isDictionary)
			{
				StartWriteDictionary();
				((ADDictionaryType)type).Write(value, this, memberReferenceMode);
				EndWriteDictionary();
			}
			else
			{
				if (type.type == typeof(GameObject))
					((ADType_GameObject)type).saveChildren = settings.saveChildren;

				StartWriteObject(null);

				if (type.isADTypeUnityObject)
					((ADUnityObjectType)type).WriteObject(value, this, memberReferenceMode);
				else
					type.Write(value, this);
				EndWriteObject(null);
			}
		}

		#endregion

		/* Writes a property as a name value pair. */
		#region WriteProperty(name, value) methods

		public virtual void WriteProperty(string name, object value)
		{
			if (SerializationDepthLimitExceeded())
				return;

			StartWriteProperty(name); Write(value); EndWriteProperty(name);
		}

		/// <summary>Writes a field or property to the writer. Note that this should only be called within an ES3Type.</summary>
		/// <param name="name">The name of the field or property.</param>
		/// <param name="value">The value we want to write.</param>
		public virtual void WriteProperty<T>(string name, object value)
		{
			WriteProperty(name, value, ADType.GetOrCreateADType(typeof(T)));
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public virtual void WriteProperty(string name, object value, ADType type)
		{
			WriteProperty(name, value, type);
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public virtual void WriteProperty(string name, object value, ADType type, ES3.ReferenceMode memberReferenceMode)
		{
			if (SerializationDepthLimitExceeded())
				return;

			StartWriteProperty(name);
			Write(value, type, memberReferenceMode);
			EndWriteProperty(name);
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public virtual void WritePropertyByRef(string name, UnityEngine.Object value)
		{
			if (SerializationDepthLimitExceeded())
				return;

			StartWriteProperty(name);
			if (value == null)
			{
				WriteNull();
				return;
			};
			StartWriteObject(name);
			WriteRef(value);
			EndWriteObject(name);
			EndWriteProperty(name);
		}

		/// <summary>Writes a private property to the writer. Note that this should only be called within an ES3Type.</summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="objectContainingProperty">The object containing the property we want to write.</param>
		public void WritePrivateProperty(string name, object objectContainingProperty)
		{
			var property = ReflectionExtension.GetADReflectedProperty(objectContainingProperty.GetType(), name);
			if (property.IsNull)
				throw new MissingMemberException("A private property named " + name + " does not exist in the type " + objectContainingProperty.GetType());
			WriteProperty(name, property.GetValue(objectContainingProperty), ADType.GetOrCreateADType(property.MemberType));
		}

		/// <summary>Writes a private field to the writer. Note that this should only be called within an ES3Type.</summary>
		/// <param name="name">The name of the field.</param>
		/// <param name="objectContainingField">The object containing the property we want to write.</param>
		public void WritePrivateField(string name, object objectContainingField)
		{
			var field = ReflectionExtension.GetADReflectedProperty(objectContainingField.GetType(), name);
			if (field.IsNull)
				throw new MissingMemberException("A private field named " + name + " does not exist in the type " + objectContainingField.GetType());
			WriteProperty(name, field.GetValue(objectContainingField), ADType.GetOrCreateADType(field.MemberType));
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public void WritePrivateProperty(string name, object objectContainingProperty, ADType type)
		{
			var property = ReflectionExtension.GetADReflectedProperty(objectContainingProperty.GetType(), name);
			if (property.IsNull)
				throw new MissingMemberException("A private property named " + name + " does not exist in the type " + objectContainingProperty.GetType());
			WriteProperty(name, property.GetValue(objectContainingProperty), type);
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public void WritePrivateField(string name, object objectContainingField, ADType type)
		{
			var field = ReflectionExtension.GetADReflectedProperty(objectContainingField.GetType(), name);
			if (field.IsNull)
				throw new MissingMemberException("A private field named " + name + " does not exist in the type " + objectContainingField.GetType());
			WriteProperty(name, field.GetValue(objectContainingField), type);
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public void WritePrivatePropertyByRef(string name, object objectContainingProperty)
		{
			var property = ReflectionExtension.GetADReflectedProperty(objectContainingProperty.GetType(), name);
			if (property.IsNull)
				throw new MissingMemberException("A private property named " + name + " does not exist in the type " + objectContainingProperty.GetType());
			WritePropertyByRef(name, (UnityEngine.Object)property.GetValue(objectContainingProperty));
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public void WritePrivateFieldByRef(string name, object objectContainingField)
		{
			var field = ReflectionExtension.GetADReflectedMember(objectContainingField.GetType(), name);
			if (field.IsNull)
				throw new MissingMemberException("A private field named " + name + " does not exist in the type " + objectContainingField.GetType());
			WritePropertyByRef(name, (UnityEngine.Object)field.GetValue(objectContainingField));
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public void WriteType(Type type)
		{
			WriteProperty(ADType.typeFieldName, ReflectionExtension.GetTypeString(type));
		}

		#endregion

		#region Create methods

		/// <summary>Creates a new ADWriter.</summary>
		/// <param name="filePath">The relative or absolute path of the file we want to write to.</param>
		/// <param name="settings">The settings we want to use to override the default settings.</param>
		public static ADWriter Create(string filePath, ES3Settings settings)
		{
			return Create(new ES3Settings(filePath, settings));
		}

		/// <summary>Creates a new ADWriter.</summary>
		/// <param name="settings">The settings we want to use to override the default settings.</param>
		public static ADWriter Create(ES3Settings settings)
		{
			return Create(settings, true, true, false);
		}

		#endregion

		/*
		 * Checks whether serialization depth limit has been exceeded
		 */
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		protected bool SerializationDepthLimitExceeded()
		{
			if (serializationDepth > serializationDepthLimit)
			{
				Debug.LogFormat("Serialization depth limit of " + serializationDepthLimit + " has been exceeded, indicating that there may be a circular reference." +
					"\nIf this is not a circular reference, you can increase the depth by going to Window > AD > Settings > Advanced Settings > Serialization Depth Limit");
				return true;
			}
			return false;
		}

		/*
		 * 	Marks a key for deletion.
		 * 	When merging files, keys marked for deletion will not be included.
		 */
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public virtual void MarkKeyForDeletion(string key)
		{
			keysToDelete.Add(key);
		}

		/*
		 * 	Merges the contents of the non-temporary file with this ADWriter,
		 * 	ignoring any keys which are marked for deletion.
		 */
		protected void Merge()
		{
			using (var reader = ES3Reader.Create(settings))
			{
				if (reader == null)
					return;
				Merge(reader);
			}
		}

		/*
		 * 	Merges the contents of the ES3Reader with this ADWriter,
		 * 	ignoring any keys which are marked for deletion.
		 */
		protected void Merge(ES3Reader reader)
		{
			foreach (KeyValuePair<string, ES3Data> kvp in reader.RawEnumerator)
				if (!keysToDelete.Contains(kvp.Key) || kvp.Value.type == null) // Don't add keys whose data is of a type which no longer exists in the project.
					Write(kvp.Key, kvp.Value.type.type, kvp.Value.bytes);
		}

		/// <summary>Stores the contents of the writer and overwrites any existing keys if overwriting is enabled.</summary>
		public virtual void Save()
		{
			Save(overwriteKeys);
		}

		/// <summary>Stores the contents of the writer and overwrites any existing keys if overwriting is enabled.</summary>
		/// <param name="overwriteKeys">Whether we should overwrite existing keys.</param>
		public virtual void Save(bool overwriteKeys)
		{
			if (overwriteKeys)
				Merge();
			EndWriteFile();
			Dispose();

			// If we're writing to a location which can become corrupted, rename the backup file to the file we want.
			// This prevents corrupt data.
			if (settings.location == ES3.Location.File || settings.location == ES3.Location.PlayerPrefs)
				ES3IO.CommitBackup(settings);
		}
	}
}

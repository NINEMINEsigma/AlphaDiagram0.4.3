using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using System;
using AD.Types;
using AD.BASE;
using System.Globalization;
using System.Text;

namespace AD.Utility
{
	public abstract class ADWriter : IDisposable
	{
		protected HashSet<string> keysToDelete = new HashSet<string>();

		internal bool writeHeaderAndFooter = true;
		internal bool overwriteKeys = true;

		protected int serializationDepth = 0;
		public int serializationDepthLimit = 16;

		#region Abstract Methods

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

		public ADWriter(bool writeHeaderAndFooter, bool overwriteKeys)
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
			//MarkKeyForDeletion(key);
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
			//MarkKeyForDeletion(key);
		}

		#endregion

		#region Write(value) & Write(value, ES3Type) Methods

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public virtual void Write(object value)
		{
			if (value == null) { WriteNull(); return; }

			var type = ADType.GetOrCreateADType(value.GetType());
			Write(value, type);
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public virtual void Write(object value, ADType type)
		{
			// Note that we have to check UnityEngine.Object types for null by casting it first, otherwise
			// it will always return false.
			if (value == null || (ReflectionExtension.IsAssignableFrom(typeof(UnityEngine.Object), value.GetType()) && value as UnityEngine.Object == null))
			{
				WriteNull();
				return;
			}
			if (type == null)
				throw new ArgumentNullException("ADType argument cannot be null.");

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

			if (type.isUnsupported)
			{
				if (type.isCollection || type.isDictionary)
					throw new NotSupportedException(type.type + " is not supported because it's element type is not supported");
				else
					throw new NotSupportedException("Types of " + type.type + " are not supported");
			}

			if (type.isPrimitive || type.isEnum)
				type.Write(value, this);
			else if (type.isCollection)
			{
				StartWriteCollection();
				((ADCollectionType)type).Write(value, this);
				EndWriteCollection();
			}
			else if (type.isDictionary)
			{
				StartWriteDictionary();
				((ADDictionaryType)type).Write(value, this);
				EndWriteDictionary();
			}
			else
			{
				StartWriteObject(null);
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
			if (SerializationDepthLimitExceeded())
				return;

			StartWriteProperty(name);
			Write(value, type);
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
		public void WriteType(Type type)
		{
			WriteProperty(ADType.typeFieldName, ReflectionExtension.GetTypeString(type));
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
		/*[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public virtual void MarkKeyForDeletion(string key)
		{
			keysToDelete.Add(key);
		}*/
	}

	public class ADJSONWriter : ADWriter
	{
		internal StreamWriter baseWriter;

		public Encoding encoding = System.Text.Encoding.UTF8;

		private bool isFirstProperty = true;

		public ADJSONWriter(Stream stream) : this(stream, true, true) { }

		internal ADJSONWriter(Stream stream, bool writeHeaderAndFooter, bool mergeKeys) : base(writeHeaderAndFooter, mergeKeys)
		{
			baseWriter = new StreamWriter(stream);
			StartWriteFile();
		}

		#region WritePrimitive(value) methods.

		internal override void WritePrimitive(int value) { baseWriter.Write(value); }
		internal override void WritePrimitive(float value) { baseWriter.Write(value.ToString("R", CultureInfo.InvariantCulture)); }
		internal override void WritePrimitive(bool value) { baseWriter.Write(value ? "true" : "false"); }
		internal override void WritePrimitive(decimal value) { baseWriter.Write(value.ToString(CultureInfo.InvariantCulture)); }
		internal override void WritePrimitive(double value) { baseWriter.Write(value.ToString("R", CultureInfo.InvariantCulture)); }
		internal override void WritePrimitive(long value) { baseWriter.Write(value); }
		internal override void WritePrimitive(ulong value) { baseWriter.Write(value); }
		internal override void WritePrimitive(uint value) { baseWriter.Write(value); }
		internal override void WritePrimitive(byte value) { baseWriter.Write(System.Convert.ToInt32(value)); }
		internal override void WritePrimitive(sbyte value) { baseWriter.Write(System.Convert.ToInt32(value)); }
		internal override void WritePrimitive(short value) { baseWriter.Write(System.Convert.ToInt32(value)); }
		internal override void WritePrimitive(ushort value) { baseWriter.Write(System.Convert.ToInt32(value)); }
		internal override void WritePrimitive(char value) { WritePrimitive(value.ToString()); }
		internal override void WritePrimitive(byte[] value) { WritePrimitive(System.Convert.ToBase64String(value)); }


		internal override void WritePrimitive(string value)
		{
			baseWriter.Write("\"");

			// Escape any quotation marks within the string.
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				switch (c)
				{
					case '\"':
					case '“':
					case '”':
					case '\\':
					case '/':
						baseWriter.Write('\\');
						baseWriter.Write(c);
						break;
					case '\b':
						baseWriter.Write("\\b");
						break;
					case '\f':
						baseWriter.Write("\\f");
						break;
					case '\n':
						baseWriter.Write("\\n");
						break;
					case '\r':
						baseWriter.Write("\\r");
						break;
					case '\t':
						baseWriter.Write("\\t");
						break;
					default:
						baseWriter.Write(c);
						break;
				}
			}
			baseWriter.Write("\"");
		}

		internal override void WriteNull()
		{
			baseWriter.Write("null");
		}

		#endregion

		#region Format-specific methods

		private static bool CharacterRequiresEscaping(char c)
		{
			return c == '\"' || c == '\\' || c == '“' || c == '”';
		}

		private void WriteCommaIfRequired()
		{
			if (!isFirstProperty)
				baseWriter.Write(',');
			else
				isFirstProperty = false;
			WriteNewlineAndTabs();
		}

		internal override void WriteRawProperty(string name, byte[] value)
		{
			StartWriteProperty(name); baseWriter.Write(encoding.GetString(value, 0, value.Length)); EndWriteProperty(name);
		}

		internal override void StartWriteFile()
		{
			if (writeHeaderAndFooter)
				baseWriter.Write('{');
			base.StartWriteFile();
		}

		internal override void EndWriteFile()
		{
			base.EndWriteFile();
			WriteNewlineAndTabs();
			if (writeHeaderAndFooter)
				baseWriter.Write('}');
		}

		internal override void StartWriteProperty(string name)
		{
			base.StartWriteProperty(name);
			WriteCommaIfRequired();
			Write(name);

			baseWriter.Write(' ');
			baseWriter.Write(':');
			baseWriter.Write(' ');
		}

		internal override void EndWriteProperty(string name)
		{
			// It's not necessary to perform any operations after writing the property in JSON.
			base.EndWriteProperty(name);
		}

		internal override void StartWriteObject(string name)
		{
			base.StartWriteObject(name);
			isFirstProperty = true;
			baseWriter.Write('{');
		}

		internal override void EndWriteObject(string name)
		{
			base.EndWriteObject(name);
			// Set isFirstProperty to false incase we didn't write any properties, in which case
			// WriteCommaIfRequired() is never called.
			isFirstProperty = false;
			WriteNewlineAndTabs();
			baseWriter.Write('}');
		}

		internal override void StartWriteCollection()
		{
			base.StartWriteCollection();
			baseWriter.Write('[');
			WriteNewlineAndTabs();
		}

		internal override void EndWriteCollection()
		{
			base.EndWriteCollection();
			WriteNewlineAndTabs();
			baseWriter.Write(']');
		}

		internal override void StartWriteCollectionItem(int index)
		{
			if (index != 0)
				baseWriter.Write(',');
		}

		internal override void EndWriteCollectionItem(int index)
		{
		}

		internal override void StartWriteDictionary()
		{
			StartWriteObject(null);
		}

		internal override void EndWriteDictionary()
		{
			EndWriteObject(null);
		}

		internal override void StartWriteDictionaryKey(int index)
		{
			if (index != 0)
				baseWriter.Write(',');
		}

		internal override void EndWriteDictionaryKey(int index)
		{
			baseWriter.Write(':');
		}

		internal override void StartWriteDictionaryValue(int index)
		{
		}

		internal override void EndWriteDictionaryValue(int index)
		{
		}

		#endregion

		public override void Dispose()
		{
			baseWriter.Dispose();
		}

		public void WriteNewlineAndTabs()
		{
			baseWriter.Write(Environment.NewLine);
			for (int i = 0; i < serializationDepth; i++)
				baseWriter.Write('\t');
		}
	}
}

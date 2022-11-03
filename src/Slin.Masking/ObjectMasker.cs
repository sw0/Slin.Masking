using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Xml.Linq;
using System.Diagnostics.Contracts;
using System.Text;
using System.Reflection;

namespace Slin.Masking
{
	/// <summary>
	/// <see cref="ObjectMasker"/> is supposed to support to iterate the object and mask properties if the key/value matches the rules. 
	/// The masking is actually performed by <see cref="IMasker"/>.
	/// </summary>
	public class ObjectMasker : IObjectMasker
	{
		protected readonly IMasker _masker = null;
		protected readonly IJsonMasker _jMasker = null;
		protected readonly IXmlMasker _xMasker = null;
		protected readonly IObjectMaskingOptions _options;

		protected bool MaskJsonSerializedEnabled => _options.MaskJsonSerializedEnabled
			&& _options.SerializedKeys != null && _options.SerializedKeys.Any();
		protected bool MaskXmlSerializedEnabled => _options.MaskXmlSerializedEnabled
			&& _options.SerializedKeys != null && _options.SerializedKeys.Any();
		protected bool MaskXmlSerializedOnXmlAttributeEnabled => _options.MaskXmlSerializedOnXmlAttributeEnabled;
		protected bool MaskJsonSerializedOnXmlAttributeEnabled => _options.MaskJsonSerializedOnXmlAttributeEnabled;

		protected bool IsMaskUrlEnabled => _options.MaskUrlEnabled && _options.UrlKeys != null
			&& _options.UrlKeys.Any();

		/// <summary>
		/// indicates whether to support nested key-value-pairs. 
		/// Like to support mask {"key":"ssn", "value":"123456789"}
		/// </summary>
		protected bool MaskNestedKvpEnabled => _options.MaskNestedKvpEnabled;

		/// <summary>
		/// Enabled is exposed to caller. It has no internal logic check on it inside <see cref="ObjectMasker"/>.
		/// </summary>
		public bool Enabled => _options.Enabled;

		public ObjectMasker(IMasker masker, IObjectMaskingOptions options)
		{
			_masker = masker;
			var jMasker = new JsonMasker(masker, options);
			var xMasker = new XmlMasker(masker, options);

			jMasker.SetXmlMasker(xMasker);
			xMasker.SetJsonMasker(jMasker);

			_jMasker = jMasker;
			_xMasker = xMasker;

			_options = options ?? new ObjectMaskingOptions();
		}

		public bool TryParse(string value, out JsonElement? node, bool basicValidation = true)
		{
			Contract.Assert(_jMasker != null, "_jMasker should be initialized");
			return _jMasker.TryParse(value, out node, basicValidation);
		}

		public bool TryParse(string value, out XElement node, bool basicValidation = true)
		{
			Contract.Assert(_xMasker != null, "_xMasker should be initialized");
			return _xMasker.TryParse(value, out node, basicValidation);
		}

		/// <summary>
		/// MaskObject and append the masked reuslt to builder.
		/// NOTE: if value is XElement or JsonNode, the original instance will be masked if masking happened. 
		/// If it's a string but not a valid JSON,XML, do nothing and direct return original value.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="builder"></param>
		public void MaskObject(object value, StringBuilder builder)
		{
			if (value == null) return;

			if (value is JsonElement ele)
			{
				_jMasker.MaskObject(ele, builder);
			}
			else if (value is JsonNode node)
			{
				//todo please don't use JsonNode
				var result = MaskObject(node);
				builder.Append(result);
			}
			else if (value is XElement xEle)
			{
				var result = _xMasker.MaskXmlElementString(xEle);
				builder.Append(result);
			}
			else if (value is string str)
			{
				if (_jMasker.TryParse(str, out var ele2))
				{
					//this is what we expected path [1.1]
					_jMasker.MaskObject(ele2, builder);
				}
				else if (_xMasker.TryParse(str, out var parsedNode))
				{
					//this is what we expected path [1.2]
					var result = _xMasker.MaskXmlElementString(parsedNode);
					builder.Append(result);
				}
				else
				{
					//actually, I think this is not expected.
					//this is special processing, different with other simple types, like boolean, Enum, etc
					//todo return str or use _jMasker.MaskObject(value, builder) ?
					builder.Append(str); //keep it unchanged
				}
			}
			else
			{
				//ACTUALLY, this this is the case that we expected. [1.0]
				_jMasker.MaskObject(value, builder);
			}
		}

		/// <summary>
		/// MaskObject and append the masked reuslt to builder.
		/// NOTE: if value is XElement or JsonNode, the original instance will be masked if masking happened.
		/// </summary>
		/// <param name="value">expecting a object or string of Json,Xml</param>
		/// <returns></returns>
		public string MaskObject(object value)
		{
			if (value == null) return "";

			var builder = new StringBuilder();

			MaskObject(value, builder);

			return builder.ToString();
		}


		#region -- URL --

		public string MaskUrl(string url, bool maskParamters = true, params UrlMaskingPattern[] overwrittenPatterns) => _masker.MaskUrl(url, maskParamters, overwrittenPatterns);

		#endregion


		#region -- XML -- 

		public string MaskXmlElementString(XElement element)
		{
			return _xMasker.MaskXmlElementString(element);
		}


		#endregion

	}
}

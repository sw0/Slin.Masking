using System;
using System.Collections.Generic;

namespace Slin.Masking
{
    /// <summary>
    /// ObjectMaskingOptions is options for <see cref="ObjectMasker"/>.
    /// </summary>
    public class ObjectMaskingOptions : IObjectMaskingOptions
    {
        /// <summary>
        /// default false. It works with <see cref="UrlKeys"/> and rules defined in <see cref="UrlMaskingPattern"/>.
        /// </summary>
        public bool MaskUrlEnabled { get; set; }

        /// <summary>
        /// specify which keys will treated as URL or Query(kvp). Will be used when <see cref="MaskUrlEnabled"/> is true.
        /// </summary>
        public List<string> UrlKeys { get; set; } = new List<string>();

        /// <summary>
        /// SerializedKeys with string value value will be tried deserialized and get masked if <see cref="MaskJsonSerializedEnabled"/> or <see cref="MaskXmlSerializedEnabled"/> is true.
        /// </summary>
        public List<string> SerializedKeys { get; set; } = new List<string>();

        /// <summary>
        /// if true, it will try to parse value as JSON for keys found in <see cref="SerializedKeys"/>. If parsed successfully, it will do the masking recursively for the parsed object. Working with <see cref="SerializedKeys"/>, <see cref="JsonMinLength"/>
        /// </summary>
        public bool MaskJsonSerializedEnabled { get; set; } = true;
        /// <summary>
        /// default: true. Indicates to make deserialized string as JSON and keep it as JSON or unchanged as string.
        /// </summary>
        public bool MaskJsonSerializedParsedAsNode { get; set; } = true;

        /// <summary>
        /// default: false. It works with <see cref="SerializedKeys"/>, <see cref="XmlMinLength"/>.
        /// </summary>
        public bool MaskXmlSerializedEnabled { get; set; } = false;
        /// <summary>
        /// default false. If true, it will try to parse xml attribute as XML and process masking recursively.
        /// </summary>
        public bool MaskXmlSerializedOnXmlAttributeEnabled { get; set; } = false;
        /// <summary>
        /// default false. If true, it will try to parse xml attribute as JSON and process masking recursively.
        /// </summary>
        public bool MaskJsonSerializedOnXmlAttributeEnabled { get; set; } = false;
        /// <summary>
        /// Default true. If any of <see cref="MaskJsonSerializedEnabled"/> or <see cref="MaskXmlSerializedEnabled"/> is enabled. and this is enabled also, it will be used to indicates comparison for Serialized Keys or Url Keys against property name
        /// </summary>
        public bool SerializedKeysCaseSensitive { get; set; } = true;

        /// <summary>
        /// default:false, if you need to enable number mask, set it to true. For example, account's balance.
        /// </summary>
        public bool MaskJsonNumberEnabled { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool MaskNestedKvpEnabled { get; set; }

        /// <summary>
        /// if <see cref="MaskNestedKvpEnabled"/> is true. <see cref="KeyKeyValueKeys"/> indicates which key-key-value-keys specified. If not set, it will use default settings: 'Key:Value' and 'key:value'. Please be aware key names specified here are case-sensitive.
        /// </summary>
        public List<KeyKeyValueKey> KeyKeyValueKeys { get; set; } = new List<KeyKeyValueKey>(KeyKeyValueKey.DefaultKeyKeyValueKeys);

        /// <summary>
        /// Default: 3. 
        /// If value.Length is less then N, masker will bypass it, because it usually does not contain secnsitive data.
        /// For name it might be short. So set it to 3. Like firstname, lastname.
        /// </summary>
        public int ValueMinLength { get; set; } = 3;
        /// <summary>
        /// default: 30. If string length is less than N, it will bypass parsing Xml document.
        /// Because, such document is too small and probably does not contains data eligible for masking.
        /// This is used when <see cref="MaskXmlSerializedEnabled"/> is enabled.
        /// </summary>
        public int XmlMinLength { get; set; } = 30;
        /// <summary>
        /// default: 15. If string length is less than N, it will bypass parsing Json document.
        /// null or empty string is actually a simple document, probably does not contains data eligible for masking
        /// This is used when <see cref="MaskJsonSerializedEnabled"/> is enabled.
        /// </summary>
        public int JsonMinLength { get; set; } = 15;

        /// <summary>
        /// default: 64
        /// </summary>
        public int JsonMaxDepth { get; set; } = 64;//default

        /// <summary>
        /// allowed characters
        /// </summary>
        public string JsonAllowedCharacters { get; set; }

        /// <summary>
        /// allowed unicoderanges, i.e. CjkUnifiedIdeographs
        /// </summary>
        public string[] JsonAllowedUnicodeRanges { get; set; }

        /// <summary>
        /// default: Default. That is non-object value in array will be bypassed.
        /// </summary>
        /// <example>
        /// given {"ssn":["101123456","101222333"]}, these two ssn would not be masked.
        /// </example>
        public ModeIfArray GlobalModeForArray { get; set; }

        /// <summary>
        /// precondition <see cref="MaskUrlEnabled"/> is true and <see cref="UrlKeys"/> are specified. And it contains list of regular expressions, find matched key-value from values (matched by <see cref="UrlKeys"/>) and do the masking. For query, it will auto recognize it and do the masking by key-value. Of cause, if you want you can use patterns also for query part if necessary. Note: using regular expression has a bit affection in performance if URLs found.
        /// But please be aware
        /// </summary>
        /// <example><![CDATA[
        /// we can use following regular expression to find the first name and last name from URL like https://tainisoft.com/api/users/filter/firstname/Joe/lastname/Jobs
        /// * "/firstname/(?<firstName>[^/]+/lastname/(?<lastName>[^/]+"
        /// * or you can use two expression for each: "/firstname/(?<firstName>[^/]+/" and "/lastname/(?<lastName>[^/]+"
        /// ]]>
        /// </example>
        public List<UrlMaskingPattern> UrlMaskingPatterns { get; set; } = new List<UrlMaskingPattern>();
    }
}

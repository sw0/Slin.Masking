# Slin.Masking

[TOC]

# Overview

Slin.Masking contains following two projects:

## Slin.Masking 

Slin.Masking provides basic masking feature, base on given key and value or URL. Masking rules are required, either by setting file or programing way to get `IMaskingProfile` instance.

## Slin.Masking.NLog

Slin.Masking.NLog provides a couple of extension methods that allow developer easier to use masking along with NLog.



# Get Started of Slin.Masking

In NetCore application (including Console or Web application). We need to do following steps:

> 1. Install package

```powershell
dotnet add package Slin.Masking.NLog
```

> 2. Setup NLog

```csharp
// Like in Program.cs file

var logger = LogManager
	.Setup(setupBuilder: (setupBuilder) => setupBuilder.UseMasking("masking.json")) //HERE
	.GetCurrentClassLogger();
```

In above code, it will read masking options and rules from following files:

* masking.json
* masking.custom.json

Or you can use your another approach:

```
var profile = new MaskingProfile(){
   NamedFormatterDefintions = new Dictionary<string, ValueFormatterDefinition>(StringComparer.OrdinalIgnoreCase),
   Rules = new Dictionary<string, MaskRuleDefinition>{
      {"pan": new MaskRuleDefinition{Formatters = new List<ValueFormatterDefinition>{
        new ValueFormatterDefinition{Format = "*"}
      }}}
   }
};
var logger = LogManager
	.Setup(setupBuilder: (setupBuilder) => setupBuilder.UseMasking(profile)) //HERE
	.GetCurrentClassLogger();
```



> 3. Masking configuration file example

## Overview

* object masking options
* masking rules settings
  * NamedFormatterDefintions
  * Rules

## Masking options 

Masking options are list below. And all these options are optional, if you're not clear about what it does, let it be or remove them from json file, hence default values will be used.

* MaskUrlEnabled
* MaskJsonSerializedEnabled
* MaskXmlSerializedEnabled
* MaskXmlSerializedOnXmlAttributeEnabled
* MaskJsonSerializedOnXmlAttributeEnabled
* MaskJsonNumberEnabled
* MaskNestedKvpEnabled
* KeyKeyValueKeys
* ValueMinLength
* XmlMinLength
* JsonMinLength

## Masking Rules

Masking rules settings contains:

* NamedFormatterDefintions: this is a group of named formatters, it like a shared definition. **NOTE:** it does not work unless you define the `Rules`.
* Rules: Rules must be provided. It contains one or more formatters. Formatters can be defined directly or refer to NamedFormatterDefintions by "Name".

```json
{
  "Masking": {
    "Enabled": true, //default: true
    "MaskUrlEnabled": true, //default: false, work with UrlKeys
    "MaskJsonSerializedEnabled": true, //default: true, work with SerializedKeys
    "MaskXmlSerializedEnabled": true, //Not Implemented
    "MaskXmlSerializedOnXmlAttributeEnabled": true, //default false
    "MaskJsonSerializedOnXmlAttributeEnabled": true, //default false
    "MaskJsonNumberEnabled": true, //this works only in JSON
    "MaskNestedKvpEnabled": true, //default: false, this will support mask data like {"Key":"ssn", "Value":"123456789"}
    "KeyKeyValueKeys": [ //if null and MaskNestedKvpEnabled is true, it will use "key""value" and "Key","Value".
      {
        "KeyKeyName": "Key",
        "ValueKeyName": "Value"
      },
      {
        "KeyKeyName": "key",
        "ValueKeyName": "value"
      }
    ],
    "ValueMinLength": 3, //if key matched, but value's length <=3, it will skip masking.
    "XmlMinLength": 15, //system setting, usually no need to set. it bypasses deserializing if length < N
    "JsonMinLength": 10, //system setting, usualy no need to set. it bypasses deserializing if length < N. null or empty no need to consider also.
    "SerializedKeysCaseSensitive": false,
    //UrlKeys: if MaskUrlEnabled is enabled and key is matched here, it will mask URL/kvp base on UrlMaskingPatterns
    "UrlKeys": [ "requestUrl", "query", "kvpFIEld", "kvpfield" ],
    "SerializedKeys": [ "Body", "ResponseBody", "reserialize" ],
    "UrlMaskingPatterns": [ //IgnoreCase: default true
      {
        "Pattern": "firstname/(?<firstName>[^/]+)|lastName/(?<lastname>[^/\\?]+)",
        "IgnoreCase": true
      },
      {
        "Pattern": "pan/(?<pan>\\d{15,16})"
      }
    ],
    "NamedFormatterDefintions": {
      //NOTE: definition names here are case-insensitive.
      //For a given Name(unique and ignore case), the formatter defintion contains: 
      //  valid Format, 
      //  optional ValuePattern and IgnoreCase for value pattern. ValuePattern can be regular expression(simple regex usage).
      //  default true of Enabled.
      //  Enabled: default true. BUT no need to set it to False here.
      "null": { "Format": "null" },
      "Empty": { "Format": "EMPTY" },
      "REDACTED": { "Format": "REDACTED" },
      "password": { "Format": "*6" },
      "dob": { "Format": "REDACTED" },
      "name": { "Format": "L2*" },
      "ssn": { "Format": "*" },
      "phone": { "Format": "L3R4" },
      "email": {
        "Format": "L3*@",
        "Description": "email is special, shawn@a.cn will be masked as 'sha**@a.cn'"
      },
      "pan": {
        "ValuePattern": "^\\d{15,16}$",
        "Format": "L4*R4",
        //IgnoreCase is working with ValuePattern.
        "IgnoreCase": false,
        "Enabled": true //default true
      },
      "Remove": { "RemoveNode": true } //todo not supported yet, do we need it?
    },
    "Rules": {
      //NOTE: rule key by default is case-insensitive, so does name of formatter. 
      //It's controlled by setting:KeyCaseInsensitive
      "SSN": { "Formatters": [ { "Name": "SSN" } ] },
      "DOB": { "Formatters": [ { "Name": "dob" } ] },
      "Pan": {
        "KeyName": "^pan|PersonalAccountNumber|PrimaryAccountNumber$",
        //IgnoreKeyCase is working with KeyName
        "IgnoreKeyCase": true,
        "Formatters": [ { "Name": "pan" } ]
      },
      "Balance": { "Formatters": [ { "Format": "null" } ] },
      "FirstName": { "Formatters": [ { "Name": "Name" } ] },
      "LastName": { "Formatters": [ { "Name": "Name" } ] },
      "Email": { "Formatters": [ { "Name": "email" } ] },
      "Password": { "Formatters": [ { "Format": "*" } ] },
      "PhoneNumber": { "Formatters": [ { "Name": "phone" } ] },
      "imaGeData": { "Formatters": [ { "name": "REDACTED" } ] }
    }
  }
}
```

**NOTE:**

Optional, you can add another file `masking.custom.json`, which can overwrite those in `masking.json`.

## nlog.config file

Here, Slin.Masking.NLog is opinioned for JSON layout. In Slin.Masking.NLog, `EventPropertiesMaskLayoutRenderer` with name `event-properties-masker` was introduced and can be used like below:

```

		<target xsi:type="File" name="traffic"
			  fileName="C:/temp/webapi6/traffic_${shortdate:format:yyyyMMdd}.log"
			  archiveFileName="C:/temp/webapi6/traffic_${shortdate:format:yyyyMMdd}.log.{#}" archiveAboveSize="80000000" archiveNumbering="Rolling" maxArchiveFiles="10"
			  >
			<layout xsi:type="JsonLayout" includeAllProperties="false" includeMdc="false">
				<attribute name="time" layout="${date:format:yyyy-MM-ddTHH\:mm\:ss.fff}" />
				<attribute name="level" layout="${pad:padding=5:inner=${level:uppercase=true}}"/>
				<!--<attribute name="servicecontext" layout="${mdlc:item=LogContext}"/>-->
				<attribute name="thread" layout="${threadid}"/>
				<attribute name="correlationId" layout="${mdlc:item=correlationId}"/>
				<attribute name="requestPath" layout="${event-properties:RequestPath}"/>
				<attribute name="logger" layout="${logger:shortName=true}"/>
				<attribute name="clientRequestId" layout="${aspnet-request-headers:HeaderNames=X-Request-ID:valuesOnly=true}"/>
				<attribute name="clientIp" layout="${aspnet-request-ip}"/>
				<attribute name="eventName" layout="${event-properties:EventName}"/>
				<attribute name="requestUrl" layout="${aspnet-request-url}"/>
				<attribute name="details" encode="false" layout="${event-properties-masker}" />				
				<!--<attribute name="data" encode="false" >
					<layout type='JsonLayout' includeAllProperties="true" maxRecursionLimit="20"/>
				</attribute>-->
				<attribute name="exception" layout="${exception:format=ToString}" />
			</layout>
		</target>
```

It has following properties:

1. Mode, and `Mode` must be one of these: `object`, `url`,`reserialize` (**all case-sensitive here!**). `object` is default value.
   * object: it will try mask the object base on the rules. 
   * url: need work with property `Item`. it will try to get the speicific item as string and mask it.
   * reserialize: need `Item` specified. It will get the value by specific key set in `Item`, and try to deserialize it as JSON or XML document and mask the object.
2. Item, it's used to specify the specific object by key set in `Item` here and do the masking. If item is not found in event properties, it does nothing.
3. Disabled: Boolean, default `true`, which is used to indicates whether enable masking or not!

Example:

1. Just use `event-properties-masker`, it will mask all the items in log data.

```xml
<attribute name="details" encode="false" layout="${event-properties-masker}" />
```

2. Specify `Item`, it will mask the specific value in log data by key set in `Item`. Note, it's case sensitive.

```xml
<attribute name="data1" encode="false" layout="${event-properties-masker:Item=data1}" />
```

3. Specify `Item` and `Mode` of `url`, `reserialize`, it will mask the specific value in log data by key set in `Item`. Note, it's case sensitive.

```xml
<attribute name="requestUrl" encode="false" layout="${event-properties-masker:Item=requestUrl:Mode=url}" />
```

```xml
<attribute name="requestUrl" encode="false" layout="${event-properties-masker:Item=requestBody:Mode=reserialize}" />
```

4. Disabled and render log without masking but use normal JSON serializer defined in NLog. Add `Disabled=true`.

   ```xml
   <attribute name="requestUrl" encode="false" layout="${event-properties-masker:Item=requestBody:Mode=reserialize:Disabled=true}" />
   ```

   

# Details

## Slin.Masking

Slin.Masking provides following fundamental classes:

* MaskFormatter. The fundamental masker base on format. 
* IMaskingOptions, IObjectMaskingOptions and IMaskingProfile. 
  * IMaskingProfile inherits `IMaskingOptions` and `IObjectMaskingOptions`
  * MaskingProfile implements `IMaskingProfile`
* IKeyedMasker and `KeyedMasker`
  * `KeyedMasker` support is the **most fundamental** implementation of masking.
* IMasker, IUrlMasker and `Masker`
  * `Masker` is built on top of `KeyedMasker`
* `IJsonMasker`,`IXmlMsker`, `IUrlMasker`
* IObjectMasker and ObjectMasker
  * `IObjectMasker` inherids `IJsonMasker`,`IXmlMsker`, `IUrlMasker`
  * `ObjectMasker` implement `IObjectMasker` and is built on top of `IMasker`
  * `IObjectMaskingOptions` take the responsible for ObjectMasker to high level configuration.
  * `IMaskingOptions` is  responsible for `Masker` and `KeyedMasker` with definitions of rules.



### MaskFormat

MaskFormat contains 3 parts and special cases.

#### Basic format of 3 parts

* char count to keep for left
* char count to keep for right
* char count for middle asterisk 

For example:

* `L2` : keep left 2 chars
* `R2` : keep right 2 chars
* `*`: mask all chars as '*'.
* `L4*`, same as `L4`
* `L4*4`: keep max 4 '*' after masking.
* `L4*R4`: Keep left 4 and right 4 chars.
* `L4*6R4`: keep left 4 and right 4 chars, and put 6 '*' at maximum in middle.
* `*6R4`

Note: the compose of these 3 parts are in order: Left , Middle and right. So such format is invalid: `R4*4`, `*` must be put before `R` and after `L`; or `*4L4`, `*` must be put after 'L4';

Another way, using '#' and '*', for example:

* `#*#`
* `##**`
* `##***###`: equals `L2*3R3`
* `*##`: equals`*1R2`
* `**##`

#### Special cases

* `null`: set the value to null
* `EMPTY`: set the value to empty
* `REDACTED`: mask the value to "REDACTED"
* `REPLACEMENT=`: mask the value by just replacement. For example:
  * `REPLACEMENT=`: same as `EMPTY`
  * `REPLACEMENT=REDACTED`: same as `REDACTED`
  * `REPLACEMENT=-----`: It will be replaced as "-----"
  * `REPLACEMENT=###`: It will be replaced as "###"



## Slin.Masking.NLog

Slin.Masking.NLog is built on Slin.Masking. It provides class `EventPropertiesMaskLayoutRenderer` with name `event-properties-masker`.

So you can easily integrate it with NLog, for more details, please refer to section Get Started.


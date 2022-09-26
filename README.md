# Slin.Masking

[TOC]

# Overview

Slin.Masking contains following two projects:

1. Slin.Masking 

`Slin.Masking` provides basic masking features.

* You can use `MaskFormatter` to do the masking by format you provided, like "*" for example.
* Or you can use `Masker` to do masking base on given key and value or URL. Masking rules are required, either by configuration file or programing way to get `IMaskingProfile` instance. 

If you just want to mask something without NLog, you can just this package.

2. Slin.Masking.NLog

Slin.Masking.NLog is built base on `Slin.Masking`. If you're using NLog to write JSON format log files, this is fit for you. `Slin.Masking.NLog` provides a couple of extension methods that allow developer easier to use masking along with NLog. It needs a bit changes in `NLog.config` file. 

If you don't use NLog, but you want masking, you can just refer to `Slin.Masking`.



# Get Started of Slin.Masking.NLog

In NetCore application (including Console or Web application). We need to do following steps:

## 1. Install package

Firstly, we need to install the package into your project, where NLog is initialized.

```powershell
dotnet add package Slin.Masking.NLog
```

## 2. Steup NLog with Masking

Masking works base on rules, which is defined in `IMaskingProfile` (inherits `IObjectMaskingOptions` and `IMaskingOptions`). `MaskingProfile` implemented `IMaskingProfile`. So we need provide MaskingProfile to make it working by default. 

We can do by following two ways if you want masking with NLog.

### 2.1 Programming way

Or you can use your another approach like below. BUT you need to provided all the rules by your self.

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
	.Setup(setupBuilder: (setupBuilder) => setupBuilder.UseMasking(profile)) //HERE is the magic
	.GetCurrentClassLogger();
```

### 2.1 Using configuration file (preferred)

I prefer to use file configuration for masking rules and options. You can use default file "masking.json" or specific your own configuration file. It will read masking options and rules from following files:

* `filename`.json : MUST. Organization level rules are suggested to be set in this file.
* `filename.custom`.json. OPTIONAL. Project specific level rules are suggested to be set in this file, which will overwrite to those set in `filename.json`.

By default, filename is "masking.json". If you use another name, like `maskrules.json`, it will automatically use the filename, like `maskrules.custom.json`.

```csharp
// Like in Program.cs file

var logger = LogManager
	.Setup(setupBuilder: (setupBuilder) => setupBuilder.UseMasking("masking.json")) //HERE is the magic
	.GetCurrentClassLogger();
```

**The next step is providing a masking file**. You can refer to next section. 

## Masking configuration file

As mentioned above, it will use two files, using `masking.json` and `masking.custom.json`(optinoal but suggested) by default. And please make sure these 

In the file, it contains:

* object masking options. This plays like a global settings when masking object. It does not including masking rules. It's affects `ObjectMasker`
* masking rules settings. This contains all the rules that used by `Masker`,`UrlMasker`,`KeyedMasker`.
  * NamedFormatterDefintions
  * Rules

## Masking options 

Masking options have default values, if you're not clear about what it does, let it be or remove them from json file, hence default values will be used. Here are the options (applied on `ObjectMasker`:

* Enabled
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
    "Enabled": true, //default: true, NOTE: it does not affect ObjectMasker, it affect global masking for layour render "event-properties-masker"
    "MaskUrlEnabled": true, //default: false, work with UrlKeys
    "MaskJsonSerializedEnabled": true, //default: true, work with SerializedKeys
    "MaskXmlSerializedEnabled": true, //default: true
    "MaskXmlSerializedOnXmlAttributeEnabled": false, //default false
    "MaskJsonSerializedOnXmlAttributeEnabled": false, //default false
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
    "UrlKeys": [ "requestUrl", "query" ],
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
    "NamedFormatters": {
      //NOTE: definition names here are case-insensitive.
      //For a given Name(unique and ignore case), the formatter contains: 
      //  valid Format, 
      //  optional ValuePattern and IgnoreCase for value pattern. ValuePattern can be regular expression(simple regex usage).
      //  default true of Enabled.
      //  Enabled: default true. BUT no need to set it to False here.
      "null": { "Format": "null" },
      "empty": { "Format": "EMPTY" },
      "redacted": { "Format": "REDACTED" },
      "credential_long": {
        "Format": "L9*4R6",
        "ValuePattern": ".{24,}"
      },
      "credential_short": {
        "Format": "L3*4R3",
        "ValuePattern": ".{10,24}"
      },
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
      "cvv": { "Format": "*" },
      "Remove": { "RemoveNode": true } //todo not supported yet, do we need it?
    },
    "Rules": {
      //NOTE: rule key by default is case-insensitive, so does name of formatter. 
      //It's controlled by property:IgnoreKeyCase, default is true
      "authorization": {
        "KeyName": "^authorization|access_token|accesstoken|code$",
        "IgnoreKeyCase": true,
        "Formatters": [
          { "Name": "credential_long" },
          { "Name": "credential_short" }
        ]
      },
      "SSN": { "Formatters": [ { "Name": "SSN" } ] },
      "DOB": { "Formatters": [ { "Name": "dob" } ] },
      "Pan": {
        "KeyName": "^pan|PersonalAccountNumber|PrimaryAccountNumber$",
        //IgnoreKeyCase is working with KeyName
        "IgnoreKeyCase": true,
        "Formatters": [ { "Name": "pan" } ]
      },
      "cvv": { "Formatters": [ { "Name": "cvv" } ] },
      "Balance": { "Formatters": [ { "Format": "null" } ] },
      "FirstName": { "Formatters": [ { "Name": "Name" } ] },
      "LastName": { "Formatters": [ { "Name": "Name" } ] },
      "Email": { "Formatters": [ { "Name": "email" } ] },
      "Password": { "Formatters": [ { "Format": "*" } ] },
      "PhoneNumber": { "Formatters": [ { "Name": "phone" } ] }
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
			  fileName="C:/logs/apimaksingsample/traffic_${shortdate:format:yyyyMMdd}.log"
			  archiveFileName="C:/logs/apimaksingsample/traffic_${shortdate:format:yyyyMMdd}.log.{#}" archiveAboveSize="80000000" archiveNumbering="Rolling" maxArchiveFiles="10"
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

2. Specify `Item`, it will mask the specific value in log data by key set in `Item`. Note, it's case sensitive before v0.1.30; after v0.1.30, it became case-insensitive.

    ```xml
    <attribute name="data1" encode="false" layout="${event-properties-masker:Item=data1}" />
    ```

3. Excludes specific properties by names, joint with `,`, this will be ignored if `Item` is set. Property names here is case-insensitive. Example:

    ```xml
    <attribute name="details" encode="false" layout="${event-properties-masker:excludeproperties=excludedx,excludedy:disabled=false}" />
    ```

    In above layout attribute, properties with name "excludedx" and "excluedy" will not get rendered. 

4. Specify `Item` and `Mode` of `url`, `reserialize`, it will mask the specific value in log data by key set in `Item`. Note, it's case sensitive.

    ```xml
    <attribute name="requestUrl" encode="false" layout="${event-properties-masker:Item=requestUrl:Mode=url}" />
    ```

    ```xml
    <attribute name="requestUrl" encode="false" layout="${event-properties-masker:Item=requestBody:Mode=reserialize}" />
    ```

5. Disabled and render log without masking but use normal JSON serializer defined in NLog. Add `Disabled=true`.

    ```xml
    <attribute name="requestUrl" encode="false" layout="${event-properties-masker:Item=requestBody:Mode=reserialize:Disabled=true}" />
    ```

   

# Details

## Slin.Masking

Slin.Masking provides fundamental masking interfaces and class like below:

* `MaskFormatter`. The fundamental masker base on format. 
* `IMaskingOptions` and IMaskingProfile. 
  * IMaskingProfile inherits `IMaskingOptions` and `IObjectMaskingOptions`
  * MaskingProfile implements `IMaskingProfile`
* `IKeyedMasker` and `KeyedMasker`
  * `KeyedMasker` support is the **most fundamental** implementation of masking.
* `IMasker`, I`UrlMasker` and `Masker`
  * `Masker` is built on top of `KeyedMasker`



### About MaskFormatter

You can directly use `MaskFormatter`, and here extension method also provided.

```
// example 1
var masked = "input-data-goes-here".Mask("L3*6R3");
//OR
var masked = "input-data-goes-here".Mask("{0:L3*6R3}");

// example 2
var masked = string.Format(MaskFormatter.Default, "{0:L3*6R3}", "input-data-goes-here");
```



### About KeyedMasker

You can directly use `Masker`, which inherits `IMasker`, it depends on `IMaskingOptions`, that contains the masking rules definitions.



You can use it like below:

```csharp
//DI part

			services.Configure<MaskingProfile>(configuration.GetSection("Masking"))
				.PostConfigure<MaskingProfile>((options) => options.Normalize());

			services.AddSingleton<IMaskingProfile>(provider =>
			{
				var profile = provider.GetRequiredService<IOptions<MaskingProfile>>()!.Value;
				return profile;
			});
			services.AddSingleton<IMaskingOptions>(provider =>
			{
				var profile = provider.GetRequiredService<IMaskingProfile>();
				return profile;
			});

			services.AddScoped<IMasker, Masker>();
			services.AddScoped<IUrlMasker, Masker>();
```



Usage of `Masker`:

```
public class SampleClass{
    private readonly IMasker _masker;
    public SampleClass(IMasker masker){
    	_masker = masker;
    }
    
    public string YouMethod(string input)
    {
         if(_masker.TryMask("ssn", "123456789", out var masked)){
              //.... expected "*********"
         }
         
         
         var maskedUrl = _masker.MaskUrl("https://gitbank.com/transfers/dob/1988-01-01/pan/5348123456789012?ssn=123456789", true));
         //expected result: https://gitbank.com/transfers/dob/REDACTED/pan/5348********9012?ssn=*********
    }
}
```



### About ObjectMasker

Fundamental interface for `IObjectMasker`.

```csharp
//IObjectMasker

	public interface IJsonMasker
	{
		string MaskJsonObjectString(JsonNode node);
	}

	public interface IXmlMasker
	{
		string MaskXmlElementString(XElement node);
	}

	public interface IObjectMasker : IJsonMasker, IXmlMasker, IUrlMasker
	{
		string MaskObject(object value);

		/// <summary>
		/// Indicates enabled or not. Just used as global setting. Not affecting ObjectMasker actually.
		/// </summary>
		bool Enabled { get; }
	}
```



Slin.Masking provides following fundamental classes:

* `IJsonMasker`,`IXmlMsker`, `IUrlMasker`
* `IObjectMasker` and `ObjectMasker`
  * `IObjectMasker` inherids `IJsonMasker`,`IXmlMsker`, `IUrlMasker` and depends on `IMasker` and `IObjectMaskingOptions`.
  * `ObjectMasker` implement `IObjectMasker` and is built on top of `IMasker`
  * `IObjectMaskingOptions` take the responsible for ObjectMasker to high level configuration.
  * `IMaskingOptions` is  responsible for `Masker` and `KeyedMasker` with definitions of rules.



Usage:

```

public class SampleClass{
    private readonly IMasker _masker;
    public SampleClass(IMasker masker){
    	_masker = masker;
    }
    
    public string YouMethod(string input)
    {
        var data = new List<string, object>{
           {"ssn","123456789"},
           {"requestUrl","https://gitbank.com/transfers/dob/1988-01-01/pan/5348123456789012?ssn=123456789"},
           {"userinfo": {"dob":"1988-01-01", "firstName":"shawn"}},
           
           //NOTE: to deserialize and mask it, you need make sure masking:MaskJsonSerializedEnabled set to true.
           {"ResponseBody":"{\"dob\":\"1988-01-01\",\"firstName\":\"shawn\"}"},
        };
        
        var outputString = _masker.MaskObject(data);
        return outputString;
    }
}
```

#### Advanced Usages

There are a few advanced usages, that it works with settings in `MaskProfile` (or from configuration file):

```
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
```

The settings above are powerful for some complex scenarios:

* MaskUrlEnabled works with `UrlKeys`, `UrlMaskingPatterns`
* Mask XML string (like WCF request/response, which is in SOAP, XML format)
  * MaskXmlSerializedEnabled
  * SerializedKeys
  * MaskXmlSerializedOnXmlAttributeEnabled  //NOTE: default is false
* Mask JSON string (like request/response in RESTful API)
  * MaskJsonSerializedEnabled
  * SerializedKeys
  * MaskJsonSerializedOnXmlAttributeEnabled
  * MaskJsonNumberEnabled : usually used to mask transaction amount or sensitive amount.
* MaskNestedKvpEnabled and `KeyKeyValueKeys`
  * it works on memory object
  * it also works on XML/JSON string if `MaskXmlSerializedEnabled` or `MaskJsonSerializedEnabled` is enabled.



# Mask Format

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

#### Email

If you want to mask email, the domain part will be kept, the mask format is similar as above, the only different is that the format must ends with a `@`. For example:

* `L3@` will get `Sha**@abc.com` for `Shawn@abc.com`
* `*@` will get `*****@abc.com` for `Shawn@abc.com`

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


## References
* https://learn.microsoft.com/en-us/nuget/hosting-packages/local-feeds

```
nuget init c:\work\mypackages \\myserver\packages
```
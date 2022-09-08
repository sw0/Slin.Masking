# Slin.Masking
# Get Started of Slin.Masking
You need to provide a `MaskingProfile`, which can be coded or from read from configuration file.

`MaskingProfile` contains 4 parts:
* ObjectMaskingOptions: this will be used for `ObjectMasker` when masking object.
* MaskingOptions: Options for masking, it is responsible for `IMasker`.
* UrlMaskingPatterns: will be used when `ObjectMaskingOptions.MaskUrlEnabled` is enabled and `MaskUrlEnabled.UrlKeys` is set.
* NamedFormatterDefintions: this is a group of named formatters, it like a shared definition. NOTE: it does not work unless you define the `Rules`.
* Rules: Rules must be provided. It contains one or more formatters. Formatters can be defined directly or refer to NamedFormatterDefintions by "Name".

Here is an example:
```
{
  "Masking": {
    "ObjectMaskingOptions": {
      "Enabled": true,  //if false, ObjectMasker will not work. But it does affect fundamental masker: Masker
      "MaskUrlEnabled": true, //default: false, work with UrlKeys
      "MaskJsonSerializedEnabled": true, //default: true, work with SerializedKeys
      "MaskXmlSerializedEnabled": false, //Not Implemented
      "MaskJsonNumberEnabled": true, //this works only in JSON
      "ValueMinLength": 3,
      //"SerializedKeysCaseSensitive": true,
      "UrlKeys": [ "requestUrl", "query" ],
      "SerializedKeys": [ "Body" ]
    },
    "MaskingOptions": {
      "PatternCheckChars": [ "?", "+", "*", "\\", "[", "|", "(", "^", ".", "$" ],
      "KeyCaseInsensitive": true,
      "ValueCaseInsensitive": false, //todo this is global setting, maybe we can control more precisely in ValuePattern.
      "ThrowIfNamedProfileNotFound": false
    },
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
      //NOTE: definition names here are case-insensitive
      "null": { "Format": "null" },
      "Empty": { "Format": "EMPTY" },
      "REDACTED": { "Format": "REDACTED" },
      "Remove": { "RemoveNode": true }, //todo not supported yet, do we need it?
      "PAN15": {
        "ValuePattern": "^\\d{15}$",
        "Format": "L6*R4"
      },
      "PAN16": { //just for sample here to use two different formatter base on length difference
        "ValuePattern": "^\\d{16}$",
        "Format": "L4*R4"
      },
      "DOB": {
        //"KeyName": "^DOB$|^DateOfBirth$",
        "Format": "REDACTED"
      },
      "Name": {
        "Format": "L2*"
      },
      "SSN": {
        "Format": "*"
      },
      "Email": {
        "Format": "L3*",
        "Description": "email is special, that it will be separated by '@' to two parts. use normal format for these two part separately"
      }
    },
    "Rules": {
      //NOTE: rule key by default is case-insensitive, so does name of formatter. 
      //It's controlled by setting:KeyCaseInsensitive
      "SSN": { "Formatters": [ { "Name": "SSN" } ] },
      "DOB": { "Formatters": [ { "Name": "dob" } ] },
      "Pan": {
        "KeyName": "^pan|PersonalAccountNumber|PrimaryAccountNumber$", //"^Pan|PAN$",
        "Formatters": [
          { "Name": "Pan15" },
          { "Name": "pan16" }
        ]
      },
      "Balance": { "Formatters": [ { "Format": "null" } ] },
      "FirstName": { "Formatters": [ { "Name": "Name" } ] },
      "LastName": { "Formatters": [ { "Name": "Name" } ] },
      "Email": { "Formatters": [ { "Name": "email" } ] },
      "Password": { "Formatters": [ { "Format": "*" } ] },
      "PhoneNumber": { "Formatters": [ { "Format": "L4" } ] },
      "temperatureC": { "Formatters": [ { "Format": "null" } ] },
      "temperaturef": { "Formatters": [ { "name": "null" } ] }
    }
  }
}
```


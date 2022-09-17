Thanks for using `Slin.Masking` or `Slin.Masking.NLog`.

After you installed the package, you'll see `masking.template.json` file.

Please copy the file and save it to `masking.json` or other name if you want. Suggested: `masking.json`.

Add also suggested creating another one `masking.custom.json` which put your custom rules for masking.

And don't forget to make `masking.json` and `masking.custom.json` file "CopyToOutputDirectory" enabled.



To use with NLog, just make a slight change in NLog initialization statements:

```
var logger = LogManager
	.Setup(setupBuilder: (setupBuilder) => setupBuilder.UseMasking("masking.json"))
	.GetCurrentClassLogger();
```

And in nlog.config, please use the renderer `event-properties-masker` to enable masking. 

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



For mode options, it has: `object`,`url`,`reserialize` and you can set another property `disabled` if you don't want masking enabled.



For more details, please refer to 

* [NuGet Gallery | Slin.Masking](https://www.nuget.org/packages/Slin.Masking)
* [NuGet Gallery | Slin.Masking.NLog](https://www.nuget.org/packages/Slin.Masking.NLog)
* https://github.com/sw0/slin.masking.


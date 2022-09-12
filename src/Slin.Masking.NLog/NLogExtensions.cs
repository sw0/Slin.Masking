﻿using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Slin.Masking.NLog
{
	public static class NLogExtensions
	{
		/// <summary>
		/// it will try use masking.json and masking.custom.json to create singleton masking profile.
		/// And register the IMaskingProfile, IMasker, IObjectMasker in the container.
		/// </summary>
		/// <param name="setupBuilder"></param>
		/// <param name="jsonFile"></param>
		public static void UseMasking(this ISetupBuilder setupBuilder, string jsonFile = "masking.json")
		{
			try
			{
				var fn = System.IO.Path.GetFileNameWithoutExtension(jsonFile);

				var configBuilder = new ConfigurationBuilder();
				configBuilder.AddJsonFile($"{fn}.json")
				.AddJsonFile($"{fn}.custom.json", true);
				var cfg = configBuilder.Build();

				var profile = cfg.GetSection("masking").Get<MaskingProfile>();
				profile.Normalize();

				var masker = new Masker(profile);

				setupBuilder.SetupExtensions(s =>
				   s.RegisterSingletonService<IMaskingProfile>(profile)
				   //.RegisterSingletonService<IMasker>(masker)
				   .RegisterSingletonService<IObjectMasker>(new ObjectMasker(masker, profile))
				   .RegisterLayoutRenderer<EventPropertiesMaskLayoutRenderer>("event-properties-masker")
				);
			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="setupBuilder"></param>
		/// <param name="profile"></param>
		public static void UseMasking(this ISetupBuilder setupBuilder, IMaskingProfile profile)
		{
			try
			{
				profile.Normalize();

				var masker = new Masker(profile);

				setupBuilder.SetupExtensions(s => s
				.RegisterSingletonService<IMaskingProfile>(profile)
				//.RegisterSingletonService<IMasker>(masker)
				.RegisterSingletonService<IObjectMasker>(new ObjectMasker(masker, profile))
				.RegisterLayoutRenderer<EventPropertiesMaskLayoutRenderer>("event-properties-masker")
				);
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}

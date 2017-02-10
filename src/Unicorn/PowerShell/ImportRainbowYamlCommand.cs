﻿using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using Kamsar.WebConsole;
using Rainbow.Model;
using Rainbow.Storage.Sc.Deserialization;
using Unicorn.Deserialization;
using Unicorn.Logging;

namespace Unicorn.PowerShell
{
	/// <summary>
	/// # ITEM DESERIALIZATION
	/// $yaml | Import-RainbowItem # Deserialize YAML from pipeline into Sitecore
	/// $yaml | Import-RainbowItem -Raw # Deserialize and disable all field filters
	/// $yamlStringArray | Import-RainbowItem # Deserialize multiple at once
	/// $yaml | ConvertFrom-RainbowYaml | Import-RainbowItem # Deserialize from IItemData
	/// </summary>
	[Cmdlet("Import", "RainbowItem")]
	public class ImportRainbowYamlCommand : YamlCommandBase
	{
		protected override void ProcessRecord()
		{
			if (Yaml == null && Items == null) throw new InvalidOperationException("Neither YAML strings or IItemDatas were passed in, cannot process.");

			var console = new PowershellProgressStatus(Host, "Deserialize Item");
			var consoleLogger = new WebConsoleLogger(console, MessageType.Debug);

			var yaml = CreateFormatter(CreateFieldFilter());

			var deserializer = new DefaultDeserializer(new DefaultDeserializerLogger(consoleLogger), CreateFieldFilter());

			if (Yaml != null)
			{
				foreach (var yamlItem in Yaml)
				{
					using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(yamlItem)))
					{
						var item = yaml.ReadSerializedItem(stream, "(from PowerShell)");

						consoleLogger.Info(item.Path);
						deserializer.Deserialize(item);
					}
				}
			}

			if (Items != null)
			{
				foreach (var item in Items)
				{
					consoleLogger.Info(item.Path);
					deserializer.Deserialize(item);
				}
			}
		}

		[Parameter(ValueFromPipeline = true)]
		public string[] Yaml { get; set; }

		[Parameter(ValueFromPipeline = true)]
		public IItemData[] Items { get; set; }
	}
}
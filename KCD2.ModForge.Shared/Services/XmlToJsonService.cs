﻿using KCD2.ModForge.Shared.Adapter;
using KCD2.ModForge.Shared.Models.Attributes;
using KCD2.ModForge.Shared.Models.ModItems;
using System.Diagnostics;

namespace KCD2.ModForge.Shared.Services
{
	public class XmlToJsonService
	{
		private readonly XmlAdapterOfT<Perk> perkAdapter;
		private readonly XmlAdapterOfT<Buff> buffAdapter;
		private readonly LocalizationAdapter localizationAdapter;
		private Dictionary<string, Dictionary<string, string>> localizationCache;
		private readonly JsonAdapterOfT<Perk> jsonPerkAdapter;
		private readonly JsonAdapterOfT<Buff> jsonBuffAdapter;
		private readonly UserConfigurationService userConfigurationService;

		// TODO: Weitere Abstraktion einfügen. IModItemSource(?)
		public XmlToJsonService(XmlAdapterOfT<Perk> perkAdapter, XmlAdapterOfT<Buff> buffAdapter, LocalizationAdapter localizationAdapter, JsonAdapterOfT<Perk> jsonPerkAdapter, JsonAdapterOfT<Buff> jsonBuffAdapter, UserConfigurationService userConfigurationService)
		{
			this.perkAdapter = perkAdapter;
			this.buffAdapter = buffAdapter;
			this.localizationAdapter = localizationAdapter;
			this.jsonPerkAdapter = jsonPerkAdapter;
			this.jsonBuffAdapter = jsonBuffAdapter;
			this.userConfigurationService = userConfigurationService;
		}

		public IList<Perk> Perks { get; private set; }
		public IList<Buff> Buffs { get; private set; }
		public IList<BuffParam> BuffParams { get; private set; }

		private async Task InitializeExport()
		{
			Perks = await perkAdapter.ReadModItems("");
			Buffs = await buffAdapter.ReadModItems("");
			localizationCache = localizationAdapter.LoadAllLocalizationsFromPaks();
		}

		private async Task AssignLocalizations()
		{
			await AssignPerkLocalizations();
			await AssignBuffLocalizations();
			return;
		}

		public async Task ConvertXmlToJsonAsync()
		{
			var watch = Stopwatch.StartNew();
			await InitializeExport();
			await AssignLocalizations();
			await jsonPerkAdapter.WriteElements(Perks);
			await jsonBuffAdapter.WriteElements(Buffs);
			watch.Stop();
		}

		public async Task<bool> TryReadJsonFiles()
		{
			var perkPath = Path.Combine(
							Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
							"ModForge", $"perks.json");
			var buffPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"ModForge", $"buffs.json");

			if (File.Exists(perkPath) == false)
			{
				return false;
			}

			if (File.Exists(buffPath) == false)
			{
				return false;
			}

			var perks = await ReadPerkJsonFile(perkPath);
			var buffs = await ReadBuffJsonFile(buffPath);
			Perks = perks.ToList();
			Buffs = buffs.ToList();
			return true;
		}

		private async Task<IEnumerable<Perk>> ReadPerkJsonFile(string filePath)
		{
			return await jsonPerkAdapter.ReadAsync(filePath);
		}

		private async Task<IEnumerable<Buff>> ReadBuffJsonFile(string filePath)
		{
			return await jsonBuffAdapter.ReadAsync(filePath);
		}

		private Task AssignPerkLocalizations()
		{

			foreach (var perk in Perks)
			{
				foreach (var language in LocalizationAdapter.LanguageMap.Values)
				{
					if (localizationCache.TryGetValue(language, out var langDict))
					{
						var descKey = GetAttributeValue(perk.Attributes, "perk_ui_desc");
						var loreDescKey = GetAttributeValue(perk.Attributes, "perk_ui_lore_desc");
						var nameKey = GetAttributeValue(perk.Attributes, "perk_ui_name");

						if (descKey != null && langDict.TryGetValue(descKey, out var desc))
						{
							perk.Localization.Descriptions[language] = new Dictionary<string, string> { [descKey] = desc };
						}

						if (loreDescKey != null && langDict.TryGetValue(loreDescKey, out var loreDesc))
						{
							perk.Localization.LoreDescriptions[language] = new Dictionary<string, string> { [loreDescKey] = loreDesc };
						}

						if (nameKey != null && langDict.TryGetValue(nameKey, out var name))
						{
							perk.Localization.Names[language] = new Dictionary<string, string> { [nameKey] = name };
						}
					}
				}
			}
			return Task.CompletedTask;
		}

		private Task AssignBuffLocalizations()
		{
			foreach (var buff in Buffs)
			{
				foreach (var language in LocalizationAdapter.LanguageMap.Values)
				{
					if (localizationCache.TryGetValue(language, out var langDict))
					{
						var descKey = GetAttributeValue(buff.Attributes, "buff_desc");
						var loreDescKey = GetAttributeValue(buff.Attributes, "slot_buff_ui_name");
						var uiNameKey = GetAttributeValue(buff.Attributes, "buff_ui_name");

						if (descKey != null && langDict.TryGetValue(descKey, out var desc))
						{
							buff.Localization.Descriptions[language] = new Dictionary<string, string> { [descKey] = desc };
						}

						if (loreDescKey != null && langDict.TryGetValue(loreDescKey, out var loreDesc))
						{
							buff.Localization.LoreDescriptions[language] = new Dictionary<string, string> { [loreDescKey] = loreDesc };
						}

						if (uiNameKey != null && langDict.TryGetValue(uiNameKey, out var name))
						{
							buff.Localization.Names[language] = new Dictionary<string, string> { [uiNameKey] = name };
						}
					}
				}
			}
			return Task.CompletedTask;
		}

		private string? GetAttributeValue(IEnumerable<IAttribute> attributes, params string[] names)
		{
			foreach (var name in names)
			{
				var attr = attributes.FirstOrDefault(a => a.Name == name);
				if (attr != null)
				{
					return attr.Value?.ToString();
				}
			}
			return null;
		}
	}
}

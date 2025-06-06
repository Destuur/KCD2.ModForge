﻿using KCD2.ModForge.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KCD2.ModForge.UI.Layout
{
	public partial class MainLayout
	{
		private bool isLoading;
		private bool _isDarkMode;
		private MudThemeProvider? _mudThemeProvider;
		bool _drawerOpen = false;
		private bool _isLoaded = false;

		[Inject]
		public XmlToJsonService? XmlToJsonService { get; set; }
		[Inject]
		public NavigationService? NavigationService { get; set; }
		//[Inject]
		//public ModService ModService { get; set; }
		public string ModName { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				_isLoaded = true;
				_isDarkMode = await _mudThemeProvider!.GetSystemPreference();
				StateHasChanged();
			}
		}

		void DrawerToggle()
		{
			_drawerOpen = !_drawerOpen;
		}

		public void ToggleDarkMode()
		{
			_isDarkMode = !_isDarkMode;
		}

		public async Task ImportGameData()
		{
			isLoading = true;
			if (XmlToJsonService is null)
			{
				return;
			}
			try
			{
				await Task.Run(async () =>
				{
					await XmlToJsonService.ConvertXmlToJsonAsync();
				});
			}
			finally
			{
				isLoading = false;
			}
		}

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			if (XmlToJsonService is null)
			{
				return;
			}
			await XmlToJsonService.TryReadJsonFiles();
		}
	}
}

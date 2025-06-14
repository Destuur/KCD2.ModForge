﻿using KCD2.ModForge.Shared.Services;
using KCD2.ModForge.UI.Components.DialogComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace KCD2.ModForge.UI.Pages
{
	public partial class ModItems
	{
		private int selectedTab = 0;

		[Inject]
		public ModService? ModService { get; private set; }
		[Parameter]
		public string? ModId { get; set; }
		[Inject]
		public NavigationManager NavigationManager { get; set; }
		[Inject]
		public IDialogService DialogService { get; set; }
		[Inject]
		public ISnackbar Snackbar { get; set; }


		private async Task Checkout()
		{
			var parameters = new DialogParameters<MoreModItemsDialog>
			{
				{ x => x.ContentText, "Hast thou pulled enough pizzles? Depart then, and shape thy mod!" },
				{ x => x.ButtonText, "Create Mod" }
			};

			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

			var dialog = await DialogService.ShowAsync<MoreModItemsDialog>("Create Your Mod", parameters, options);
			var result = await dialog.Result;

			if (result.Canceled == false)
			{
				NavigationManager.NavigateTo("/modoverview");
			}
		}

		public void SaveMod()
		{
			ModService.WriteModCollectionAsJson();
			Snackbar.Add(
				"Mod successfully saved",
				Severity.Success,
				config =>
				{
					config.DuplicatesBehavior = SnackbarDuplicatesBehavior.Prevent;
				});
			ModService.ClearCurrentMod();
			NavigationManager.NavigateTo("/");
		}

		private async Task Cancel()
		{
			var parameters = new DialogParameters<ChangesDetectedDialog>
			{
				{ x => x.ContentText, "Are you sure you want to cancel the modding?" },
				{ x => x.ButtonText, "I am sure" }
			};

			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

			var dialog = await DialogService.ShowAsync<ChangesDetectedDialog>("Cancel Modding", parameters, options);
			var result = await dialog.Result;

			if (result.Canceled == false)
			{
				ModService.ClearCurrentMod();
				NavigationManager.NavigateTo($"/");
			}
		}

		protected override async Task OnInitializedAsync()
		{
			if (ModService is null ||
				string.IsNullOrEmpty(ModId))
			{
				return;
			}
			ModService.TryGetModFromCollection(ModId);
			await base.OnInitializedAsync();
		}
	}
}

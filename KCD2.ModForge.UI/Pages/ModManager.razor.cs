﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCD2.ModForge.UI.Pages
{
	public partial class ModManager
	{
		[Inject]
		public NavigationManager Navigation { get; set; }
	}
}

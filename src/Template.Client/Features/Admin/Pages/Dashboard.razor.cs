using Microsoft.AspNetCore.Components;

using MudBlazor;

using Template.Client.Features.Admin.Models;
using Template.Client.Features.Admin.State;
using Template.Client.State;

namespace Template.Client.Features.Admin.Pages;

public partial class Dashboard
{
	private readonly string[] xAxisLabels = { "8am", "9am", "10am", "11am", "12pm", "1pm", "2pm", "3pm", "4pm" };

	private readonly List<ActivityItemModel> activities =
	[
		new() { Time = "1 min ago", User = "admin", Message = "updated Password Policy", Type = "Info" },
		new() { Time = "5 mins ago", User = "jdoe", Message = "failed login (3 attempts)", Type = "Warning" },
		new() { Time = "12 mins ago", User = "system", Message = "Daily backup completed", Type = "Success" },
		new() { Time = "25 mins ago", User = "alice", Message = "logged out", Type = "Info" },
		new() { Time = "1 hour ago", User = "bob", Message = "account locked", Type = "Error" },
	];

	// Chart Data
	private readonly List<ChartSeries> series =
	[
		new ChartSeries() { Name = "Active Sessions", Data = [40, 20, 25, 27, 46, 60, 48, 80, 15] },
	];

	[Inject]
	private Store Store { get; set; } = null!;

	private AdminState _state = new();

	protected override void OnInitialized()
	{
		_state = Store.GetState<AdminState>() ?? new AdminState();
		Store.Subscribe<AdminState>(UpdateState);

		Store.Dispatch(new LoadDashboardAction());

		Task.Run(async () =>
		{
			await Task.Delay(500);
			Store.Dispatch(new DashboardLoadedAction(1250, 84, 3, 12));
		});
	}

	private static Color GetActivityColor(string type) => type switch
	{
		"Info" => Color.Info,
		"Success" => Color.Success,
		"Warning" => Color.Warning,
		"Error" => Color.Error,
		_ => Color.Default,
	};

	private void UpdateState(AdminState newState)
	{
		_state = newState;
		InvokeAsync(StateHasChanged);
	}
}

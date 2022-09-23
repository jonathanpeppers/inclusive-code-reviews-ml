namespace MLTrainer;

public partial class AppShell : Shell
{
	bool _isDestroying;
	ViewModels.ClassificationViewModel _classificationViewModel;
	public AppShell(ViewModels.ClassificationViewModel classificationViewModel)
	{
		InitializeComponent();
		BindingContext = _classificationViewModel = classificationViewModel;
	}

	protected override void OnNavigated(ShellNavigatedEventArgs args)
	{
		base.OnNavigated(args);
#if WINDOWS
		Window.Activated += AppShellActivated;
		Window.Deactivated += AppShellDeactivated;
		Window.Destroying += WindowDestroying;
	}

	void WindowDestroying(object? sender, EventArgs e)
	{
		_isDestroying = true;
		UnsubscribeKeyDown();
	}

	void AppShellDeactivated(object? sender, EventArgs e)
	{
		UnsubscribeKeyDown();
	}

	void AppShellActivated(object? sender, EventArgs e)
	{
		MauiWinUIWindow? win = Window.Handler.PlatformView as MauiWinUIWindow;
		if (win?.Content != null)
			win.Content.KeyDown += ContentKeyDown;
	}

	void ContentKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		if (e.Key == Windows.System.VirtualKey.F1)
		{
			_classificationViewModel.BadCommentCommand.Execute(null);
		}
		else if (e.Key == Windows.System.VirtualKey.F2)
		{
			_classificationViewModel.GoodCommentCommand.Execute(null);
		}
		else if (e.Key == Windows.System.VirtualKey.Escape)
		{
			_classificationViewModel.SkipCommentCommand.Execute(null);
		}
	}

	void UnsubscribeKeyDown()
	{
		if (_isDestroying)
			return;

		MauiWinUIWindow? win = Window.Handler.PlatformView as MauiWinUIWindow;
		if (win?.Content != null)
			win.Content.KeyDown -= ContentKeyDown;
	}
#else
	}
#endif
}

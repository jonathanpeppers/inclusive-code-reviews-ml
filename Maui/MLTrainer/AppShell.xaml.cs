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
		if (e.Key == Windows.System.VirtualKey.Left)
		{
			_classificationViewModel.BadCommentCommand.Execute(null);
		}
		if (e.Key == Windows.System.VirtualKey.Right)
		{
			_classificationViewModel.GoodCommentCommand.Execute(null);
		}
		if (e.Key == Windows.System.VirtualKey.Space)
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

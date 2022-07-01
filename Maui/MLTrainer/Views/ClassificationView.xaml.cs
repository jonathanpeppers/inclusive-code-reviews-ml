namespace MLTrainer.Views;

public partial class ClassificationView : ContentView
{
	public ClassificationView()
	{
		InitializeComponent();

		BindingContext = App.Current?.Handler?.MauiContext?.Services.GetService<ViewModels.ClassificationViewModel>();
	}
}
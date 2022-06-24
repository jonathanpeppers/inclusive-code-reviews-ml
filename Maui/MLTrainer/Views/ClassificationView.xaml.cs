using MLTrainer.ViewModels;

namespace MLTrainer.Views;

public partial class ClassificationView : ContentView
{
    public ClassificationView()
    {
        InitializeComponent();

        BindingContext = App.Current.Handler.MauiContext.Services.GetService<ClassificationViewModel>();     
    }
}
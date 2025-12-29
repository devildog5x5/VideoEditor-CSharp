using System.Windows;
using VideoEditor.ViewModels;

namespace VideoEditor
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
            
            // Connect timeline selection to properties
            ViewModel.TimelineViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModels.TimelineViewModel.SelectedClip))
                {
                    ViewModel.PropertiesViewModel.SetClip(ViewModel.TimelineViewModel.SelectedClip);
                }
            };
        }
    }
}


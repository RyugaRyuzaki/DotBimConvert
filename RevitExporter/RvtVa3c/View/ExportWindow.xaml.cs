using RvtVa3c.ViewModel;


namespace RvtVa3c
{
    public partial class ExportWindow
    {
        public ExportWindow(ExportViewModel ExportViewModel)
        {
            InitializeComponent();
            DataContext = ExportViewModel;
        }
    }
}

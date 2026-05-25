using System.Windows.Controls;

namespace BagsOn.Views
{
    public partial class EmptyPageView : UserControl
    {
        public EmptyPageView(string title, string description)
        {
            InitializeComponent();

            TitleTextBlock.Text = title;
            DescriptionTextBlock.Text = description;
        }
    }
}
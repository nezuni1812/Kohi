using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kohi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IncomeExpensePage : Page
    {
        public ThuViewModel ThuVM { get; set; } = new ThuViewModel();
        public ChiViewModel ChiVM { get; set; } = new ChiViewModel();

        public IncomeExpensePage()
        {
            this.InitializeComponent();
            GridContent.DataContext = ThuVM;
        }

        private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem == IncomeSelectorBar)
            {
                ReceiptCategory.Text = "Loại phiếu thu";
                Amount.Text = "Số tiền thu";
                addButtonTextBlock.Text = "Thêm phiếu thu";
                GridContent.DataContext = ThuVM;
            }
            else if (sender.SelectedItem == ExpenseSelectorBar)
            {
                ReceiptCategory.Text = "Loại phiếu chi";
                Amount.Text = "Số tiền chi";
                addButtonTextBlock.Text = "Thêm phiếu chi";
                GridContent.DataContext = ChiVM;
            }
        }

        private void addButton_click(object sender, RoutedEventArgs e)
        {
        }
    }

    public class ThuViewModel
    {
    }
    public class ChiViewModel
    {
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace REX.AC_Extension.Resources.Dialogs
{
    /// <summary>
    /// Interaction logic for SubControl.xaml
    /// </summary>
    public partial class SubControl : REX.Common.REXExtensionControl
    {
        public SubControl()
        {
            InitializeComponent();
        }
        public SubControl(REX.Common.REXExtension extension)
            : base(extension)
        {
            InitializeComponent();
        }
    }
}

﻿namespace Gui
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel
                {
                    View = this
                };
        }
    }
}

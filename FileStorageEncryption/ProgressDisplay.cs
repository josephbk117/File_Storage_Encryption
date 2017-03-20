using System.Windows;
using System.Windows.Controls;

namespace FileStorageEncryption
{
    class ProgressDisplay
    {
        private string fileName;
        private ProgressBar progressbar;

        public ProgressDisplay(DockPanel parent, string fileName)
        {
            this.fileName = fileName;
            DockPanel pan = new DockPanel();
            DockPanel.SetDock(pan, Dock.Top);
            Label lbl1 = new Label()
            {
                Content = fileName,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            DockPanel.SetDock(lbl1, Dock.Left);
            pan.Children.Add(lbl1);
            progressbar = new ProgressBar()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 300
            };
            DockPanel.SetDock(progressbar, Dock.Right);
            pan.Children.Add(progressbar);
            parent.Children.Add(pan);
        }

        public void SetProgress(int progressPercentage)
        {
            progressbar.Value = progressPercentage;
        }
    }
}

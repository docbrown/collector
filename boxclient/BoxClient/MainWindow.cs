using System.Windows.Forms;

namespace BoxClient;

public class MainWindow : Form
{
    protected StatusStrip MainStatusStrip { get; private set; }
    protected SplitContainer MainSplitContainer { get; private set; }
    protected TreeView FolderTreeView { get; private set; }

    public MainWindow()
    {
        MainSplitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel1
        };
        Controls.Add(MainSplitContainer);

        FolderTreeView = new TreeView
        {
            Dock = DockStyle.Fill
        };
        MainSplitContainer.Panel1.Controls.Add(FolderTreeView);

        MainMenuStrip = new MenuStrip();
        Controls.Add(MainMenuStrip);

        MainStatusStrip = new StatusStrip();
        Controls.Add(MainStatusStrip);
    }
}

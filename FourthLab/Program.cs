using Gdk;
using Gtk;

class MyWindow : Gtk.Window
{
    private Box mainBox;
    public MyWindow() : base("Fourth lab")
    {
        SetDefaultSize(800, 600);
        SetPosition(WindowPosition.Center);

        MenuBar mb = new MenuBar();

        MenuItem file = new MenuItem("File");
        MenuItem view = new MenuItem("View");

        Menu filemenu = new Menu();
        Menu viewmenu = new Menu();

        file.Submenu = filemenu;
        view.Submenu = viewmenu;

        MenuItem[] fileItems =
        {
            new MenuItem("New"),
            new MenuItem("Open"),
            new MenuItem("Fast"),
            new SeparatorMenuItem(),
            new MenuItem("Exit")
        };

        foreach (MenuItem menuItem in fileItems)
        {
            filemenu.Append(menuItem);
        }

        MenuItem[] viewItems =
        {
            new MenuItem("Vertical Box"),
            new MenuItem("Horizontal Box"),
            new MenuItem("Horizontal Panel"),
            new MenuItem("Grid"),
            new MenuItem("Frame")

        };

        viewItems[0].Activated += CreateVerticalBox;
        viewItems[1].Activated += CreateHorizontalBox;
        viewItems[2].Activated += CreateHorizontalPane;
        viewItems[3].Activated += CreateGrid;
        viewItems[4].Activated += CreateFrame;


        foreach (MenuItem menuItem in viewItems)
        {
            viewmenu.Append(menuItem);
        }

        mb.Append(file);
        mb.Append(view);

        mainBox = new Box(Orientation.Vertical, 5);
        mainBox.PackStart(mb, false, false, 0);

        Add(mainBox);

    }

    Widget[] InitWidgets() 
    {
        return new Widget[] {
            new Label("First label"),
            new Label("Another label"),
            new Button("Click me"),
            new ToggleButton("Push me")
        };
    }

    protected override bool OnDeleteEvent(Event evnt)
    {
        Application.Quit();
        return base.OnDeleteEvent(evnt);
    }

    public void CreateVerticalBox(object? sender, EventArgs e)
    {
        ClearContent();

        Box box = new Box(Orientation.Vertical, 5);
        foreach (Widget widget in InitWidgets())
        {
            box.PackStart(widget, false, false, 5);
        }

        mainBox.PackStart(box, false, false, 5);
        ShowAll();
    }

    public void CreateHorizontalBox(object? sender, EventArgs e)
    {
        ClearContent();

        Box box = new Box(Orientation.Horizontal, 5);
        foreach (Widget widget in InitWidgets())
        {
            box.PackStart(widget, false, false, 5);
        }

        mainBox.PackStart(box, false, false, 5);
        ShowAll();
    }

    public void CreateHorizontalPane(object? sender, EventArgs e)
    {
        ClearContent();

        Widget[] widgets = InitWidgets();
 
        Paned paned = new Paned(Orientation.Horizontal);
        paned.Pack1(widgets[0], true, true);
        paned.Pack2(widgets[widgets.Length - 1], true, true);

        mainBox.PackStart(paned, false, false, 5);
        ShowAll();
    }

    public void CreateGrid(object? sender, EventArgs e) 
    {
        ClearContent();

        Widget[] widgets = InitWidgets();
        Grid grid = new Grid
        {
            RowSpacing = 10,
            ColumnSpacing = 10
        };

        for (int i = 0; i < widgets.Length; i++)
        {
            int row = i / 2;
            int col = i % 2;
            grid.Attach(widgets[i], col, row, 1, 1);
        }

        mainBox.PackStart(grid, true, true, 5);
        ShowAll();
    }
    
    public void CreateFrame(object? sender, EventArgs e)
    {
        Frame frame = new Frame("Framed Layout");

        if (mainBox.Children.Length == 1)
        {
            Box emptyBox = new Box(Orientation.Vertical, 5);
            frame.Add(emptyBox);
        }
        else
        {
            Widget content = mainBox.Children[1];
            mainBox.Remove(content);
            frame.Add(content);
        }

        mainBox.PackStart(frame, false, false, 5);

        ShowAll();
    }


    void ClearContent()
    {
        foreach (Widget child in mainBox.Children)
        {
            if (!(child is MenuBar))
            {
                mainBox.Remove(child);
                child.Destroy();
            }
        }
    }

}

class MainWindow
{
    static void Main()
    {
        Application.Init();
        MyWindow myWindow = new MyWindow();
        myWindow.ShowAll();
        Application.Run();
    }
}

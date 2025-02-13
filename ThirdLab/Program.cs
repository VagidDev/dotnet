using System.ComponentModel;
using Gdk;
using Gtk;
class MyWindow : Gtk.Window
{
    private Label label;
    public MyWindow() : base("MyFirstApp")
    {
        SetDefaultSize(300, 300);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Application.Quit(); };

        Toolbar tb = new Toolbar();
        tb.ToolbarStyle = ToolbarStyle.Icons;

        ToolButton newtb = new ToolButton(Stock.New);
        ToolButton opentb = new ToolButton(Stock.Open);
        ToolButton savetb = new ToolButton(Stock.Save);
        ToolButton undotb = new ToolButton(Stock.Undo);
        ToolButton redotb = new ToolButton(Stock.Redo);
        SeparatorToolItem septb = new SeparatorToolItem();
        ToolButton quittb = new ToolButton(Stock.Quit);

        tb.Insert(undotb, 0);
        tb.Insert(redotb, 1);
        tb.Insert(newtb, 2);
        tb.Insert(opentb, 3);
        tb.Insert(savetb, 4);
        tb.Insert(septb, 5);
        tb.Insert(quittb, 6);

        quittb.Clicked += OnClicked;
        opentb.Clicked += OpenWindow;
        Box vbox = new Box(Orientation.Vertical, 2);
        vbox.PackStart(tb, false, false, 0);

        Add(vbox);
    }

    void OnClicked(object sender, EventArgs args)
    {
        Application.Quit();
    }

    void OpenWindow(object sender, EventArgs args) 
    {
        OpenWindow openWindow = new OpenWindow();
        openWindow.ShowAll();
    }

}

class OpenWindow : Gtk.Window
{
    public OpenWindow() : base("Open window")
    {
        SetDefaultSize(300, 300);
        SetPosition(WindowPosition.Center);

        Label label = new Label("Opened window!");

        Add(label);
    }

    protected override bool OnDeleteEvent(Event evnt)
    {
        Dispose();
        return true;
    }
}
class Hello
{
    static void Main()
    {
        Application.Init();
        MyWindow w = new MyWindow();
        w.ShowAll();
        Application.Run();
    }
}
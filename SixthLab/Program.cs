using Cairo;
using Gdk;
using Gtk;
using Color = Cairo.Color;

class Area : DrawingArea 
{
    public Color color = new Color(1, 0, 0);
    public Area() 
    {
        SetSizeRequest(0, 100);
    }
    public void setColor(Color c) 
    {
        color = c;
        QueueDraw();
    }
    protected override bool OnDrawn(Context c) 
    {
        c.SetSourceColor(color);
        c.Paint();
        return true;
    }
}

class Program 
{
    private static Area area = new Area();
    static void Main() 
    {
        Application.Init();

        var builder = new Builder();
        builder.AddFromFile("interface.glade");
        var window = (Gtk.Window)builder.GetObject("MyWindow");
        
        RadioButton r = (RadioButton)builder.GetObject("r");
        RadioButton g = (RadioButton)builder.GetObject("g");
        RadioButton b = (RadioButton)builder.GetObject("b");
        RadioButton p = (RadioButton)builder.GetObject("p");
        RadioButton y = (RadioButton)builder.GetObject("y");
        RadioButton w = (RadioButton)builder.GetObject("w");
        RadioButton black = (RadioButton)builder.GetObject("black");
        
        var vbox = (Box)builder.GetObject("vbox");
        var xmlArea = (DrawingArea)builder.GetObject("area");
        vbox.Remove(xmlArea);
        vbox.Add(area);
        
        r.Clicked += on_red_clicked;
        g.Clicked += on_green_clicked;
        b.Clicked += on_blue_clicked;
        p.Clicked += on_purple_clicked;
        y.Clicked += on_yellow_clicked;
        w.Clicked += on_white_clicked;
        black.Clicked += on_black_clicked;
        
        window.DeleteEvent += (o, args) => Application.Quit();
        window.ShowAll();
        Application.Run();
    }

    static void on_red_clicked(object? sender, EventArgs e) => area.setColor(new Color(1, 0, 0));
    static void on_green_clicked(object? sender, EventArgs e) => area.setColor(new Color(0, 1, 0));
    static void on_blue_clicked(object? sender, EventArgs e) => area.setColor(new Color(0, 0, 1));
    static void on_purple_clicked(object? sender, EventArgs e) => area.setColor(new Color(1, 0, 1));
    static void on_yellow_clicked(object? sender, EventArgs e) => area.setColor(new Color(1, 1, 0));
    static void on_white_clicked(object? sender, EventArgs e) => area.setColor(new Color(1, 1, 1));
    static void on_black_clicked(object? sender, EventArgs e) => area.setColor(new Color(0, 0, 0));
}
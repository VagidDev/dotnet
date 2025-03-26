using Backend;
using Cairo;
using Gdk;
using Gtk;
using System;
using Color = Cairo.Color;

namespace Frontend
{

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

    public class FrontendApp
    {
        private BackendService _backendService;
        private Area area = new Area();
        public FrontendApp()
        {
            _backendService = new BackendService();
            Application.Init();
            
        }

        private void Initialize() {
            Gtk.Window window = new Gtk.Window("Colors");
            window.DeleteEvent += delegate { Application.Quit(); };
            window.SetDefaultSize(400, 300);
            Box hbox = new Box(Gtk.Orientation.Horizontal, 5);
            RadioButton r = new RadioButton("red");
            RadioButton g = new RadioButton(r, "green");
            RadioButton b = new RadioButton(r, "blue");
            RadioButton p = new RadioButton(r, "purple");
            RadioButton y = new RadioButton(r, "yellow");
            RadioButton w = new RadioButton(r, "white");
            RadioButton black = new RadioButton(r, "black");

            r.Clicked += on_red_clicked;
            g.Clicked += on_green_clicked;
            b.Clicked += on_blue_clicked;
            p.Clicked += on_purple_clicked;
            y.Clicked += on_yellow_clicked;
            w.Clicked += on_white_clicked;
            black.Clicked += on_black_clicked;

            hbox.Add(r);
            hbox.Add(g);
            hbox.Add(b);
            hbox.Add(p);
            hbox.Add(y);
            hbox.Add(w);
            hbox.Add(black);
            hbox.Margin = 5;
            Box vbox = new Box(Gtk.Orientation.Vertical, 5);
            vbox.Add(hbox);
            vbox.Add(area);
            window.Add(vbox);
            window.ShowAll();
            Application.Run();
        }
        void on_red_clicked(object? sender, EventArgs e)
        {
            area.setColor(_backendService.getRed());
        }
        void on_green_clicked(object? sender, EventArgs e)
        {
            area.setColor(_backendService.getGreen());
        }
        void on_blue_clicked(object? sender, EventArgs e)
        {
            area.setColor(_backendService.getBlue());
        }

        void on_purple_clicked(object? sender, EventArgs e)
        {
            area.setColor(_backendService.getPurple());
        }

        void on_yellow_clicked(object? sender, EventArgs e)
        {
            area.setColor(_backendService.getYellow());
        }

        void on_white_clicked(object? sender, EventArgs e)
        {
            area.setColor(_backendService.getWhite());
        }

        void on_black_clicked(object? sender, EventArgs e)
        {
            area.setColor(_backendService.getBlack());
        }

    }

}
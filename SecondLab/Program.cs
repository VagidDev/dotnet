using Gdk;
using Gtk;

class MyWindow : Gtk.Window {
	public MyWindow() : base("MyFirstApp") {
		MenuBar mb = new MenuBar();
		
		MenuItem file = new MenuItem("File");
		MenuItem view = new MenuItem("View");
		
		Menu filemenu = new Menu();
		Menu viewmenu = new Menu();
		
		file.Submenu = filemenu;
		view.Submenu = viewmenu;
		
		MenuItem[] fileItems = {new MenuItem("New"), new MenuItem("Open"), new MenuItem("Fast"), new SeparatorMenuItem(), new MenuItem("Exit")};
		
		foreach (MenuItem menuItem in fileItems) {
			filemenu.Append(menuItem);
		}
		
		MenuItem statitem = new MenuItem("Stat");
		viewmenu.Append(statitem);

		MenuItem aboutItem = new MenuItem("About");
		viewmenu.Append(aboutItem);

		CheckMenuItem checkMenuItem = new CheckMenuItem("Status Bar");
		checkMenuItem.Active = true;
		viewmenu.Append(checkMenuItem);

		mb.Append(file);
		mb.Append(view);

		Box hbox = new Box(Orientation.Horizontal, 2);
		hbox.PackStart(mb, false, false, 0);
		
		Box buttonBox = new Box(Orientation.Vertical, 5);
		
		Button mybutton1 = new Button("PushMe");
		mybutton1.TooltipText = "Just Push Me";
		buttonBox.Add(mybutton1);
		
		ToggleButton mybutton2 = new ToggleButton("StateMe");
		mybutton2.TooltipText = "Open Function Window";
		buttonBox.Add(mybutton2);

		CheckButton mybutton3 = new CheckButton("OneOfStateApp");
		mybutton3.TooltipText = "Activate Sets";
		buttonBox.Add(mybutton3);
		//не нашел я ваш статусбар
		ProgressBar statusbar = new ProgressBar();
		statusbar.Fraction = 0.5;
		buttonBox.Add(statusbar);

		Box mainFrame = new Box(Orientation.Vertical, 5);
		mainFrame.PackStart(hbox, false, false, 5);
		mainFrame.PackStart(buttonBox, false, false, 5);

		Add(mainFrame);
		
		aboutItem.Activated += (sender, e) => 
		{
			AuthorWindow authorWindow = new AuthorWindow();
			authorWindow.ShowAll();
		};

		checkMenuItem.Activated += (sender, o) => 
		{
			if (checkMenuItem.Active == true)
				statusbar.Visible = true;
			else
				statusbar.Visible = false;	
		};

	}



	protected override bool OnDeleteEvent(Event e) { 
		Application.Quit(); return true;
	}
}

class AuthorWindow : Gtk.Window {
	public AuthorWindow() : base("Author") {
		Label label = new Label("Автор\nДопустим тут информация");
		Add(label);
	}

	protected override bool OnDeleteEvent(Event e) { 
		Dispose();
		return true;
	}
}

class Hello {
	static void Main() {
		Application.Init();
		MyWindow w = new MyWindow();
		w.ShowAll();
		Application.Run();
	}
}
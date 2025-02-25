using Gdk;
using Gtk;
using static Gtk.Orientation;
class MyWindow : Gtk.Window
{
    Entry text_box = new Entry();
    Label input_label = new Label("password: ");
    Button generate_button = new Button("Generate");
    CheckButton uppercase = new CheckButton("Uppercase");
    Button exit_button = new Button("Exit");
    ComboBoxText language_comboBox = new ComboBoxText();
    public MyWindow() : base("password selector")
    {
        Box row = new Box(Horizontal, 0);
        row.Add(input_label);
        row.Add(text_box);
        Box row2 = new Box(Horizontal, 3);
        row2.Add(generate_button);
        generate_button.Clicked += on_generate;
        row2.Add(uppercase);
        Box row3 = new Box(Horizontal, 0);
        row3.Add(exit_button);
        language_comboBox.AppendText("RUS");
        language_comboBox.AppendText("ROM");
        language_comboBox.AppendText("ENG");
        row3.Add(language_comboBox);
        language_comboBox.Changed += change_language;
        exit_button.Clicked += on_exit;
        Box vbox = new Box(Vertical, 5);
        vbox.Add(row);
        vbox.Add(row2);
        vbox.Add(row3);
        Add(vbox);
        vbox.Margin = 5;
    }
    void on_generate(object? sender, EventArgs args)
    {
        Random rand = new Random();
        string consonants = "bcdfghjklmnoqrstuwxyz";
        string vowels = "aeiou";
        string password = "";
        for (int i = 0; i < 5; ++i)
        {
            password += consonants[rand.Next(consonants.Length)];
            password += vowels[rand.Next(vowels.Length)];
        }
        if (uppercase.Active)
            password = password.ToUpper();
        text_box.Text = password;
    }

    void change_language(object? sender, EventArgs args)
    {
        var value = language_comboBox.ActiveText;
        switch (value)
        {
            case "RUS":
                input_label.Text = "пароль:";
                generate_button.Label = "Сгенерировать";
                uppercase.Label = "Верхний регистр";
                exit_button.Label = "Выход";
                break;
            case "ROM":
                input_label.Text = "parola:";
                generate_button.Label = "Generați";
                uppercase.Label = "Majuscule";
                exit_button.Label = "Ieșire";
                break;
            case "ENG":
                input_label.Text = "password:";
                generate_button.Label = "Generate";
                uppercase.Label = "Uppercase";
                exit_button.Label = "Exit";
                break;
        }
    }

    void on_exit(object? sender, EventArgs args)
    {
        using (MessageDialog d = new MessageDialog(this,
        DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
        "the password is '{0}'", text_box.Text))
        {
            d.Run();
            Application.Quit();
        }
    }
    protected override bool OnDeleteEvent(Event e)
    {
        Application.Quit();
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
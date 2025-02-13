using Gdk;
using Gtk;
using System;

class MyWindow : Gtk.Window
{
    private Gtk.Label label;

    public MyWindow() : base("MyFirstApp")
    {
        // Устанавливаем размер окна
        this.SetDefaultSize(300, 200);

        // Создаем лейбл
        label = new Gtk.Label("Hello World!");
        label.Visible = false; // Скрываем лейбл по умолчанию

        // Создаем зеленую кнопку
        Gtk.Button greenButton = new Gtk.Button("Show Label");
        greenButton.Name = "greenButton"; // Устанавливаем имя для стилизации

        // Создаем красную кнопку
        Gtk.Button redButton = new Gtk.Button("Hide Label");
        redButton.Name = "redButton"; // Устанавливаем имя для стилизации

        // Подключаем события нажатия на кнопки
        greenButton.Clicked += OnGreenButtonClicked;
        redButton.Clicked += OnRedButtonClicked;

        // Создаем вертикальный контейнер для размещения элементов
        Gtk.Box box = new Gtk.Box(Gtk.Orientation.Vertical, 5);
        box.PackStart(label, true, true, 0);
        box.PackStart(greenButton, true, true, 0);
        box.PackStart(redButton, true, true, 0);

        // Добавляем контейнер в окно
        base.Add(box);

        // Применяем стили для кнопок
        ApplyStyles();
    }

    // Обработчик нажатия на зеленую кнопку
    private void OnGreenButtonClicked(object? sender, EventArgs e)
    {
        label.Visible = true; // Показываем лейбл
    }

    // Обработчик нажатия на красную кнопку
    private void OnRedButtonClicked(object? sender, EventArgs e)
    {
        label.Visible = false; // Скрываем лейбл
    }

    // Применение стилей для кнопок
    private void ApplyStyles()
    {
        var cssProvider = new CssProvider();

        try
        {
            // Загружаем CSS из строки
            cssProvider.LoadFromData(@"
                #greenButton {
                    background: green;
                    color: white;
                }
                #redButton {
                    background: red;
                    color: white;
                }
            ");

            // Применяем стили ко всему экрану
            Gtk.StyleContext.AddProviderForScreen(
                Gdk.Screen.Default,
                cssProvider,
                Gtk.StyleProviderPriority.Application
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при загрузке CSS: " + ex.Message);
        }
    }

    // Обработчик закрытия окна
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

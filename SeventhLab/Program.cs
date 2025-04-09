using System;
using Gtk;
using System.Data;
using System.Data.SQLite;
using MySql.Data.MySqlClient;
using Npgsql;

public class DatabaseConnectionTester : Window
{
    private ComboBoxText dbTypeCombo;
    private Entry connectionStringEntry;
    private TextView resultTextView;
    private Button testButton;
    
    public DatabaseConnectionTester() : base("Database Connection Tester")
    {
        SetDefaultSize(600, 400);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Application.Quit(); };
        
        Box mainBox = new Box(Orientation.Vertical, 5);
        
        Box dbTypeBox = new Box(Orientation.Horizontal, 5);
        dbTypeBox.PackStart(new Label("Database Type:"), false, false, 0);
        
        dbTypeCombo = new ComboBoxText();
        dbTypeCombo.AppendText("SQLite");
        dbTypeCombo.AppendText("MySQL");
        dbTypeCombo.AppendText("PostgreSQL");
        dbTypeCombo.Active = 0;
        dbTypeBox.PackStart(dbTypeCombo, true, true, 0);
        
        Box connectionStringBox = new Box(Orientation.Horizontal, 5);
        connectionStringBox.PackStart(new Label("Connection String:"), false, false, 0);
        
        connectionStringEntry = new Entry();
        connectionStringEntry.Text = "Connection string;";
        connectionStringBox.PackStart(connectionStringEntry, true, true, 0);
        
        testButton = new Button("Test Connection");
        testButton.Clicked += OnTestConnection;
        
        resultTextView = new TextView();
        resultTextView.Editable = false;
        ScrolledWindow scrolledWindow = new ScrolledWindow();
        scrolledWindow.Add(resultTextView);
        
        mainBox.PackStart(dbTypeBox, false, false, 0);
        mainBox.PackStart(connectionStringBox, false, false, 0);
        mainBox.PackStart(testButton, false, false, 0);
        mainBox.PackStart(scrolledWindow, true, true, 0);
        
        Add(mainBox);
        ShowAll();
    }
    
    private void OnTestConnection(object? sender, EventArgs args)
    {
        string dbType = dbTypeCombo.ActiveText;
        string connectionString = connectionStringEntry.Text;
        
        if (string.IsNullOrEmpty(connectionString))
        {
            ShowMessage("Error", "Connection string cannot be empty!");
            return;
        }
        
        try
        {
            bool success = false;
            string message = "";
            
            switch (dbType)
            {
                case "SQLite":
                    connectionString = "Data Source=:memory:;Version=3;";
                    success = TestSQLiteConnection(connectionString, out message);
                    break;
                case "MySQL":
                    connectionString = "Server=localhost;Database=testdb;Uid=root;Pwd=RootUser@123;";
                    success = TestMySQLConnection(connectionString, out message);
                    break;
                case "PostgreSQL":
                    connectionString = "Host=localhost;Database=testdb;Username=postgres;Password=postgres;";
                    success = TestPostgreSQLConnection(connectionString, out message);
                    break;
                default:
                    message = "Unsupported database type";
                    break;
            }

            connectionStringEntry.Text = connectionString;
            
            
            string result = $"Database Type: {dbType}\n" +
                           $"Connection String: {connectionString}\n" +
                           $"Status: {(success ? "SUCCESS" : "FAILED")}\n" +
                           $"Message: {message}\n\n";
            
            resultTextView.Buffer.Text += result;
        }
        catch (Exception ex)
        {
            ShowMessage("Error", $"Exception: {ex.Message}");
        }
    }
    
    private bool TestSQLiteConnection(string connectionString, out string message)
    {
        try
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                message = "Connection opened successfully";
                return true;
            }
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }
    
    private bool TestMySQLConnection(string connectionString, out string message)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                message = "Connection opened successfully";
                return true;
            }
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }
    
    private bool TestPostgreSQLConnection(string connectionString, out string message)
    {
        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                message = "Connection opened successfully";
                return true;
            }
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }
    
    private void ShowMessage(string title, string message)
    {
        MessageDialog md = new MessageDialog(this, 
            DialogFlags.DestroyWithParent,
            MessageType.Info, 
            ButtonsType.Close, 
            message);
        md.Title = title;
        md.Run();
        md.Destroy();
    }
    
    public static void Main()
    {
        Application.Init();
        new DatabaseConnectionTester();
        Application.Run();
    }
}
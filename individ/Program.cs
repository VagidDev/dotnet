using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using Gtk;
using System.IO;

namespace PolyclinicManagementSystem
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.Init();

            MainWindow mainWindow = new MainWindow();
            mainWindow.ShowAll();

            Application.Run();
        }
    }

    public class DatabaseManager
    {
        private static DatabaseManager _instance;
        private MySqlConnection _connection;
        private string _connectionString = "Server=localhost;Database=polyclinic;User ID=policlinyc_application;Password=apppPassword@123;";

        private DatabaseManager()
        {
            try
            {
                _connection = new MySqlConnection(_connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения к базе данных: {ex.Message}");
            }
        }

        public static DatabaseManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DatabaseManager();
            }
            return _instance;
        }

        public bool OpenConnection()
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка открытия соединения: {ex.Message}");
                return false;
            }
        }

        public void CloseConnection()
        {
            try
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка закрытия соединения: {ex.Message}");
            }
        }

        public DataTable ExecuteQuery(string query)
        {
            DataTable dataTable = new DataTable();

            if (OpenConnection())
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, _connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(dataTable);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка выполнения запроса: {ex.Message}");
                }
                finally
                {
                    CloseConnection();
                }
            }

            return dataTable;
        }

        public int ExecuteNonQuery(string query)
        {
            int result = 0;

            if (OpenConnection())
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, _connection);
                    result = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка выполнения запроса: {ex.Message}");
                }
                finally
                {
                    CloseConnection();
                }
            }

            return result;
        }
    }

    public class MainWindow : Window
    {
        private Notebook notebook;

        public MainWindow() : base("Система управления поликлиникой")
        {
            SetDefaultSize(800, 600);
            SetPosition(WindowPosition.Center);
            DeleteEvent += delegate { Application.Quit(); };

            notebook = new Notebook();

            notebook.AppendPage(new PatientsTab(), new Label("Пациенты"));
            notebook.AppendPage(new DoctorsTab(), new Label("Врачи"));
            notebook.AppendPage(new DiagnosesTab(), new Label("Диагнозы"));
            notebook.AppendPage(new TreatmentsTab(), new Label("Методы лечения"));
            notebook.AppendPage(new PrescriptionsTab(), new Label("Рецепты"));

            Add(notebook);
        }
    }

    public class PatientsTab : VBox
    {
        private TreeView patientTreeView;
        private ListStore patientListStore;
        private Entry firstNameEntry, lastNameEntry, phoneEntry, addressEntry;
        private DateEntry birthDateEntry;

        public PatientsTab()
        {
            patientListStore = new ListStore(typeof(int), typeof(string), typeof(string),
                                            typeof(string), typeof(string), typeof(string));

            patientTreeView = new TreeView(patientListStore);
            patientTreeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            patientTreeView.AppendColumn("Имя", new CellRendererText(), "text", 1);
            patientTreeView.AppendColumn("Фамилия", new CellRendererText(), "text", 2);
            patientTreeView.AppendColumn("Дата рождения", new CellRendererText(), "text", 3);
            patientTreeView.AppendColumn("Телефон", new CellRendererText(), "text", 4);
            patientTreeView.AppendColumn("Адрес", new CellRendererText(), "text", 5);

            ScrolledWindow scrolledWindow = new ScrolledWindow();
            scrolledWindow.Add(patientTreeView);

            Table formTable = new Table(5, 2, false);
            formTable.Attach(new Label("Имя:"), 0, 1, 0, 1);
            firstNameEntry = new Entry();
            formTable.Attach(firstNameEntry, 1, 2, 0, 1);

            formTable.Attach(new Label("Фамилия:"), 0, 1, 1, 2);
            lastNameEntry = new Entry();
            formTable.Attach(lastNameEntry, 1, 2, 1, 2);

            formTable.Attach(new Label("Дата рождения:"), 0, 1, 2, 3);
            birthDateEntry = new DateEntry();
            formTable.Attach(birthDateEntry, 1, 2, 2, 3);

            formTable.Attach(new Label("Телефон:"), 0, 1, 3, 4);
            phoneEntry = new Entry();
            formTable.Attach(phoneEntry, 1, 2, 3, 4);

            formTable.Attach(new Label("Адрес:"), 0, 1, 4, 5);
            addressEntry = new Entry();
            formTable.Attach(addressEntry, 1, 2, 4, 5);

            HBox buttonBox = new HBox();
            Button refreshButton = new Button("Обновить");
            refreshButton.Clicked += OnRefreshClicked;

            Button addButton = new Button("Добавить");
            addButton.Clicked += OnAddClicked;

            Button editButton = new Button("Редактировать");
            editButton.Clicked += OnEditClicked;

            Button deleteButton = new Button("Удалить");
            deleteButton.Clicked += OnDeleteClicked;

            buttonBox.PackStart(refreshButton, false, false, 5);
            buttonBox.PackStart(addButton, false, false, 5);
            buttonBox.PackStart(editButton, false, false, 5);
            buttonBox.PackStart(deleteButton, false, false, 5);

            PackStart(scrolledWindow, true, true, 5);
            PackStart(formTable, false, false, 5);
            PackStart(buttonBox, false, false, 5);

            LoadPatients();
        }

        private void LoadPatients()
        {
            patientListStore.Clear();

            string query = "SELECT id, first_name, last_name, birth_date, phone, address FROM patients";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                patientListStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["first_name"].ToString(),
                    row["last_name"].ToString(),
                    Convert.ToDateTime(row["birth_date"]).ToString("yyyy-MM-dd"),
                    row["phone"].ToString(),
                    row["address"].ToString()
                );
            }
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadPatients();
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            string firstName = firstNameEntry.Text;
            string lastName = lastNameEntry.Text;
            string birthDate = birthDateEntry.Date.ToString("yyyy-MM-dd");
            string phone = phoneEntry.Text;
            string address = addressEntry.Text;

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(birthDate))
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                        "Имя, фамилия и дата рождения обязательны для заполнения!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            string query = $"INSERT INTO patients (first_name, last_name, birth_date, phone, address) VALUES " +
                          $"('{firstName}', '{lastName}', '{birthDate}', '{phone}', '{address}')";

            int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

            if (result > 0)
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                       "Пациент успешно добавлен!");
                dialog.Run();
                dialog.Destroy();

                firstNameEntry.Text = "";
                lastNameEntry.Text = "";
                phoneEntry.Text = "";
                addressEntry.Text = "";

                LoadPatients();
            }
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            if (patientTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)patientListStore.GetValue(iter, 0);
                string firstName = firstNameEntry.Text;
                string lastName = lastNameEntry.Text;
                string birthDate = birthDateEntry.Date.ToString("yyyy-MM-dd");
                string phone = phoneEntry.Text;
                string address = addressEntry.Text;

                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(birthDate))
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                           "Имя, фамилия и дата рождения обязательны для заполнения!");
                    dialog.Run();
                    dialog.Destroy();
                    return;
                }

                string query = $"UPDATE patients SET first_name = '{firstName}', last_name = '{lastName}', " +
                              $"birth_date = '{birthDate}', phone = '{phone}', address = '{address}' WHERE id = {id}";

                int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                if (result > 0)
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                           "Данные пациента успешно обновлены!");
                    dialog.Run();
                    dialog.Destroy();

                    LoadPatients();
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите пациента для редактирования!");
                dialog.Run();
                dialog.Destroy();
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            if (patientTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)patientListStore.GetValue(iter, 0);

                MessageDialog confirmDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question,
                                                              ButtonsType.YesNo, "Вы уверены, что хотите удалить этого пациента?");
                ResponseType response = (ResponseType)confirmDialog.Run();
                confirmDialog.Destroy();

                if (response == ResponseType.Yes)
                {
                    string query = $"DELETE FROM patients WHERE id = {id}";
                    int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                    if (result > 0)
                    {
                        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                               "Пациент успешно удален!");
                        dialog.Run();
                        dialog.Destroy();

                        LoadPatients();
                    }
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите пациента для удаления!");
                dialog.Run();
                dialog.Destroy();
            }
        }
    }

    public class DateEntry : HBox
    {
        private SpinButton yearSpin;
        private SpinButton monthSpin;
        private SpinButton daySpin;

        public DateEntry()
        {
            
            yearSpin = new SpinButton(1900, 2100, 1);
            yearSpin.Value = DateTime.Now.Year;

          
            monthSpin = new SpinButton(1, 12, 1);
            monthSpin.Value = DateTime.Now.Month;

        
            daySpin = new SpinButton(1, 31, 1);
            daySpin.Value = DateTime.Now.Day;

           
            Label yearLabel = new Label("Год:");
            Label monthLabel = new Label("Месяц:");
            Label dayLabel = new Label("День:");

          
            PackStart(yearLabel, false, false, 2);
            PackStart(yearSpin, false, false, 2);
            PackStart(monthLabel, false, false, 2);
            PackStart(monthSpin, false, false, 2);
            PackStart(dayLabel, false, false, 2);
            PackStart(daySpin, false, false, 2);
        }

        public DateTime Date
        {
            get
            {
                return new DateTime((int)yearSpin.Value, (int)monthSpin.Value, (int)daySpin.Value);
            }
            set
            {
                yearSpin.Value = value.Year;
                monthSpin.Value = value.Month;
                daySpin.Value = value.Day;
            }
        }
    }

    public class DoctorsTab : VBox
    {
        private TreeView doctorTreeView;
        private ListStore doctorListStore;
        private Entry firstNameEntry, lastNameEntry, phoneEntry;
        private ComboBox specialtyComboBox;
        private ListStore specialtyStore;

        public DoctorsTab()
        {
            
            doctorListStore = new ListStore(typeof(int), typeof(string), typeof(string),
                                          typeof(string), typeof(string), typeof(int));

          
            doctorTreeView = new TreeView(doctorListStore);
            doctorTreeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            doctorTreeView.AppendColumn("Имя", new CellRendererText(), "text", 1);
            doctorTreeView.AppendColumn("Фамилия", new CellRendererText(), "text", 2);
            doctorTreeView.AppendColumn("Специальность", new CellRendererText(), "text", 3);
            doctorTreeView.AppendColumn("Телефон", new CellRendererText(), "text", 4);

            ScrolledWindow scrolledWindow = new ScrolledWindow();
            scrolledWindow.Add(doctorTreeView);

          
            specialtyStore = new ListStore(typeof(int), typeof(string));
            specialtyComboBox = new ComboBox(specialtyStore);
            CellRendererText specialtyCell = new CellRendererText();
            specialtyComboBox.PackStart(specialtyCell, true);
            specialtyComboBox.AddAttribute(specialtyCell, "text", 1);

          
            LoadSpecialties();

          
            Table formTable = new Table(4, 2, false);
            formTable.Attach(new Label("Имя:"), 0, 1, 0, 1);
            firstNameEntry = new Entry();
            formTable.Attach(firstNameEntry, 1, 2, 0, 1);

            formTable.Attach(new Label("Фамилия:"), 0, 1, 1, 2);
            lastNameEntry = new Entry();
            formTable.Attach(lastNameEntry, 1, 2, 1, 2);

            formTable.Attach(new Label("Специальность:"), 0, 1, 2, 3);
            formTable.Attach(specialtyComboBox, 1, 2, 2, 3);

            formTable.Attach(new Label("Телефон:"), 0, 1, 3, 4);
            phoneEntry = new Entry();
            formTable.Attach(phoneEntry, 1, 2, 3, 4);

          
            HBox buttonBox = new HBox();
            Button refreshButton = new Button("Обновить");
            refreshButton.Clicked += OnRefreshClicked;

            Button addButton = new Button("Добавить");
            addButton.Clicked += OnAddClicked;

            Button editButton = new Button("Редактировать");
            editButton.Clicked += OnEditClicked;

            Button deleteButton = new Button("Удалить");
            deleteButton.Clicked += OnDeleteClicked;

            Button manageSpecialtiesButton = new Button("Управление специальностями");
            manageSpecialtiesButton.Clicked += OnManageSpecialtiesClicked;

            buttonBox.PackStart(refreshButton, false, false, 5);
            buttonBox.PackStart(addButton, false, false, 5);
            buttonBox.PackStart(editButton, false, false, 5);
            buttonBox.PackStart(deleteButton, false, false, 5);
            buttonBox.PackStart(manageSpecialtiesButton, false, false, 5);

            PackStart(scrolledWindow, true, true, 5);
            PackStart(formTable, false, false, 5);
            PackStart(buttonBox, false, false, 5);

            LoadDoctors();
        }

        private void LoadSpecialties()
        {
            specialtyStore.Clear();

            string query = "SELECT id, name FROM specialties";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                specialtyStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["name"].ToString()
                );
            }
        }

        private void LoadDoctors()
        {
            doctorListStore.Clear();

            string query = "SELECT d.id, d.first_name, d.last_name, s.name as specialty_name, d.phone, d.specialty_id " +
                          "FROM doctors d LEFT JOIN specialties s ON d.specialty_id = s.id";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                int specialtyId = row["specialty_id"] != DBNull.Value ? Convert.ToInt32(row["specialty_id"]) : -1;

                doctorListStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["first_name"].ToString(),
                    row["last_name"].ToString(),
                    row["specialty_name"]?.ToString() ?? "Не указана",
                    row["phone"].ToString(),
                    specialtyId
                );
            }
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadSpecialties();
            LoadDoctors();
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            string firstName = firstNameEntry.Text;
            string lastName = lastNameEntry.Text;
            string phone = phoneEntry.Text;

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                       "Имя и фамилия обязательны для заполнения!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            int specialtyId = -1;
            if (specialtyComboBox.GetActiveIter(out TreeIter iter))
            {
                specialtyId = (int)specialtyStore.GetValue(iter, 0);
            }

            string query = $"INSERT INTO doctors (first_name, last_name, specialty_id, phone) VALUES " +
                          $"('{firstName}', '{lastName}', {(specialtyId > 0 ? specialtyId.ToString() : "NULL")}, '{phone}')";

            int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

            if (result > 0)
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                       "Врач успешно добавлен!");
                dialog.Run();
                dialog.Destroy();

                firstNameEntry.Text = "";
                lastNameEntry.Text = "";
                phoneEntry.Text = "";
                specialtyComboBox.Active = -1;

                LoadDoctors();
            }
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            if (doctorTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)doctorListStore.GetValue(iter, 0);
                string firstName = firstNameEntry.Text;
                string lastName = lastNameEntry.Text;
                string phone = phoneEntry.Text;

                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                           "Имя и фамилия обязательны для заполнения!");
                    dialog.Run();
                    dialog.Destroy();
                    return;
                }

                int specialtyId = -1;
                if (specialtyComboBox.GetActiveIter(out TreeIter specIter))
                {
                    specialtyId = (int)specialtyStore.GetValue(specIter, 0);
                }

                string query = $"UPDATE doctors SET first_name = '{firstName}', last_name = '{lastName}', " +
                              $"specialty_id = {(specialtyId > 0 ? specialtyId.ToString() : "NULL")}, phone = '{phone}' WHERE id = {id}";

                int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                if (result > 0)
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                           "Данные врача успешно обновлены!");
                    dialog.Run();
                    dialog.Destroy();

                    LoadDoctors();
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите врача для редактирования!");
                dialog.Run();
                dialog.Destroy();
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            if (doctorTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)doctorListStore.GetValue(iter, 0);

                MessageDialog confirmDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question,
                                                              ButtonsType.YesNo, "Вы уверены, что хотите удалить этого врача?");
                ResponseType response = (ResponseType)confirmDialog.Run();
                confirmDialog.Destroy();

                if (response == ResponseType.Yes)
                {
                    string query = $"DELETE FROM doctors WHERE id = {id}";
                    int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                    if (result > 0)
                    {
                        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                               "Врач успешно удален!");
                        dialog.Run();
                        dialog.Destroy();

                        LoadDoctors();
                    }
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите врача для удаления!");
                dialog.Run();
                dialog.Destroy();
            }
        }

        private void OnManageSpecialtiesClicked(object sender, EventArgs e)
        {
            SpecialtiesDialog dialog = new SpecialtiesDialog();
            dialog.ShowAll();
            dialog.Run();
            dialog.Destroy();

            LoadSpecialties();
            LoadDoctors();
        }
    }

    public class SpecialtiesDialog : Dialog
    {
        private TreeView specialtyTreeView;
        private ListStore specialtyListStore;
        private Entry specialtyNameEntry;

        public SpecialtiesDialog() : base("Управление специальностями", null, DialogFlags.Modal)
        {
            SetDefaultSize(400, 400);

            specialtyListStore = new ListStore(typeof(int), typeof(string));

            specialtyTreeView = new TreeView(specialtyListStore);
            specialtyTreeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            specialtyTreeView.AppendColumn("Название", new CellRendererText(), "text", 1);

            ScrolledWindow scrolledWindow = new ScrolledWindow();
            scrolledWindow.Add(specialtyTreeView);

            Table formTable = new Table(1, 2, false);
            formTable.Attach(new Label("Название:"), 0, 1, 0, 1);
            specialtyNameEntry = new Entry();
            formTable.Attach(specialtyNameEntry, 1, 2, 0, 1);

            HBox buttonBox = new HBox();
            Button refreshButton = new Button("Обновить");
            refreshButton.Clicked += OnRefreshClicked;

            Button addButton = new Button("Добавить");
            addButton.Clicked += OnAddClicked;

            Button editButton = new Button("Редактировать");
            editButton.Clicked += OnEditClicked;

            Button deleteButton = new Button("Удалить");
            deleteButton.Clicked += OnDeleteClicked;

            buttonBox.PackStart(refreshButton, false, false, 5);
            buttonBox.PackStart(addButton, false, false, 5);
            buttonBox.PackStart(editButton, false, false, 5);
            buttonBox.PackStart(deleteButton, false, false, 5);

            VBox contentArea = new VBox();
            contentArea.PackStart(scrolledWindow, true, true, 5);
            contentArea.PackStart(formTable, false, false, 5);
            contentArea.PackStart(buttonBox, false, false, 5);

            ((Container)this.Child).Add(contentArea);

            this.AddButton("Закрыть", (int)ResponseType.Close);

            LoadSpecialties();
        }

        private void LoadSpecialties()
        {
            specialtyListStore.Clear();

            string query = "SELECT id, name FROM specialties";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                specialtyListStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["name"].ToString()
                );
            }
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadSpecialties();
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            string name = specialtyNameEntry.Text;

            if (string.IsNullOrEmpty(name))
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                       "Название специальности обязательно для заполнения!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            string query = $"INSERT INTO specialties (name) VALUES ('{name}')";

            int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

            if (result > 0)
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                       "Специальность успешно добавлена!");
                dialog.Run();
                dialog.Destroy();

                specialtyNameEntry.Text = "";

                LoadSpecialties();
            }
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            if (specialtyTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)specialtyListStore.GetValue(iter, 0);
                string name = specialtyNameEntry.Text;

                if (string.IsNullOrEmpty(name))
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                           "Название специальности обязательно для заполнения!");
                    dialog.Run();
                    dialog.Destroy();
                    return;
                }

                string query = $"UPDATE specialties SET name = '{name}' WHERE id = {id}";

                int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                if (result > 0)
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                           "Специальность успешно обновлена!");
                    dialog.Run();
                    dialog.Destroy();

                    LoadSpecialties();
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите специальность для редактирования!");
                dialog.Run();
                dialog.Destroy();
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            if (specialtyTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)specialtyListStore.GetValue(iter, 0);

                MessageDialog confirmDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question,
                                                              ButtonsType.YesNo, "Вы уверены, что хотите удалить эту специальность?");
                ResponseType response = (ResponseType)confirmDialog.Run();
                confirmDialog.Destroy();

                if (response == ResponseType.Yes)
                {
                    string query = $"DELETE FROM specialties WHERE id = {id}";
                    int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                    if (result > 0)
                    {
                        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                               "Специальность успешно удалена!");
                        dialog.Run();
                        dialog.Destroy();

                        LoadSpecialties();
                    }
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите специальность для удаления!");
                dialog.Run();
                dialog.Destroy();
            }
        }
    }

    public class DiagnosesTab : VBox
    {
        private TreeView diagnosisTreeView;
        private ListStore diagnosisListStore;
        private ComboBox patientComboBox;
        private ComboBox doctorComboBox;
        private DateEntry diagnosisDateEntry;
        private TextView diagnosisTextView;
        private ListStore patientStore;
        private ListStore doctorStore;

        public DiagnosesTab()
        {
            diagnosisListStore = new ListStore(typeof(int), typeof(string), typeof(string),
                                             typeof(string), typeof(string), typeof(int), typeof(int));

            diagnosisTreeView = new TreeView(diagnosisListStore);
            diagnosisTreeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            diagnosisTreeView.AppendColumn("Пациент", new CellRendererText(), "text", 1);
            diagnosisTreeView.AppendColumn("Врач", new CellRendererText(), "text", 2);
            diagnosisTreeView.AppendColumn("Дата", new CellRendererText(), "text", 3);
            diagnosisTreeView.AppendColumn("Диагноз", new CellRendererText(), "text", 4);

            ScrolledWindow scrolledWindow = new ScrolledWindow();
            scrolledWindow.Add(diagnosisTreeView);

            patientStore = new ListStore(typeof(int), typeof(string));
            patientComboBox = new ComboBox(patientStore);
            CellRendererText patientCell = new CellRendererText();
            patientComboBox.PackStart(patientCell, true);
            patientComboBox.AddAttribute(patientCell, "text", 1);

            doctorStore = new ListStore(typeof(int), typeof(string));
            doctorComboBox = new ComboBox(doctorStore);
            CellRendererText doctorCell = new CellRendererText();
            doctorComboBox.PackStart(doctorCell, true);
            doctorComboBox.AddAttribute(doctorCell, "text", 1);

            LoadPatients();
            LoadDoctors();

            Table formTable = new Table(4, 2, false);
            formTable.Attach(new Label("Пациент:"), 0, 1, 0, 1);
            formTable.Attach(patientComboBox, 1, 2, 0, 1);

            formTable.Attach(new Label("Врач:"), 0, 1, 1, 2);
            formTable.Attach(doctorComboBox, 1, 2, 1, 2);

            formTable.Attach(new Label("Дата:"), 0, 1, 2, 3);
            diagnosisDateEntry = new DateEntry();
            formTable.Attach(diagnosisDateEntry, 1, 2, 2, 3);

            formTable.Attach(new Label("Диагноз:"), 0, 1, 3, 4);
            ScrolledWindow textScrolledWindow = new ScrolledWindow();
            diagnosisTextView = new TextView();
            textScrolledWindow.Add(diagnosisTextView);
            textScrolledWindow.SetSizeRequest(-1, 100);
            formTable.Attach(textScrolledWindow, 1, 2, 3, 4);

            HBox buttonBox = new HBox();
            Button refreshButton = new Button("Обновить");
            refreshButton.Clicked += OnRefreshClicked;

            Button addButton = new Button("Добавить");
            addButton.Clicked += OnAddClicked;

            Button editButton = new Button("Редактировать");
            editButton.Clicked += OnEditClicked;

            Button deleteButton = new Button("Удалить");
            deleteButton.Clicked += OnDeleteClicked;

            Button treatmentsButton = new Button("Методы лечения");
            treatmentsButton.Clicked += OnTreatmentsClicked;

            Button prescriptionsButton = new Button("Рецепты");
            prescriptionsButton.Clicked += OnPrescriptionsClicked;

            buttonBox.PackStart(refreshButton, false, false, 5);
            buttonBox.PackStart(addButton, false, false, 5);
            buttonBox.PackStart(editButton, false, false, 5);
            buttonBox.PackStart(deleteButton, false, false, 5);
            buttonBox.PackStart(treatmentsButton, false, false, 5);
            buttonBox.PackStart(prescriptionsButton, false, false, 5);

            PackStart(scrolledWindow, true, true, 5);
            PackStart(formTable, false, false, 5);
            PackStart(buttonBox, false, false, 5);

            diagnosisTreeView.Selection.Changed += OnDiagnosisSelected;

            LoadDiagnoses();
        }

        private void LoadPatients()
        {
            patientStore.Clear();

            string query = "SELECT id, CONCAT(last_name, ' ', first_name) AS full_name FROM patients";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                patientStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["full_name"].ToString()
                );
            }
        }

        private void LoadDoctors()
        {
            doctorStore.Clear();

            string query = "SELECT d.id, CONCAT(d.last_name, ' ', d.first_name, ' (', IFNULL(s.name, 'Не указана'), ')') AS full_name " +
                          "FROM doctors d LEFT JOIN specialties s ON d.specialty_id = s.id";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                doctorStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["full_name"].ToString()
                );
            }
        }

        private void LoadDiagnoses()
        {
            diagnosisListStore.Clear();

            string query = "SELECT d.id, CONCAT(p.last_name, ' ', p.first_name) AS patient_name, " +
                          "CONCAT(doc.last_name, ' ', doc.first_name) AS doctor_name, " +
                          "d.diagnosis_date, d.diagnosis, d.patient_id, d.doctor_id " +
                          "FROM diagnoses d " +
                          "JOIN patients p ON d.patient_id = p.id " +
                          "JOIN doctors doc ON d.doctor_id = doc.id";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                diagnosisListStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["patient_name"].ToString(),
                    row["doctor_name"].ToString(),
                    Convert.ToDateTime(row["diagnosis_date"]).ToString("yyyy-MM-dd"),
                    row["diagnosis"].ToString(),
                    Convert.ToInt32(row["patient_id"]),
                    Convert.ToInt32(row["doctor_id"])
                );
            }
        }

        private void OnDiagnosisSelected(object sender, EventArgs e)
        {
            if (diagnosisTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int patientId = (int)diagnosisListStore.GetValue(iter, 5);
                int doctorId = (int)diagnosisListStore.GetValue(iter, 6);
                string date = (string)diagnosisListStore.GetValue(iter, 3);
                string diagnosis = (string)diagnosisListStore.GetValue(iter, 4);

                SetComboBoxActiveById(patientComboBox, patientStore, patientId);
                SetComboBoxActiveById(doctorComboBox, doctorStore, doctorId);
                diagnosisDateEntry.Date = DateTime.Parse(date);
                diagnosisTextView.Buffer.Text = diagnosis;
            }
        }

        private void SetComboBoxActiveById(ComboBox comboBox, ListStore store, int id)
        {
            TreeIter iter;
            bool valid = store.GetIterFirst(out iter);
            int index = 0;

            while (valid)
            {
                int currentId = (int)store.GetValue(iter, 0);
                if (currentId == id)
                {
                    comboBox.Active = index;
                    return;
                }

                valid = store.IterNext(ref iter);
                index++;
            }

            comboBox.Active = -1;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadPatients();
            LoadDoctors();
            LoadDiagnoses();
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            if (!patientComboBox.GetActiveIter(out TreeIter patientIter) || !doctorComboBox.GetActiveIter(out TreeIter doctorIter))
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                       "Выберите пациента и врача!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            int patientId = (int)patientStore.GetValue(patientIter, 0);
            int doctorId = (int)doctorStore.GetValue(doctorIter, 0);
            string diagnosisDate = diagnosisDateEntry.Date.ToString("yyyy-MM-dd");
            string diagnosis = diagnosisTextView.Buffer.Text;

            if (string.IsNullOrEmpty(diagnosis))
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                       "Диагноз обязателен для заполнения!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            string query = $"INSERT INTO diagnoses (patient_id, doctor_id, diagnosis_date, diagnosis) VALUES " +
                          $"({patientId}, {doctorId}, '{diagnosisDate}', '{diagnosis}')";

            int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

            if (result > 0)
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                       "Диагноз успешно добавлен!");
                dialog.Run();
                dialog.Destroy();

             
                patientComboBox.Active = -1;
                doctorComboBox.Active = -1;
                diagnosisTextView.Buffer.Text = "";

              
                LoadDiagnoses();
            }
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            if (diagnosisTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)diagnosisListStore.GetValue(iter, 0);

                if (!patientComboBox.GetActiveIter(out TreeIter patientIter) || !doctorComboBox.GetActiveIter(out TreeIter doctorIter))
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                           "Выберите пациента и врача!");
                    dialog.Run();
                    dialog.Destroy();
                    return;
                }

                int patientId = (int)patientStore.GetValue(patientIter, 0);
                int doctorId = (int)doctorStore.GetValue(doctorIter, 0);
                string diagnosisDate = diagnosisDateEntry.Date.ToString("yyyy-MM-dd");
                string diagnosis = diagnosisTextView.Buffer.Text;

                string query = $"UPDATE diagnoses SET patient_id = {patientId}, doctor_id = {doctorId}, " +
                              $"diagnosis_date = '{diagnosisDate}', diagnosis = '{diagnosis}' WHERE id = {id}";

                int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                if (result > 0)
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                           "Диагноз успешно обновлен!");
                    dialog.Run();
                    dialog.Destroy();

                    LoadDiagnoses();
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите диагноз для редактирования!");
                dialog.Run();
                dialog.Destroy();
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            if (diagnosisTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)diagnosisListStore.GetValue(iter, 0);

                MessageDialog confirmDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question,
                                                              ButtonsType.YesNo, "Вы уверены, что хотите удалить этот диагноз?");
                ResponseType response = (ResponseType)confirmDialog.Run();
                confirmDialog.Destroy();

                if (response == ResponseType.Yes)
                {
                    string query = $"DELETE FROM diagnoses WHERE id = {id}";
                    int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                    if (result > 0)
                    {
                        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                               "Диагноз успешно удален!");
                        dialog.Run();
                        dialog.Destroy();

                        LoadDiagnoses();
                    }
                }
            }
        }

        private void OnTreatmentsClicked(object sender, EventArgs e)
        {
            if (diagnosisTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int diagnosisId = (int)diagnosisListStore.GetValue(iter, 0);

                TreatmentsDialog dialog = new TreatmentsDialog(diagnosisId);
                dialog.ShowAll();
                dialog.Run();
                dialog.Destroy();
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите диагноз для просмотра методов лечения!");
                dialog.Run();
                dialog.Destroy();
            }
        }

        private void OnPrescriptionsClicked(object sender, EventArgs e)
        {
            if (diagnosisTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int diagnosisId = (int)diagnosisListStore.GetValue(iter, 0);

                PrescriptionsDialog dialog = new PrescriptionsDialog(diagnosisId);
                dialog.ShowAll();
                dialog.Run();
                dialog.Destroy();
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите диагноз для просмотра рецептов!");
                dialog.Run();
                dialog.Destroy();
            }
        }
    }

  
    public class TreatmentsDialog : Dialog
    {
        private int _diagnosisId;
        private TreeView treatmentTreeView;
        private ListStore treatmentListStore;
        private TextView treatmentTextView;

        public TreatmentsDialog(int diagnosisId) : base("Методы лечения", null, DialogFlags.Modal)
        {
            _diagnosisId = diagnosisId;
            SetDefaultSize(500, 400);

            VBox contentBox = new VBox();

            treatmentListStore = new ListStore(typeof(int), typeof(string));
            treatmentTreeView = new TreeView(treatmentListStore);
            treatmentTreeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            treatmentTreeView.AppendColumn("Метод лечения", new CellRendererText(), "text", 1);

            ScrolledWindow scrolledWindow = new ScrolledWindow();
            scrolledWindow.Add(treatmentTreeView);

            Label descriptionLabel = new Label("Описание метода лечения:");

            treatmentTextView = new TextView();
            ScrolledWindow textScrolledWindow = new ScrolledWindow();
            textScrolledWindow.Add(treatmentTextView);
            textScrolledWindow.SetSizeRequest(-1, 100);

            HBox buttonBox = new HBox();
            Button addButton = new Button("Добавить");
            addButton.Clicked += OnAddClicked;

            Button editButton = new Button("Редактировать");
            editButton.Clicked += OnEditClicked;

            Button deleteButton = new Button("Удалить");
            deleteButton.Clicked += OnDeleteClicked;

            buttonBox.PackStart(addButton, false, false, 5);
            buttonBox.PackStart(editButton, false, false, 5);
            buttonBox.PackStart(deleteButton, false, false, 5);

            contentBox.PackStart(scrolledWindow, true, true, 5);
            contentBox.PackStart(descriptionLabel, false, false, 5);
            contentBox.PackStart(textScrolledWindow, false, false, 5);
            contentBox.PackStart(buttonBox, false, false, 5);

            ((Container)this.Child).Add(contentBox);

           
            this.AddButton("Закрыть", (int)ResponseType.Close);

          
            LoadTreatments();

           
            treatmentTreeView.Selection.Changed += OnTreatmentSelected;
        }

        private void LoadTreatments()
        {
            treatmentListStore.Clear();

            string query = $"SELECT id, treatment_description FROM treatments WHERE diagnosis_id = {_diagnosisId}";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                treatmentListStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["treatment_description"].ToString()
                );
            }
        }

     
        private void OnAddClicked(object sender, EventArgs e)
        {
            string treatment = treatmentTextView.Buffer.Text;

            if (string.IsNullOrEmpty(treatment))
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                       "Описание метода лечения не может быть пустым!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            string query = $"INSERT INTO treatments (diagnosis_id, treatment_description) VALUES ({_diagnosisId}, '{treatment}')";
            int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

            if (result > 0)
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                       "Метод лечения успешно добавлен!");
                dialog.Run();
                dialog.Destroy();

                treatmentTextView.Buffer.Text = "";
                LoadTreatments();
            }
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            if (treatmentTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)treatmentListStore.GetValue(iter, 0);
                string treatment = treatmentTextView.Buffer.Text;

                if (string.IsNullOrEmpty(treatment))
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                           "Описание метода лечения не может быть пустым!");
                    dialog.Run();
                    dialog.Destroy();
                    return;
                }

                string query = $"UPDATE treatments SET treatment_description = '{treatment}' WHERE id = {id}";
                int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                if (result > 0)
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                           "Метод лечения успешно обновлен!");
                    dialog.Run();
                    dialog.Destroy();

                    LoadTreatments();
                }
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            if (treatmentTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)treatmentListStore.GetValue(iter, 0);

                MessageDialog confirmDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question,
                                                              ButtonsType.YesNo, "Вы уверены, что хотите удалить этот метод лечения?");
                ResponseType response = (ResponseType)confirmDialog.Run();
                confirmDialog.Destroy();

                if (response == ResponseType.Yes)
                {
                    string query = $"DELETE FROM treatments WHERE id = {id}";
                    int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                    if (result > 0)
                    {
                        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                               "Метод лечения успешно удален!");
                        dialog.Run();
                        dialog.Destroy();

                        treatmentTextView.Buffer.Text = "";
                        LoadTreatments();
                    }
                }
            }
        }

        private void OnTreatmentSelected(object sender, EventArgs e)
        {
            if (treatmentTreeView.Selection.GetSelected(out TreeIter iter))
            {
                string treatment = (string)treatmentListStore.GetValue(iter, 1);
                treatmentTextView.Buffer.Text = treatment;
            }
        }
    }

    public class PrescriptionsDialog : Dialog
    {
        private int _diagnosisId;
        private TreeView prescriptionTreeView;
        private ListStore prescriptionListStore;
        private Entry medicineNameEntry;
        private Entry dosageEntry;

        public PrescriptionsDialog(int diagnosisId) : base("Рецепты", null, DialogFlags.Modal)
        {
            _diagnosisId = diagnosisId;
            SetDefaultSize(500, 400);

            VBox contentBox = new VBox();

            prescriptionListStore = new ListStore(typeof(int), typeof(string), typeof(string));
            prescriptionTreeView = new TreeView(prescriptionListStore);
            prescriptionTreeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            prescriptionTreeView.AppendColumn("Лекарство", new CellRendererText(), "text", 1);
            prescriptionTreeView.AppendColumn("Дозировка", new CellRendererText(), "text", 2);

            ScrolledWindow scrolledWindow = new ScrolledWindow();
            scrolledWindow.Add(prescriptionTreeView);

            Table formTable = new Table(2, 2, false);
            formTable.Attach(new Label("Название лекарства:"), 0, 1, 0, 1);
            medicineNameEntry = new Entry();
            formTable.Attach(medicineNameEntry, 1, 2, 0, 1);

            formTable.Attach(new Label("Дозировка:"), 0, 1, 1, 2);
            dosageEntry = new Entry();
            formTable.Attach(dosageEntry, 1, 2, 1, 2);

            HBox buttonBox = new HBox();
            Button addButton = new Button("Добавить");
            addButton.Clicked += OnAddClicked;

            Button editButton = new Button("Редактировать");
            editButton.Clicked += OnEditClicked;

            Button deleteButton = new Button("Удалить");
            deleteButton.Clicked += OnDeleteClicked;

            buttonBox.PackStart(addButton, false, false, 5);
            buttonBox.PackStart(editButton, false, false, 5);
            buttonBox.PackStart(deleteButton, false, false, 5);

            contentBox.PackStart(scrolledWindow, true, true, 5);
            contentBox.PackStart(formTable, false, false, 5);
            contentBox.PackStart(buttonBox, false, false, 5);

            ((Container)this.Child).Add(contentBox);

            this.AddButton("Закрыть", (int)ResponseType.Close);

            LoadPrescriptions();

            prescriptionTreeView.Selection.Changed += OnPrescriptionSelected;
        }

        private void LoadPrescriptions()
        {
            prescriptionListStore.Clear();

            string query = $"SELECT id, medicine_name, dosage FROM prescriptions WHERE diagnosis_id = {_diagnosisId}";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                prescriptionListStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["medicine_name"].ToString(),
                    row["dosage"].ToString()
                );
            }
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            string medicineName = medicineNameEntry.Text;
            string dosage = dosageEntry.Text;

            if (string.IsNullOrEmpty(medicineName) || string.IsNullOrEmpty(dosage))
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                       "Название лекарства и дозировка обязательны для заполнения!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            string query = $"INSERT INTO prescriptions (diagnosis_id, medicine_name, dosage) VALUES " +
                          $"({_diagnosisId}, '{medicineName}', '{dosage}')";

            int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

            if (result > 0)
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                       "Рецепт успешно добавлен!");
                dialog.Run();
                dialog.Destroy();

                medicineNameEntry.Text = "";
                dosageEntry.Text = "";

                LoadPrescriptions();
            }
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            if (prescriptionTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)prescriptionListStore.GetValue(iter, 0);
                string medicineName = medicineNameEntry.Text;
                string dosage = dosageEntry.Text;

                if (string.IsNullOrEmpty(medicineName) || string.IsNullOrEmpty(dosage))
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                           "Название лекарства и дозировка обязательны для заполнения!");
                    dialog.Run();
                    dialog.Destroy();
                    return;
                }

                string query = $"UPDATE prescriptions SET medicine_name = '{medicineName}', dosage = '{dosage}' WHERE id = {id}";
                int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                if (result > 0)
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                           "Рецепт успешно обновлен!");
                    dialog.Run();
                    dialog.Destroy();

                    LoadPrescriptions();
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите рецепт для редактирования!");
                dialog.Run();
                dialog.Destroy();
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            if (prescriptionTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)prescriptionListStore.GetValue(iter, 0);

                MessageDialog confirmDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question,
                                                              ButtonsType.YesNo, "Вы уверены, что хотите удалить этот рецепт?");
                ResponseType response = (ResponseType)confirmDialog.Run();
                confirmDialog.Destroy();

                if (response == ResponseType.Yes)
                {
                    string query = $"DELETE FROM prescriptions WHERE id = {id}";
                    int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                    if (result > 0)
                    {
                        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                               "Рецепт успешно удален!");
                        dialog.Run();
                        dialog.Destroy();

                        medicineNameEntry.Text = "";
                        dosageEntry.Text = "";

                        LoadPrescriptions();
                    }
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите рецепт для удаления!");
                dialog.Run();
                dialog.Destroy();
            }
        }

        private void OnPrescriptionSelected(object sender, EventArgs e)
        {
            if (prescriptionTreeView.Selection.GetSelected(out TreeIter iter))
            {
                string medicineName = (string)prescriptionListStore.GetValue(iter, 1);
                string dosage = (string)prescriptionListStore.GetValue(iter, 2);

                medicineNameEntry.Text = medicineName;
                dosageEntry.Text = dosage;
            }
        }
    }

    public class TreatmentsTab : VBox
    {
        private TreeView treatmentTreeView;
        private ListStore treatmentListStore;
        private ComboBox diagnosisComboBox;
        private ListStore diagnosisStore;
        private TextView treatmentTextView;

        public TreatmentsTab()
        {
            treatmentListStore = new ListStore(typeof(int), typeof(string), typeof(string));
            treatmentTreeView = new TreeView(treatmentListStore);
            treatmentTreeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            treatmentTreeView.AppendColumn("Диагноз", new CellRendererText(), "text", 1);
            treatmentTreeView.AppendColumn("Метод лечения", new CellRendererText(), "text", 2);

            ScrolledWindow scrolledWindow = new ScrolledWindow();
            scrolledWindow.Add(treatmentTreeView);

            diagnosisStore = new ListStore(typeof(int), typeof(string));
            diagnosisComboBox = new ComboBox(diagnosisStore);
            CellRendererText diagnosisCell = new CellRendererText();
            diagnosisComboBox.PackStart(diagnosisCell, true);
            diagnosisComboBox.AddAttribute(diagnosisCell, "text", 1);

            LoadDiagnoses();

            Table formTable = new Table(2, 2, false);
            formTable.Attach(new Label("Диагноз:"), 0, 1, 0, 1);
            formTable.Attach(diagnosisComboBox, 1, 2, 0, 1);

            formTable.Attach(new Label("Метод лечения:"), 0, 1, 1, 2);
            ScrolledWindow textScrolledWindow = new ScrolledWindow();
            treatmentTextView = new TextView();
            textScrolledWindow.Add(treatmentTextView);
            textScrolledWindow.SetSizeRequest(-1, 100);
            formTable.Attach(textScrolledWindow, 1, 2, 1, 2);

            HBox buttonBox = new HBox();
            Button refreshButton = new Button("Обновить");
            refreshButton.Clicked += OnRefreshClicked;

            Button addButton = new Button("Добавить");
            addButton.Clicked += OnAddClicked;

            Button editButton = new Button("Редактировать");
            editButton.Clicked += OnEditClicked;

            Button deleteButton = new Button("Удалить");
            deleteButton.Clicked += OnDeleteClicked;

            buttonBox.PackStart(refreshButton, false, false, 5);
            buttonBox.PackStart(addButton, false, false, 5);
            buttonBox.PackStart(editButton, false, false, 5);
            buttonBox.PackStart(deleteButton, false, false, 5);

            PackStart(scrolledWindow, true, true, 5);
            PackStart(formTable, false, false, 5);
            PackStart(buttonBox, false, false, 5);

            LoadTreatments();

            treatmentTreeView.Selection.Changed += OnTreatmentSelected;
        }

        private void LoadDiagnoses()
        {
            diagnosisStore.Clear();

            string query = "SELECT d.id, CONCAT(p.last_name, ' ', p.first_name, ' - ', d.diagnosis) AS diagnosis_info " +
                          "FROM diagnoses d JOIN patients p ON d.patient_id = p.id";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                diagnosisStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["diagnosis_info"].ToString()
                );
            }
        }

        private void LoadTreatments()
        {
            treatmentListStore.Clear();

            string query = "SELECT t.id, CONCAT(p.last_name, ' ', p.first_name, ' - ', d.diagnosis) AS diagnosis_info, " +
                          "t.treatment_description " +
                          "FROM treatments t " +
                          "JOIN diagnoses d ON t.diagnosis_id = d.id " +
                          "JOIN patients p ON d.patient_id = p.id";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                treatmentListStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["diagnosis_info"].ToString(),
                    row["treatment_description"].ToString()
                );
            }
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadDiagnoses();
            LoadTreatments();
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            if (!diagnosisComboBox.GetActiveIter(out TreeIter diagnosisIter))
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                       "Выберите диагноз!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            int diagnosisId = (int)diagnosisStore.GetValue(diagnosisIter, 0);
            string treatment = treatmentTextView.Buffer.Text;

            if (string.IsNullOrEmpty(treatment))
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                       "Описание метода лечения не может быть пустым!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            string query = $"INSERT INTO treatments (diagnosis_id, treatment_description) VALUES ({diagnosisId}, '{treatment}')";
            int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

            if (result > 0)
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                       "Метод лечения успешно добавлен!");
                dialog.Run();
                dialog.Destroy();

                treatmentTextView.Buffer.Text = "";
                LoadTreatments();
            }
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            if (treatmentTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)treatmentListStore.GetValue(iter, 0);
                string treatment = treatmentTextView.Buffer.Text;

                if (string.IsNullOrEmpty(treatment))
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                           "Описание метода лечения не может быть пустым!");
                    dialog.Run();
                    dialog.Destroy();
                    return;
                }

                string query = $"UPDATE treatments SET treatment_description = '{treatment}' WHERE id = {id}";
                int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                if (result > 0)
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                           "Метод лечения успешно обновлен!");
                    dialog.Run();
                    dialog.Destroy();

                    LoadTreatments();
                }
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            if (treatmentTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)treatmentListStore.GetValue(iter, 0);

                MessageDialog confirmDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question,
                                                              ButtonsType.YesNo, "Вы уверены, что хотите удалить этот метод лечения?");
                ResponseType response = (ResponseType)confirmDialog.Run();
                confirmDialog.Destroy();

                if (response == ResponseType.Yes)
                {
                    string query = $"DELETE FROM treatments WHERE id = {id}";
                    int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                    if (result > 0)
                    {
                        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                               "Метод лечения успешно удален!");
                        dialog.Run();
                        dialog.Destroy();

                        treatmentTextView.Buffer.Text = "";
                        LoadTreatments();
                    }
                }
            }
        }

        private void OnTreatmentSelected(object sender, EventArgs e)
        {
            if (treatmentTreeView.Selection.GetSelected(out TreeIter iter))
            {
                string treatment = (string)treatmentListStore.GetValue(iter, 2);
                treatmentTextView.Buffer.Text = treatment;
            }
        }
    }

    public class PrescriptionsTab : VBox
    {
        private TreeView prescriptionTreeView;
        private ListStore prescriptionListStore;
        private ComboBox diagnosisComboBox;
        private ListStore diagnosisStore;
        private Entry medicineNameEntry;
        private Entry dosageEntry;

        public PrescriptionsTab()
        {
            prescriptionListStore = new ListStore(typeof(int), typeof(string), typeof(string), typeof(string));
            prescriptionTreeView = new TreeView(prescriptionListStore);
            prescriptionTreeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            prescriptionTreeView.AppendColumn("Диагноз", new CellRendererText(), "text", 1);
            prescriptionTreeView.AppendColumn("Лекарство", new CellRendererText(), "text", 2);
            prescriptionTreeView.AppendColumn("Дозировка", new CellRendererText(), "text", 3);

            ScrolledWindow scrolledWindow = new ScrolledWindow();
            scrolledWindow.Add(prescriptionTreeView);

            diagnosisStore = new ListStore(typeof(int), typeof(string));
            diagnosisComboBox = new ComboBox(diagnosisStore);
            CellRendererText diagnosisCell = new CellRendererText();
            diagnosisComboBox.PackStart(diagnosisCell, true);
            diagnosisComboBox.AddAttribute(diagnosisCell, "text", 1);

            LoadDiagnoses();

            Table formTable = new Table(3, 2, false);
            formTable.Attach(new Label("Диагноз:"), 0, 1, 0, 1);
            formTable.Attach(diagnosisComboBox, 1, 2, 0, 1);

            formTable.Attach(new Label("Название лекарства:"), 0, 1, 1, 2);
            medicineNameEntry = new Entry();
            formTable.Attach(medicineNameEntry, 1, 2, 1, 2);

            formTable.Attach(new Label("Дозировка:"), 0, 1, 2, 3);
            dosageEntry = new Entry();
            formTable.Attach(dosageEntry, 1, 2, 2, 3);

            HBox buttonBox = new HBox();
            Button refreshButton = new Button("Обновить");
            refreshButton.Clicked += OnRefreshClicked;

            Button addButton = new Button("Добавить");
            addButton.Clicked += OnAddClicked;

            Button editButton = new Button("Редактировать");
            editButton.Clicked += OnEditClicked;

            Button deleteButton = new Button("Удалить");
            deleteButton.Clicked += OnDeleteClicked;

            buttonBox.PackStart(refreshButton, false, false, 5);
            buttonBox.PackStart(addButton, false, false, 5);
            buttonBox.PackStart(editButton, false, false, 5);
            buttonBox.PackStart(deleteButton, false, false, 5);

            PackStart(scrolledWindow, true, true, 5);
            PackStart(formTable, false, false, 5);
            PackStart(buttonBox, false, false, 5);

            LoadPrescriptions();

            prescriptionTreeView.Selection.Changed += OnPrescriptionSelected;
        }

        private void LoadDiagnoses()
        {
            diagnosisStore.Clear();

            string query = "SELECT d.id, CONCAT(p.last_name, ' ', p.first_name, ' - ', d.diagnosis) AS diagnosis_info " +
                          "FROM diagnoses d JOIN patients p ON d.patient_id = p.id";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                diagnosisStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["diagnosis_info"].ToString()
                );
            }
        }

        private void LoadPrescriptions()
        {
            prescriptionListStore.Clear();

            string query = "SELECT pr.id, CONCAT(p.last_name, ' ', p.first_name, ' - ', d.diagnosis) AS diagnosis_info, " +
                          "pr.medicine_name, pr.dosage " +
                          "FROM prescriptions pr " +
                          "JOIN diagnoses d ON pr.diagnosis_id = d.id " +
                          "JOIN patients p ON d.patient_id = p.id";
            DataTable result = DatabaseManager.GetInstance().ExecuteQuery(query);

            foreach (DataRow row in result.Rows)
            {
                prescriptionListStore.AppendValues(
                    Convert.ToInt32(row["id"]),
                    row["diagnosis_info"].ToString(),
                    row["medicine_name"].ToString(),
                    row["dosage"].ToString()
                );
            }
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadDiagnoses();
            LoadPrescriptions();
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            if (!diagnosisComboBox.GetActiveIter(out TreeIter diagnosisIter))
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                       "Выберите диагноз!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            int diagnosisId = (int)diagnosisStore.GetValue(diagnosisIter, 0);
            string medicineName = medicineNameEntry.Text;
            string dosage = dosageEntry.Text;

            if (string.IsNullOrEmpty(medicineName) || string.IsNullOrEmpty(dosage))
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                       "Название лекарства и дозировка обязательны для заполнения!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            string query = $"INSERT INTO prescriptions (diagnosis_id, medicine_name, dosage) VALUES " +
                          $"({diagnosisId}, '{medicineName}', '{dosage}')";

            int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

            if (result > 0)
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                       "Рецепт успешно добавлен!");
                dialog.Run();
                dialog.Destroy();

                medicineNameEntry.Text = "";
                dosageEntry.Text = "";
                LoadPrescriptions();
            }
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            if (prescriptionTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)prescriptionListStore.GetValue(iter, 0);
                string medicineName = medicineNameEntry.Text;
                string dosage = dosageEntry.Text;

                if (string.IsNullOrEmpty(medicineName) || string.IsNullOrEmpty(dosage))
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                           "Название лекарства и дозировка обязательны для заполнения!");
                    dialog.Run();
                    dialog.Destroy();
                    return;
                }

                string query = $"UPDATE prescriptions SET medicine_name = '{medicineName}', dosage = '{dosage}' WHERE id = {id}";
                int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                if (result > 0)
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                           "Рецепт успешно обновлен!");
                    dialog.Run();
                    dialog.Destroy();

                    LoadPrescriptions();
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите рецепт для редактирования!");
                dialog.Run();
                dialog.Destroy();
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            if (prescriptionTreeView.Selection.GetSelected(out TreeIter iter))
            {
                int id = (int)prescriptionListStore.GetValue(iter, 0);

                MessageDialog confirmDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Question,
                                                              ButtonsType.YesNo, "Вы уверены, что хотите удалить этот рецепт?");
                ResponseType response = (ResponseType)confirmDialog.Run();
                confirmDialog.Destroy();

                if (response == ResponseType.Yes)
                {
                    string query = $"DELETE FROM prescriptions WHERE id = {id}";
                    int result = DatabaseManager.GetInstance().ExecuteNonQuery(query);

                    if (result > 0)
                    {
                        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                               "Рецепт успешно удален!");
                        dialog.Run();
                        dialog.Destroy();

                        medicineNameEntry.Text = "";
                        dosageEntry.Text = "";
                        LoadPrescriptions();
                    }
                }
            }
            else
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok,
                                                       "Выберите рецепт для удаления!");
                dialog.Run();
                dialog.Destroy();
            }
        }

        private void OnPrescriptionSelected(object sender, EventArgs e)
        {
            if (prescriptionTreeView.Selection.GetSelected(out TreeIter iter))
            {
                string medicineName = (string)prescriptionListStore.GetValue(iter, 2);
                string dosage = (string)prescriptionListStore.GetValue(iter, 3);

                medicineNameEntry.Text = medicineName;
                dosageEntry.Text = dosage;
            }
        }
    }

    public class ReportGenerator
    {
        public static void GeneratePatientReport(int patientId)
        {
            string query = $"SELECT p.first_name, p.last_name, p.birth_date, p.phone, p.address " +
                          $"FROM patients p WHERE p.id = {patientId}";

            DataTable patientData = DatabaseManager.GetInstance().ExecuteQuery(query);

            if (patientData.Rows.Count == 0)
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                       "Пациент не найден!");
                dialog.Run();
                dialog.Destroy();
                return;
            }

            DataRow patient = patientData.Rows[0];

            query = $"SELECT d.diagnosis_date, d.diagnosis, doc.first_name, doc.last_name, s.name as specialty " +
                   $"FROM diagnoses d " +
                   $"JOIN doctors doc ON d.doctor_id = doc.id " +
                   $"LEFT JOIN specialties s ON doc.specialty_id = s.id " +
                   $"WHERE d.patient_id = {patientId} " +
                   $"ORDER BY d.diagnosis_date DESC";

            DataTable diagnosesData = DatabaseManager.GetInstance().ExecuteQuery(query);

            string reportPath = $"patient_report_{patientId}.txt";
            using (StreamWriter writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("============================================");
                writer.WriteLine("              КАРТА ПАЦИЕНТА               ");
                writer.WriteLine("============================================");
                writer.WriteLine();
                writer.WriteLine($"Пациент: {patient["last_name"]} {patient["first_name"]}");
                writer.WriteLine($"Дата рождения: {Convert.ToDateTime(patient["birth_date"]).ToString("dd.MM.yyyy")}");
                writer.WriteLine($"Телефон: {patient["phone"]}");
                writer.WriteLine($"Адрес: {patient["address"]}");
                writer.WriteLine();
                writer.WriteLine("============================================");
                writer.WriteLine("                  ДИАГНОЗЫ                 ");
                writer.WriteLine("============================================");
                writer.WriteLine();

                if (diagnosesData.Rows.Count == 0)
                {
                    writer.WriteLine("Диагнозы отсутствуют");
                }
                else
                {
                    foreach (DataRow diagnosis in diagnosesData.Rows)
                    {
                        writer.WriteLine($"Дата: {Convert.ToDateTime(diagnosis["diagnosis_date"]).ToString("dd.MM.yyyy")}");
                        writer.WriteLine($"Врач: {diagnosis["last_name"]} {diagnosis["first_name"]} ({diagnosis["specialty"]})");
                        writer.WriteLine($"Диагноз: {diagnosis["diagnosis"]}");

                        string treatmentQuery = $"SELECT t.treatment_description " +
                                               $"FROM treatments t " +
                                               $"JOIN diagnoses d ON t.diagnosis_id = d.id " +
                                               $"WHERE d.diagnosis = '{diagnosis["diagnosis"]}' AND d.patient_id = {patientId}";

                        DataTable treatmentsData = DatabaseManager.GetInstance().ExecuteQuery(treatmentQuery);

                        if (treatmentsData.Rows.Count > 0)
                        {
                            writer.WriteLine("Методы лечения:");
                            foreach (DataRow treatment in treatmentsData.Rows)
                            {
                                writer.WriteLine($" - {treatment["treatment_description"]}");
                            }
                        }

                        string prescriptionQuery = $"SELECT p.medicine_name, p.dosage " +
                                                  $"FROM prescriptions p " +
                                                  $"JOIN diagnoses d ON p.diagnosis_id = d.id " +
                                                  $"WHERE d.diagnosis = '{diagnosis["diagnosis"]}' AND d.patient_id = {patientId}";

                        DataTable prescriptionsData = DatabaseManager.GetInstance().ExecuteQuery(prescriptionQuery);

                        if (prescriptionsData.Rows.Count > 0)
                        {
                            writer.WriteLine("Назначения:");
                            foreach (DataRow prescription in prescriptionsData.Rows)
                            {
                                writer.WriteLine($" - {prescription["medicine_name"]}, {prescription["dosage"]}");
                            }
                        }

                        writer.WriteLine("--------------------------------------------");
                    }
                }

                writer.WriteLine();
                writer.WriteLine("============================================");
                writer.WriteLine($"Отчет сгенерирован: {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}");
                writer.WriteLine("============================================");
            }

            MessageDialog successDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
                                                          $"Отчет успешно сгенерирован и сохранен в файл {reportPath}");
            successDialog.Run();
            successDialog.Destroy();
        }
    }

    public class SecurityManager
    {
        private static SecurityManager _instance;
        private bool _isAuthenticated = false;
        private string _currentUser = "";

        private SecurityManager() { }

        public static SecurityManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SecurityManager();
            }
            return _instance;
        }

        public bool Login(string username, string password)
        {
            if (username == "admin" && password == "admin")
            {
                _isAuthenticated = true;
                _currentUser = username;
                return true;
            }

            return false;
        }

        public void Logout()
        {
            _isAuthenticated = false;
            _currentUser = "";
        }

        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
        }

        public string CurrentUser
        {
            get { return _currentUser; }
        }
    }

    public class LoginDialog : Dialog
    {
        private Entry usernameEntry;
        private Entry passwordEntry;

        public LoginDialog() : base("Вход в систему", null, DialogFlags.Modal)
        {
            SetDefaultSize(300, 150);

            Table formTable = new Table(2, 2, false);
            formTable.Attach(new Label("Имя пользователя:"), 0, 1, 0, 1);
            usernameEntry = new Entry();
            formTable.Attach(usernameEntry, 1, 2, 0, 1);

            formTable.Attach(new Label("Пароль:"), 0, 1, 1, 2);
            passwordEntry = new Entry();
            passwordEntry.Visibility = false;
            formTable.Attach(passwordEntry, 1, 2, 1, 2);

            ((Container)this.Child).Add(formTable);

            this.AddButton("Отмена", (int)ResponseType.Cancel);
            this.AddButton("Войти", (int)ResponseType.Ok);

            this.Response += OnResponse;
        }

        private void OnResponse(object sender, ResponseArgs args)
        {
            if (args.ResponseId == ResponseType.Ok)
            {
                string username = usernameEntry.Text;
                string password = passwordEntry.Text;

                if (SecurityManager.GetInstance().Login(username, password))
                {
                    args.RetVal = ResponseType.Ok;
                }
                else
                {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok,
                                                           "Неверное имя пользователя или пароль!");
                    dialog.Run();
                    dialog.Destroy();

                    args.RetVal = ResponseType.None;
                }
            }
        }
    }
}

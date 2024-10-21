using HospitalDAO.Data;
using HospitalDAO.Infrastructure;
using HospitalDAO.Models;
using HospitalDAO.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace WebLab3
{
    public class Program
    {
        public static void Main(string[] args)
        {

            // Создание хоста
            var builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;
            
            // Внедрение зависимости для доступа к БД с использованием EF
            IConfigurationRoot configuration = builder.Configuration.AddUserSecrets<Program>().Build();
            string connectionString = configuration.GetConnectionString("RemoteSQLConnection");

            // Получение пароя и имени пользователя из secrets.json
            string secretPass = configuration["Database:password"];
            string secretUser = configuration["Database:login"];
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString)
            {
                Password = secretPass,
                UserID = secretUser
            };

            connectionString = sqlConnectionStringBuilder.ConnectionString;

            services.AddDbContext<HospitalContext>(options => options.UseSqlServer(connectionString));

            // Добавление кэширования
            services.AddMemoryCache();

            // Добавление поддержки Cookies
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Добавление поддержки Session
            services.AddDistributedMemoryCache();
            services.AddSession();

            // Внедрение зависимости AppointmentsService
            services.AddScoped<IAppointmentsService, AppointmentsService>();
            // Внедрение зависимости DiagnosisService
            services.AddScoped<IDiagnosisService, DiagnosisService>();
            // Внедрение зависимости DoctorsService
            services.AddScoped<IDoctorsService, DoctorsService>();
            // Внедрение зависимости MedicamentsService
            services.AddScoped<IMedicamentsService, MedicamentsService>();
            // Внедрение зависимости PatientsService
            services.AddScoped<IPatientsService, PatientsService>();
            // Внедрение зависимости ReceptionsService
            services.AddScoped<IReceptionsService, ReceptionsService>();
            // Внедрение зависимости SpecializationsService
            services.AddScoped<ISpecializationsService, SpecializationsService>();

            var app = builder.Build();

            // Добавление поддержки статических файлов
            app.UseStaticFiles();

            // Добавление поддержки Session
            app.UseSession();



            // Вывод информации о клиенте
            app.Map("/info", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Формирование строки для вывода 
                    string strResponse = "<HTML><HEAD><TITLE>Информация</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><TABLE BORDER><TR><TH style='padding: 0; width: 100px;'><A href='/'>Главная</A></TH></TR></TABLE><H1>Информация:</H1>";
                    strResponse += "<BR> Сервер: " + context.Request.Host;
                    strResponse += "<BR> Путь: " + context.Request.PathBase;
                    strResponse += "<BR> Протокол: " + context.Request.Protocol;
                    strResponse += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(strResponse);
                });
            });



            // Вывод кэшированной информации из таблиц базы данных



            // Вывод Specializations
            app.Map("/specializations", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Обращение к сервису
                    ISpecializationsService iSpecializationsService = context.RequestServices.GetService<ISpecializationsService>();
                    IEnumerable<Specialization> specializations = iSpecializationsService.GetSpecializations("Specializations", 20);

                    string HtmlString = "<HTML><HEAD><TITLE>Специализации</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>Список специализаций</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>Код</TH>";
                    HtmlString += "<TH>Специализация</TH>";
                    HtmlString += "</TR>";
                    foreach (var specialization in specializations)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + specialization.SpecializationId + "</TD>";
                        HtmlString += "<TD>" + specialization.SpecializationName + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // Вывод Doctors
            app.Map("/doctors", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Обращение к сервису
                    IDoctorsService iDoctorsService = context.RequestServices.GetService<IDoctorsService>();
                    IEnumerable<Doctor> doctors = iDoctorsService.GetDoctors("Doctors", 20);

                    ISpecializationsService iSpecializationsService = context.RequestServices.GetService<ISpecializationsService>();
                    IEnumerable<Specialization> specializations = iSpecializationsService.GetSpecializations("Specializations", iSpecializationsService.GetSpecializationsCount());
                    Dictionary<int, string> specializationsDict = specializations.ToDictionary(s => s.SpecializationId, s => s.SpecializationName);

                    string HtmlString = "<HTML><HEAD><TITLE>Врачи</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>Список докторов</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>Код</TH>";
                    HtmlString += "<TH>ФИО</TH>";
                    HtmlString += "<TH>Специализация</TH>";
                    HtmlString += "<TH>Контактная информация</TH>";
                    HtmlString += "<TH>Пароль</TH>";
                    HtmlString += "</TR>";

                    foreach (var doctor in doctors)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + doctor.DoctorId + "</TD>";
                        HtmlString += "<TD>" + doctor.DoctorLastName + " " + doctor.DoctorFirstName + " " + doctor.DoctorSurname + "</TD>";
                        HtmlString += "<TD>" + specializationsDict.GetValueOrDefault(doctor.SpecializationId, "Неизвестно") + "</TD>";
                        HtmlString += "<TD>" + doctor.ContactData + "</TD>";
                        HtmlString += "<TD>" + doctor.Password + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // Вывод Patients
            app.Map("/patients", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Обращение к сервису
                    IPatientsService iPatientsService = context.RequestServices.GetService<IPatientsService>();
                    IEnumerable<Patient> patients = iPatientsService.GetPatients("Patients", 20);

                    string HtmlString = "<HTML><HEAD><TITLE>Пациенты</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>Список пациентов</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>Код</TH>";
                    HtmlString += "<TH>ФИО</TH>";
                    HtmlString += "<TH>Дата рождения</TH>";
                    HtmlString += "<TH>Контактная информация</TH>";
                    HtmlString += "</TR>";

                    foreach (var patient in patients)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + patient.PatientId + "</TD>";
                        HtmlString += "<TD>" + patient.PatientLastName + " " + patient.PatientFirstName + " " + patient.PatientSurname + "</TD>";
                        HtmlString += "<TD>" + patient.BirthDate + "</TD>";
                        HtmlString += "<TD>" + patient.ContactData + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // Вывод Diagnosis
            app.Map("/diagnosis", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Обращение к сервису
                    IDiagnosisService iDiagnosisService = context.RequestServices.GetService<IDiagnosisService>();
                    IEnumerable<Diagnos> diagnosis = iDiagnosisService.GetDiagnosis("Diagnosis", 20);

                    string HtmlString = "<HTML><HEAD><TITLE>Диагнозы</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>Список диагнозов</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>Код</TH>";
                    HtmlString += "<TH>Диагноз</TH>";
                    HtmlString += "<TH>Описание</TH>";
                    HtmlString += "</TR>";

                    foreach (var diagnos in diagnosis)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + diagnos.DiagnosId + "</TD>";
                        HtmlString += "<TD>" + diagnos.DiagnosName + "</TD>";
                        HtmlString += "<TD>" + diagnos.DiagnosDescription + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // Вывод Appointments
            app.Map("/appointments", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Обращение к сервису
                    IAppointmentsService iAppointmentsService = context.RequestServices.GetService<IAppointmentsService>();
                    IEnumerable<Appointment> appointments = iAppointmentsService.GetAppointments("Appointments", 20);

                    IDoctorsService iDoctorsService = context.RequestServices.GetService<IDoctorsService>();
                    IEnumerable<Doctor> doctors = iDoctorsService.GetDoctors("Doctors", iDoctorsService.GetDoctorsCount());
                    Dictionary<int, string> doctorsDict = doctors.ToDictionary(d => d.DoctorId, d => d.DoctorLastName + " " + d.DoctorFirstName + " " + d.DoctorSurname);

                    IPatientsService iPatientsService = context.RequestServices.GetService<IPatientsService>();
                    IEnumerable<Patient> patients = iPatientsService.GetPatients("Patients", iPatientsService.GetPatientsCount());
                    Dictionary<int, string> patientsDict = patients.ToDictionary(p => p.PatientId, p => p.PatientLastName + " " + p.PatientFirstName + " " + p.PatientSurname);

                    IDiagnosisService iDiagnosisService = context.RequestServices.GetService<IDiagnosisService>();
                    IEnumerable<Diagnos> diagnosis = iDiagnosisService.GetDiagnosis("Diagnosis", iDiagnosisService.GetDiagnosisCount());
                    Dictionary<int, string> diagnosisDict = diagnosis.ToDictionary(d => d.DiagnosId, d => d.DiagnosName);

                    string HtmlString = "<HTML><HEAD><TITLE>Приемы</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>Список приемов</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>Код</TH>";
                    HtmlString += "<TH>ФИО доктора</TH>";
                    HtmlString += "<TH>ФИО пациента</TH>";
                    HtmlString += "<TH>Дата приема</TH>";
                    HtmlString += "<TH>Диагноз</TH>";
                    HtmlString += "</TR>";

                    foreach (var appointment in appointments)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + appointment.AppointmentId + "</TD>";
                        HtmlString += "<TD>" + doctorsDict.GetValueOrDefault(appointment.DoctorId, "Неизвестно") + "</TD>";
                        HtmlString += "<TD>" + patientsDict.GetValueOrDefault(appointment.PatientId, "Неизвестно") + "</TD>";
                        HtmlString += "<TD>" + appointment.AppointmentDate + "</TD>";
                        HtmlString += "<TD>" + diagnosisDict.GetValueOrDefault(appointment.DiagnosId, "Неизвестно") + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // Вывод Medicaments
            app.Map("/medicaments", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Обращение к сервису
                    IMedicamentsService iMedicamentsService = context.RequestServices.GetService<IMedicamentsService>();
                    IEnumerable<Medicament> medicaments = iMedicamentsService.GetMedicaments("Medicaments", 20);

                    string HtmlString = "<HTML><HEAD><TITLE>Лекарства</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>Список лекарств</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>Код</TH>";
                    HtmlString += "<TH>Название</TH>";
                    HtmlString += "<TH>Дозировка</TH>";
                    HtmlString += "<TH>Стоимость</TH>";
                    HtmlString += "</TR>";

                    foreach (var medicament in medicaments)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + medicament.MedicamentId + "</TD>";
                        HtmlString += "<TD>" + medicament.MedicamentName + "</TD>";
                        HtmlString += "<TD>" + medicament.MedicamentDose + "</TD>";
                        HtmlString += "<TD>" + Math.Round(medicament.MedicamentPrice, 2) + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // Вывод Receptions
            app.Map("/receptions", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Обращение к сервису
                    IReceptionsService iReceptionsService = context.RequestServices.GetService<IReceptionsService>();
                    IEnumerable<Reception> receptions = iReceptionsService.GetReceptions("Receptions", 20);

                    IMedicamentsService iMedicamentsService = context.RequestServices.GetService<IMedicamentsService>();
                    IEnumerable<Medicament> medicaments = iMedicamentsService.GetMedicaments("Medicaments", iMedicamentsService.GetMedicamentsCount());
                    Dictionary<int, string> medicamentsDict = medicaments.ToDictionary(m => m.MedicamentId, m => m.MedicamentName);

                    string HtmlString = "<HTML><HEAD><TITLE>Выписки</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>Список рецептов</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>Код</TH>";
                    HtmlString += "<TH>Код приема</TH>";
                    HtmlString += "<TH>Лекарство</TH>";
                    HtmlString += "<TH>Дозировка</TH>";
                    HtmlString += "</TR>";

                    foreach (var reception in receptions)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + reception.ReceptionId + "</TD>";
                        HtmlString += "<TD>" + reception.AppointmentId + "</TD>";
                        HtmlString += "<TD>" + medicamentsDict.GetValueOrDefault(reception.MedicamentId, "Неизвестно") + "</TD>";
                        HtmlString += "<TD>" + reception.ReceptionDose + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(HtmlString);
                });
            });



            // Работа с Cookies



            app.Map("/searchform1", (appBuilder) => 
            {
                appBuilder.Run(async (context) => 
                {

                    if (context.Request.Method == "POST")
                    {
                        context.Response.Cookies.Append("DoctorId", context.Request.Form["DoctorId"]);
                        context.Response.Cookies.Append("PatientId", context.Request.Form["PatientId"]);
                        context.Response.Cookies.Append("AppointmentDate", context.Request.Form["AppointmentDate"]);

                        context.Response.Redirect("/searchform1");
                        return;
                    }

                    // Считывание данных из Cookies
                    string Doctor = context.Request.Cookies["DoctorId"];
                    int DoctorId = !string.IsNullOrEmpty(Doctor) ? int.Parse(Doctor) : 0;
                    string Patient = context.Request.Cookies["PatientId"];
                    int PatientId = !string.IsNullOrEmpty(Patient) ? int.Parse(Patient) : 0;
                    string Date = context.Request.Cookies["AppointmentDate"];
                    DateOnly AppointmentDate = string.IsNullOrEmpty(Date) ? DateOnly.FromDateTime(DateTime.Now) : DateOnly.Parse(Date);
                    int DiagnosId = SearchDiagnos(context, DoctorId, PatientId, AppointmentDate);

                    // Генерация формы с обновлёнными данными
                    string strResponse = GenerateForm(context, DoctorId, PatientId, AppointmentDate, DiagnosId, "Cookies");

                    await context.Response.WriteAsync(strResponse);
                });
            });



            // Работа с Session



            app.Map("/searchform2", (appBuilder) => {
                appBuilder.Run(async (context) => {

                    // Считывание из Session объекта Appointment
                    Appointment appointment = context.Session.Get<Appointment>("appointment") ?? new Appointment();
                    if (appointment.AppointmentDate == default)
                    {
                        appointment.AppointmentDate = DateOnly.FromDateTime(DateTime.Now);
                        appointment.DiagnosId = -1;
                    }
                    
                    // Загрузка данных в Session
                    if (context.Request.Method == "POST")
                    {
                        string Doctor = context.Request.Form["DoctorId"];
                        appointment.DoctorId = !string.IsNullOrEmpty(Doctor) ? int.Parse(Doctor) : 0;
                        string Patient = context.Request.Form["PatientId"];
                        appointment.PatientId = !string.IsNullOrEmpty(Patient) ? int.Parse(Patient) : 0;
                        string Date = context.Request.Form["AppointmentDate"];
                        appointment.AppointmentDate = string.IsNullOrEmpty(Date) ? DateOnly.FromDateTime(DateTime.Now) : DateOnly.Parse(Date);
                        appointment.DiagnosId = SearchDiagnos(context, appointment.DoctorId, appointment.PatientId, appointment.AppointmentDate);

                        context.Session.Set<Appointment>("appointment", appointment);
                    }

                    string strResponse = GenerateForm(context, appointment.DoctorId, appointment.PatientId, appointment.AppointmentDate, appointment.DiagnosId, "Session");

                    await context.Response.WriteAsync(strResponse);
                });
            });



            // Стартовая страница и кэширование данных таблицы на web-сервере
            app.Run((context) =>
                {
                // Обращение к сервису
                ISpecializationsService iSpecializationsService = context.RequestServices.GetService<ISpecializationsService>();
                iSpecializationsService.AddSpecializations("Specializations", iSpecializationsService.GetSpecializationsCount());
                IDoctorsService iDoctorsService = context.RequestServices.GetService<IDoctorsService>();
                iDoctorsService.AddDoctors("Doctors", iDoctorsService.GetDoctorsCount());
                IPatientsService iPatientsService = context.RequestServices.GetService<IPatientsService>();
                iPatientsService.AddPatients("Patients", iPatientsService.GetPatientsCount());
                IDiagnosisService iDiagnosisService = context.RequestServices.GetService<IDiagnosisService>();
                iDiagnosisService.AddDiagnosis("Diagnosis", iDiagnosisService.GetDiagnosisCount());
                IAppointmentsService iAppointmentsService = context.RequestServices.GetService<IAppointmentsService>();
                iAppointmentsService.AddAppointments("Appointments", iAppointmentsService.GetAppointmentsCount());
                IMedicamentsService iMedicamentsService = context.RequestServices.GetService<IMedicamentsService>();
                iMedicamentsService.AddMedicaments("Medicaments", iMedicamentsService.GetMedicamentsCount());
                IReceptionsService iReceptionsService = context.RequestServices.GetService<IReceptionsService>();
                iReceptionsService.AddReceptions("Reception", iReceptionsService.GetReceptionsCount());

                string HtmlString = "<HTML><HEAD><TITLE>Мед. клиника</TITLE></HEAD>" +
                                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                    "<BODY><H1>Главная</H1>";
                HtmlString += "<H2>Доступные таблицы:</H2>";
                HtmlString += "<UL>";
                    HtmlString += "<LI><A href='/specializations'>Специализации</A></LI>";
                    HtmlString += "<LI><A href='/doctors'>Врачи</A></LI>";
                    HtmlString += "<LI><A href='/patients'>Пациенты</A></LI>";
                    HtmlString += "<LI><A href='/diagnosis'>Диагнозы</A></LI>";
                    HtmlString += "<LI><A href='/appointments'>Приемы</A></LI>";
                    HtmlString += "<LI><A href='/medicaments'>Лекарства</A></LI>";
                    HtmlString += "<LI><A href='/receptions'>Рецепты</A></LI>";
                HtmlString += "</UL>";
                HtmlString += "<H2>Доп. данные:</H2>";
                HtmlString += "<UL>";
                    HtmlString += "<LI><A href='/searchform1'>Данные о приемах (Cookies)</A></LI>";
                    HtmlString += "<LI><A href='/searchform2'>Данные о приемах (Session)</A></LI>";
                HtmlString += "</UL>";
                HtmlString += "</BODY></HTML>";
                return context.Response.WriteAsync(HtmlString);
            });

            app.Run();
        }

        // Вывод содержания
        static string PrintContentInTables()
        {
            string HtmlString = "<BR>";
            HtmlString += "<TABLE BORDER=1>";
                HtmlString += "<TR>";
                    HtmlString += "<TH style='padding: 0; width: 100px;'><A href='/'>Главная</A></TH>";
                HtmlString += "</TR>";
            HtmlString += "</TABLE>";
            HtmlString += "<TABLE BORDER=1 style='margin-left: 53px; margin-top: 3px'>";
                HtmlString += "<TR>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/specializations'>Специализации</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/doctors'>Врачи</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/patients'>Пациенты</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/diagnosis'>Диагнозы</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/appointments'>Приемы</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/medicaments'>Лекарства</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/receptions'>Рецепты</A></TH>";
                HtmlString += "</TR>";
            HtmlString += "</TABLE>";

            return HtmlString;
        }

        // Форма для просмотра информации
        public static string GenerateForm(HttpContext context, int doctorId, int patientId, DateOnly appointmentDate, int diagnosId, string str)
        {
            var db = context.RequestServices.GetService<HospitalContext>();
            List<Diagnos> diagnosis = db.Diagnosis.ToList();
            diagnosis.Sort();
            List<Doctor> doctors = db.Doctors.ToList();
            doctors.Sort();
            List<Patient> patients = db.Patients.ToList();
            patients.Sort();

            string strResponse = $"<HTML><HEAD><TITLE>Добавление приема в {str}</TITLE></HEAD>" +
                                 "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                 "<BODY><TABLE BORDER><TR><TH style='padding: 0; width: 100px;'><A href='/'>Главная</A></TH></TR></TABLE>" +
                                 "<H1>Данные приема:</H1><TABLE BORDER=1><TR><TH><BR><FORM style='margin-left:10px; margin-right:10px' method='POST'>" +
                                 "<input type='hidden' name='isSubmitted' value='true' />" +
                                 "ФИО доктора:<BR><SELECT style='width: 400px;' padding: 0;' name='DoctorId'>";

            foreach (var doctor in doctors)
            {
                strResponse += $"<option value='{doctor.DoctorId}'" +
                               $"{(doctorId == doctor.DoctorId ? " selected" : "")}>" +
                               $"{doctor.DoctorLastName + " " + doctor.DoctorFirstName + " " + doctor.DoctorSurname}</option>";
            }

            strResponse += "</SELECT><BR><BR>" +
                           "ФИО пациента:<BR><SELECT style='width: 400px;' name='PatientId'>";

            foreach (var patient in patients)
            {
                strResponse += $"<option value='{patient.PatientId}'" +
                               $"{(patientId == patient.PatientId ? " selected" : "")}>" +
                               $"{patient.PatientLastName + " " + patient.PatientFirstName + " " + patient.PatientSurname}</option>";
            }

            strResponse += "</SELECT><BR><BR>" +
                           "Дата приема:<BR><INPUT type='date' name='AppointmentDate' value='" + appointmentDate.ToString("yyyy-MM-dd") + "'><BR><BR>" +
                           "<BR><INPUT style='width: 200px;' type='submit' value='Найти диагноз'>";

            if (diagnosId == -1)
                strResponse += "<P>Данных не найдено</P>";
            else
            {
                bool flag = false;
                int i = 0;

                while (!flag && i < diagnosis.Count())
                {
                    if (diagnosis[i].DiagnosId == diagnosId)
                    {
                        flag = true;
                        strResponse += $"<P>Диагноз: {diagnosis[i].DiagnosName}</P>";
                    }
                    i++;
                }
                if (!flag)
                {
                    strResponse += "<P>Данных не найдено</P>";
                }
            }

            strResponse += "</FORM></TH></TR></TABLE></BODY></HTML>";

            return strResponse;
        }

        static int SearchDiagnos(HttpContext context, int doctorId, int patientId, DateOnly appointmentDate)
        {
            var db = context.RequestServices.GetService<HospitalContext>();
            List<Appointment> appointments = db.Appointments.ToList();

            bool flag = false;
            int i = 0;

            int diagnosId = -1;

            while (!flag && i < appointments.Count())
            {
                if (appointments[i].DoctorId == doctorId && appointments[i].PatientId == patientId && appointments[i].AppointmentDate == appointmentDate)
                {
                    flag = true;
                    diagnosId = appointments[i].DiagnosId;
                }
                i++;
            }

            return diagnosId;
        } 
    }
}

using System;
using HospitalDAO.Data;
using HospitalDAO.Infrastructure;
using HospitalDAO.Models;
using HospitalDAO.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace WebLab3
{
    public class Program
    {
        public static void Main(string[] args)
        {

            // Создание хоста
            var builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;
            // внедрение зависимости для доступа к БД с использованием EF
            //string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            IConfigurationRoot configuration = builder.Configuration.AddUserSecrets<Program>().Build();
            string connectionString = configuration.GetConnectionString("RemoteSQLConnection");

            //Считываем пароль и имя пользователя из secrets.json
            string secretPass = configuration["Database:password"];
            string secretUser = configuration["Database:login"];
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString)
            {
                Password = secretPass,
                UserID = secretUser
            };

            connectionString = sqlConnectionStringBuilder.ConnectionString;

            services.AddDbContext<HospitalContext>(options => options.UseSqlServer(connectionString));

            // добавление кэширования
            services.AddMemoryCache();

            // добавление поддержки Cookies
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // добавление поддержки Session
            services.AddDistributedMemoryCache();
            services.AddSession();

            // внедрение зависимости AppointmentsService
            services.AddScoped<IAppointmentsService, AppointmentsService>();
            // внедрение зависимости DiagnosisService
            services.AddScoped<IDiagnosisService, DiagnosisService>();
            // внедрение зависимости DoctorsService
            services.AddScoped<IDoctorsService, DoctorsService>();
            // внедрение зависимости MedicamentsService
            services.AddScoped<IMedicamentsService, MedicamentsService>();
            // внедрение зависимости PatientsService
            services.AddScoped<IPatientsService, PatientsService>();
            // внедрение зависимости ReceptionsService
            services.AddScoped<IReceptionsService, ReceptionsService>();
            // внедрение зависимости SpecializationsService
            services.AddScoped<ISpecializationsService, SpecializationsService>();

            var app = builder.Build();

            // добавляем поддержку статических файлов
            app.UseStaticFiles();

            // добавляем поддержку Session
            app.UseSession();

            // Вывод информации о клиенте
            app.Map("/info", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Формирование строки для вывода 
                    string strResponse = "<HTML><HEAD><TITLE>Информация</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><TABLE BORDER><TR><TH><A href='/'>Главная</A></TH></TR></TABLE><H1>Информация:</H1>";
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

                    // Загрузка данных из куки
                    string DoctorLastName = context.Request.Cookies["DoctorLastName"] ?? "";
                    string DoctorFirstName = context.Request.Cookies["DoctorFirstName"] ?? "";
                    string DoctorSurname = context.Request.Cookies["DoctorSurname"] ?? "";
                    string specId = context.Request.Cookies["SpecializationId"];
                    int SpecializationId = !string.IsNullOrEmpty(specId) ? int.Parse(specId) : 0;
                    string ContactData = context.Request.Cookies["ContactData"] ?? "";
                    string Password = context.Request.Cookies["Password"] ?? "";

                    var db = context.RequestServices.GetService<HospitalContext>();

                    // Загрузить доступные специализации из базы данных
                    List<Specialization> specializations = db.Specializations.ToList();

                    // Формирование строки для вывода динамической HTML формы
                    string strResponse = "<HTML><HEAD><TITLE>Cookies</TITLE></HEAD>" +
                                         "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                         "<BODY><TABLE BORDER><TR><TH><A href='/'>Главная</A></TH></TR></TABLE><BR>" +
                                         "<FORM action='/searchform1' method='POST'>" +
                                         "Фамилия:<BR><INPUT type='text' name='DoctorLastName' value='" + DoctorLastName + "'><BR>" +
                                         "Имя:<BR><INPUT type='text' name='DoctorFirstName' value='" + DoctorFirstName + "'><BR>" +
                                         "Отчество:<BR><INPUT type='text' name='DoctorSurname' value='" + DoctorSurname + "'><BR>" +
                                         "Специализация:<BR><SELECT name='SpecializationId'>";

                    foreach (var specialization in specializations)
                    {
                        strResponse += $"<option value='{specialization.SpecializationId}'" +
                                       $"{(SpecializationId == specialization.SpecializationId ? " selected" : "")}>" +
                                       $"{specialization.SpecializationName}</option>";
                    }

                    strResponse += "</SELECT>" +
                                   "<BR>Контактные данные:<BR><INPUT type='text' name='ContactData' value='" + ContactData + "'><BR>" +
                                   "Пароль:<BR><INPUT type='text' name='Password' value='" + Password + "'><BR>" +
                                   "<BR><INPUT type='submit' value='Сохранить в Cookies'>" +
                                   "<INPUT type='submit' value='Показать'></FORM></BODY></HTML>";

                    if (context.Request.Method == "POST")
                    {
                        // Запись в Cookies данных объекта Doctor
                        context.Response.Cookies.Append("DoctorLastName", context.Request.Form["DoctorLastName"]);
                        context.Response.Cookies.Append("DoctorFirstName", context.Request.Form["DoctorFirstName"]);
                        context.Response.Cookies.Append("DoctorSurname", context.Request.Form["DoctorSurname"]);
                        context.Response.Cookies.Append("SpecializationId", context.Request.Form["SpecializationId"]);
                        context.Response.Cookies.Append("ContactData", context.Request.Form["ContactData"]);
                        context.Response.Cookies.Append("Password", context.Request.Form["Password"]);
                    }

                    // Асинхронный вывод динамической HTML формы
                    await context.Response.WriteAsync(strResponse);
                });
            });



            // Работа с Session 



            //Запоминание в Session значений, введенных в форме
            app.Map("/searchform2", (appBuilder) => {
                appBuilder.Run(async (context) => {
                    // Считывание из Session объекта Doctor
                    Doctor doctor = context.Session.Get<Doctor>("doctor") ?? new Doctor();

                    var db = context.RequestServices.GetService<HospitalContext>();
                    // Загрузить доступные специализации из базы данных
                    List<Specialization> specializations = db.Specializations.ToList();

                    // Формирование строки для вывода динамической HTML формы
                    string strResponse = "<HTML><HEAD><TITLE>Session</TITLE></HEAD>" +
                                         "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                         "<BODY><TABLE BORDER><TR><TH><A href='/'>Главная</A></TH></TR></TABLE><BR>" +
                                         "<FORM action='/searchform2' method='POST'>" +
                                         "Фамилия:<BR><INPUT type='text' name='DoctorLastName' value='" + doctor.DoctorLastName + "'><BR>" +
                                         "Имя:<BR><INPUT type='text' name='DoctorFirstName' value='" + doctor.DoctorFirstName + "'><BR>" +
                                         "Отчество:<BR><INPUT type='text' name='DoctorSurname' value='" + doctor.DoctorSurname + "'><BR>" +
                                         "Специализация:<BR><SELECT name='SpecializationId'>";

                    foreach (var specialization in specializations)
                    {
                        strResponse += $"<option value='{specialization.SpecializationId}'" +
                                       $"{(doctor.SpecializationId == specialization.SpecializationId ? " selected" : "")}>" +
                                       $"{specialization.SpecializationName}</option>";
                    }

                    strResponse += "</SELECT>" +
                                   "<BR>Контактные данные:<BR><INPUT type='text' name='ContactData' value='" + doctor.ContactData + "'><BR>" +
                                   "Пароль:<BR><INPUT type='text' name='Password' value='" + doctor.Password + "'><BR>" +
                                   "<BR><INPUT type='submit' value='Сохранить в Session'>" +
                                   "<INPUT type='submit' value='Показать'></FORM></BODY></HTML>";

                    if (context.Request.Method == "POST")
                    {
                        // Запись в Session данных объекта Doctor
                        doctor.DoctorLastName = context.Request.Form["DoctorLastName"];
                        doctor.DoctorFirstName = context.Request.Form["DoctorFirstName"];
                        doctor.DoctorSurname = context.Request.Form["DoctorSurname"];
                        string specId = context.Request.Form["SpecializationId"];
                        doctor.SpecializationId = !string.IsNullOrEmpty(specId) ? int.Parse(specId) : 0;
                        doctor.ContactData = context.Request.Form["ContactData"];
                        doctor.Password = context.Request.Form["Password"];
                        context.Session.Set<Doctor>("doctor", doctor);
                    }

                    // Асинхронный вывод динамической HTML формы
                    await context.Response.WriteAsync(strResponse);
                });
            });




            // Стартовая страница и кэширование данных таблицы на web-сервере
            app.Run((context) =>
            {
                //обращение к сервису
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
                HtmlString += "<LI><A href='/searchform1'>Данные о докторах (Cookies)</A></LI>";
                HtmlString += "<LI><A href='/searchform2'>Данные о докторах (Session)</A></LI>";
                HtmlString += "</UL>";
                HtmlString += "</BODY></HTML>";
                return context.Response.WriteAsync(HtmlString);

            });

            app.Run();
        }

        static string PrintContentInTables()
        {
            string HtmlString = "<BR><TABLE BORDER=1 style='border-spacing: 0;'>";
            HtmlString += "<TR>";
            HtmlString += "<TH style='padding: 0;'><A href='/'>Главная</A></TH>";
            HtmlString += "</TR>";
            HtmlString += "<TR><TH style='border: 1px solid white; padding: 0;'></TH>";
            HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/specializations'>Специализации</A></TH>";
            HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/doctors'>Врачи</A></TH>";
            HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/patients'>Пациенты</A></TH>";
            HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/diagnosis'>Диагнозы</A></TH>";
            HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/appointments'>Приемы</A></TH>";
            HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/medicaments'>Лекарства</A></TH>";
            HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/receptions'>Рецепты</A></TH>";
            HtmlString += "</TR></TABLE>";

            return HtmlString;
        }
    }
}

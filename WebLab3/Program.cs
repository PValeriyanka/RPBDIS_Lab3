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

            // �������� �����
            var builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;
            
            // ��������� ����������� ��� ������� � �� � �������������� EF
            IConfigurationRoot configuration = builder.Configuration.AddUserSecrets<Program>().Build();
            string connectionString = configuration.GetConnectionString("RemoteSQLConnection");

            // ��������� ����� � ����� ������������ �� secrets.json
            string secretPass = configuration["Database:password"];
            string secretUser = configuration["Database:login"];
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString)
            {
                Password = secretPass,
                UserID = secretUser
            };

            connectionString = sqlConnectionStringBuilder.ConnectionString;

            services.AddDbContext<HospitalContext>(options => options.UseSqlServer(connectionString));

            // ���������� �����������
            services.AddMemoryCache();

            // ���������� ��������� Cookies
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // ���������� ��������� Session
            services.AddDistributedMemoryCache();
            services.AddSession();

            // ��������� ����������� AppointmentsService
            services.AddScoped<IAppointmentsService, AppointmentsService>();
            // ��������� ����������� DiagnosisService
            services.AddScoped<IDiagnosisService, DiagnosisService>();
            // ��������� ����������� DoctorsService
            services.AddScoped<IDoctorsService, DoctorsService>();
            // ��������� ����������� MedicamentsService
            services.AddScoped<IMedicamentsService, MedicamentsService>();
            // ��������� ����������� PatientsService
            services.AddScoped<IPatientsService, PatientsService>();
            // ��������� ����������� ReceptionsService
            services.AddScoped<IReceptionsService, ReceptionsService>();
            // ��������� ����������� SpecializationsService
            services.AddScoped<ISpecializationsService, SpecializationsService>();

            var app = builder.Build();

            // ���������� ��������� ����������� ������
            app.UseStaticFiles();

            // ���������� ��������� Session
            app.UseSession();



            // ����� ���������� � �������
            app.Map("/info", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ������������ ������ ��� ������ 
                    string strResponse = "<HTML><HEAD><TITLE>����������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><TABLE BORDER><TR><TH style='padding: 0; width: 100px;'><A href='/'>�������</A></TH></TR></TABLE><H1>����������:</H1>";
                    strResponse += "<BR> ������: " + context.Request.Host;
                    strResponse += "<BR> ����: " + context.Request.PathBase;
                    strResponse += "<BR> ��������: " + context.Request.Protocol;
                    strResponse += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(strResponse);
                });
            });



            // ����� ������������ ���������� �� ������ ���� ������



            // ����� Specializations
            app.Map("/specializations", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ��������� � �������
                    ISpecializationsService iSpecializationsService = context.RequestServices.GetService<ISpecializationsService>();
                    IEnumerable<Specialization> specializations = iSpecializationsService.GetSpecializations("Specializations", 20);

                    string HtmlString = "<HTML><HEAD><TITLE>�������������</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>������ �������������</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>�������������</TH>";
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

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // ����� Doctors
            app.Map("/doctors", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ��������� � �������
                    IDoctorsService iDoctorsService = context.RequestServices.GetService<IDoctorsService>();
                    IEnumerable<Doctor> doctors = iDoctorsService.GetDoctors("Doctors", 20);

                    ISpecializationsService iSpecializationsService = context.RequestServices.GetService<ISpecializationsService>();
                    IEnumerable<Specialization> specializations = iSpecializationsService.GetSpecializations("Specializations", iSpecializationsService.GetSpecializationsCount());
                    Dictionary<int, string> specializationsDict = specializations.ToDictionary(s => s.SpecializationId, s => s.SpecializationName);

                    string HtmlString = "<HTML><HEAD><TITLE>�����</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>������ ��������</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>�������������</TH>";
                    HtmlString += "<TH>���������� ����������</TH>";
                    HtmlString += "<TH>������</TH>";
                    HtmlString += "</TR>";

                    foreach (var doctor in doctors)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + doctor.DoctorId + "</TD>";
                        HtmlString += "<TD>" + doctor.DoctorLastName + " " + doctor.DoctorFirstName + " " + doctor.DoctorSurname + "</TD>";
                        HtmlString += "<TD>" + specializationsDict.GetValueOrDefault(doctor.SpecializationId, "����������") + "</TD>";
                        HtmlString += "<TD>" + doctor.ContactData + "</TD>";
                        HtmlString += "<TD>" + doctor.Password + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // ����� Patients
            app.Map("/patients", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ��������� � �������
                    IPatientsService iPatientsService = context.RequestServices.GetService<IPatientsService>();
                    IEnumerable<Patient> patients = iPatientsService.GetPatients("Patients", 20);

                    string HtmlString = "<HTML><HEAD><TITLE>��������</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>������ ���������</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>���� ��������</TH>";
                    HtmlString += "<TH>���������� ����������</TH>";
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

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // ����� Diagnosis
            app.Map("/diagnosis", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ��������� � �������
                    IDiagnosisService iDiagnosisService = context.RequestServices.GetService<IDiagnosisService>();
                    IEnumerable<Diagnos> diagnosis = iDiagnosisService.GetDiagnosis("Diagnosis", 20);

                    string HtmlString = "<HTML><HEAD><TITLE>��������</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>������ ���������</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>�������</TH>";
                    HtmlString += "<TH>��������</TH>";
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

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // ����� Appointments
            app.Map("/appointments", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ��������� � �������
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

                    string HtmlString = "<HTML><HEAD><TITLE>������</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>������ �������</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>��� �������</TH>";
                    HtmlString += "<TH>��� ��������</TH>";
                    HtmlString += "<TH>���� ������</TH>";
                    HtmlString += "<TH>�������</TH>";
                    HtmlString += "</TR>";

                    foreach (var appointment in appointments)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + appointment.AppointmentId + "</TD>";
                        HtmlString += "<TD>" + doctorsDict.GetValueOrDefault(appointment.DoctorId, "����������") + "</TD>";
                        HtmlString += "<TD>" + patientsDict.GetValueOrDefault(appointment.PatientId, "����������") + "</TD>";
                        HtmlString += "<TD>" + appointment.AppointmentDate + "</TD>";
                        HtmlString += "<TD>" + diagnosisDict.GetValueOrDefault(appointment.DiagnosId, "����������") + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // ����� Medicaments
            app.Map("/medicaments", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ��������� � �������
                    IMedicamentsService iMedicamentsService = context.RequestServices.GetService<IMedicamentsService>();
                    IEnumerable<Medicament> medicaments = iMedicamentsService.GetMedicaments("Medicaments", 20);

                    string HtmlString = "<HTML><HEAD><TITLE>���������</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>������ ��������</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>��������</TH>";
                    HtmlString += "<TH>���������</TH>";
                    HtmlString += "<TH>���������</TH>";
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

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });


            // ����� Receptions
            app.Map("/receptions", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ��������� � �������
                    IReceptionsService iReceptionsService = context.RequestServices.GetService<IReceptionsService>();
                    IEnumerable<Reception> receptions = iReceptionsService.GetReceptions("Receptions", 20);

                    IMedicamentsService iMedicamentsService = context.RequestServices.GetService<IMedicamentsService>();
                    IEnumerable<Medicament> medicaments = iMedicamentsService.GetMedicaments("Medicaments", iMedicamentsService.GetMedicamentsCount());
                    Dictionary<int, string> medicamentsDict = medicaments.ToDictionary(m => m.MedicamentId, m => m.MedicamentName);

                    string HtmlString = "<HTML><HEAD><TITLE>�������</TITLE></HEAD>" +
                                        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                        "<BODY>" + PrintContentInTables() +
                                        "<H1>������ ��������</H1>" +
                                        "<TABLE BORDER=1>";

                    HtmlString += "<TR>";
                    HtmlString += "<TH>���</TH>";
                    HtmlString += "<TH>��� ������</TH>";
                    HtmlString += "<TH>���������</TH>";
                    HtmlString += "<TH>���������</TH>";
                    HtmlString += "</TR>";

                    foreach (var reception in receptions)
                    {
                        HtmlString += "<TR>";
                        HtmlString += "<TD>" + reception.ReceptionId + "</TD>";
                        HtmlString += "<TD>" + reception.AppointmentId + "</TD>";
                        HtmlString += "<TD>" + medicamentsDict.GetValueOrDefault(reception.MedicamentId, "����������") + "</TD>";
                        HtmlString += "<TD>" + reception.ReceptionDose + "</TD>";
                        HtmlString += "</TR>";
                    }
                    HtmlString += "</TABLE>";
                    HtmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(HtmlString);
                });
            });



            // ������ � Cookies



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

                    // ���������� ������ �� Cookies
                    string Doctor = context.Request.Cookies["DoctorId"];
                    int DoctorId = !string.IsNullOrEmpty(Doctor) ? int.Parse(Doctor) : 0;
                    string Patient = context.Request.Cookies["PatientId"];
                    int PatientId = !string.IsNullOrEmpty(Patient) ? int.Parse(Patient) : 0;
                    string Date = context.Request.Cookies["AppointmentDate"];
                    DateOnly AppointmentDate = string.IsNullOrEmpty(Date) ? DateOnly.FromDateTime(DateTime.Now) : DateOnly.Parse(Date);
                    int DiagnosId = SearchDiagnos(context, DoctorId, PatientId, AppointmentDate);

                    // ��������� ����� � ����������� �������
                    string strResponse = GenerateForm(context, DoctorId, PatientId, AppointmentDate, DiagnosId, "Cookies");

                    await context.Response.WriteAsync(strResponse);
                });
            });



            // ������ � Session



            app.Map("/searchform2", (appBuilder) => {
                appBuilder.Run(async (context) => {

                    // ���������� �� Session ������� Appointment
                    Appointment appointment = context.Session.Get<Appointment>("appointment") ?? new Appointment();
                    if (appointment.AppointmentDate == default)
                    {
                        appointment.AppointmentDate = DateOnly.FromDateTime(DateTime.Now);
                        appointment.DiagnosId = -1;
                    }
                    
                    // �������� ������ � Session
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



            // ��������� �������� � ����������� ������ ������� �� web-�������
            app.Run((context) =>
                {
                // ��������� � �������
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

                string HtmlString = "<HTML><HEAD><TITLE>���. �������</TITLE></HEAD>" +
                                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                    "<BODY><H1>�������</H1>";
                HtmlString += "<H2>��������� �������:</H2>";
                HtmlString += "<UL>";
                    HtmlString += "<LI><A href='/specializations'>�������������</A></LI>";
                    HtmlString += "<LI><A href='/doctors'>�����</A></LI>";
                    HtmlString += "<LI><A href='/patients'>��������</A></LI>";
                    HtmlString += "<LI><A href='/diagnosis'>��������</A></LI>";
                    HtmlString += "<LI><A href='/appointments'>������</A></LI>";
                    HtmlString += "<LI><A href='/medicaments'>���������</A></LI>";
                    HtmlString += "<LI><A href='/receptions'>�������</A></LI>";
                HtmlString += "</UL>";
                HtmlString += "<H2>���. ������:</H2>";
                HtmlString += "<UL>";
                    HtmlString += "<LI><A href='/searchform1'>������ � ������� (Cookies)</A></LI>";
                    HtmlString += "<LI><A href='/searchform2'>������ � ������� (Session)</A></LI>";
                HtmlString += "</UL>";
                HtmlString += "</BODY></HTML>";
                return context.Response.WriteAsync(HtmlString);
            });

            app.Run();
        }

        // ����� ����������
        static string PrintContentInTables()
        {
            string HtmlString = "<BR>";
            HtmlString += "<TABLE BORDER=1>";
                HtmlString += "<TR>";
                    HtmlString += "<TH style='padding: 0; width: 100px;'><A href='/'>�������</A></TH>";
                HtmlString += "</TR>";
            HtmlString += "</TABLE>";
            HtmlString += "<TABLE BORDER=1 style='margin-left: 53px; margin-top: 3px'>";
                HtmlString += "<TR>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/specializations'>�������������</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/doctors'>�����</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/patients'>��������</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/diagnosis'>��������</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/appointments'>������</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/medicaments'>���������</A></TH>";
                    HtmlString += "<TH style='border: 1px solid black; padding: 0;'><A href='/receptions'>�������</A></TH>";
                HtmlString += "</TR>";
            HtmlString += "</TABLE>";

            return HtmlString;
        }

        // ����� ��� ��������� ����������
        public static string GenerateForm(HttpContext context, int doctorId, int patientId, DateOnly appointmentDate, int diagnosId, string str)
        {
            var db = context.RequestServices.GetService<HospitalContext>();
            List<Diagnos> diagnosis = db.Diagnosis.ToList();
            diagnosis.Sort();
            List<Doctor> doctors = db.Doctors.ToList();
            doctors.Sort();
            List<Patient> patients = db.Patients.ToList();
            patients.Sort();

            string strResponse = $"<HTML><HEAD><TITLE>���������� ������ � {str}</TITLE></HEAD>" +
                                 "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                                 "<BODY><TABLE BORDER><TR><TH style='padding: 0; width: 100px;'><A href='/'>�������</A></TH></TR></TABLE>" +
                                 "<H1>������ ������:</H1><TABLE BORDER=1><TR><TH><BR><FORM style='margin-left:10px; margin-right:10px' method='POST'>" +
                                 "<input type='hidden' name='isSubmitted' value='true' />" +
                                 "��� �������:<BR><SELECT style='width: 400px;' padding: 0;' name='DoctorId'>";

            foreach (var doctor in doctors)
            {
                strResponse += $"<option value='{doctor.DoctorId}'" +
                               $"{(doctorId == doctor.DoctorId ? " selected" : "")}>" +
                               $"{doctor.DoctorLastName + " " + doctor.DoctorFirstName + " " + doctor.DoctorSurname}</option>";
            }

            strResponse += "</SELECT><BR><BR>" +
                           "��� ��������:<BR><SELECT style='width: 400px;' name='PatientId'>";

            foreach (var patient in patients)
            {
                strResponse += $"<option value='{patient.PatientId}'" +
                               $"{(patientId == patient.PatientId ? " selected" : "")}>" +
                               $"{patient.PatientLastName + " " + patient.PatientFirstName + " " + patient.PatientSurname}</option>";
            }

            strResponse += "</SELECT><BR><BR>" +
                           "���� ������:<BR><INPUT type='date' name='AppointmentDate' value='" + appointmentDate.ToString("yyyy-MM-dd") + "'><BR><BR>" +
                           "<BR><INPUT style='width: 200px;' type='submit' value='����� �������'>";

            if (diagnosId == -1)
                strResponse += "<P>������ �� �������</P>";
            else
            {
                bool flag = false;
                int i = 0;

                while (!flag && i < diagnosis.Count())
                {
                    if (diagnosis[i].DiagnosId == diagnosId)
                    {
                        flag = true;
                        strResponse += $"<P>�������: {diagnosis[i].DiagnosName}</P>";
                    }
                    i++;
                }
                if (!flag)
                {
                    strResponse += "<P>������ �� �������</P>";
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



using PatientManagement.Interfaces;
using PatientManagement.Models;
using PatientManagement.Repositories;
using PatientManagement.Services;
using PatientManagement.UI;

namespace PatientManagement
{
    public class Program
    {
        static void Main(string[] args)
        {
            IRepository<int, Appointment> appointmentRepository = new AppointmentRepository();

            IAppointmentService appointmentService = new AppointmentService(appointmentRepository);

            ManageAppointment appointmentUI = new ManageAppointment(appointmentService);

            appointmentUI.Run();

        }
    }
}

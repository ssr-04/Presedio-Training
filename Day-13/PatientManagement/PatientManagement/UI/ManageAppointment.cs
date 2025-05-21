using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatientManagement.Interfaces;
using PatientManagement.Models;

namespace PatientManagement.UI
{
    public class ManageAppointment
    {
        private readonly IAppointmentService _appointmentService;

        public ManageAppointment(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        private void DisplayMainMenu()
        {
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("1. Add New Appointment");
            Console.WriteLine("2. Search Appointments");
            Console.WriteLine("3. Exit");
            Console.Write("Enter your choice: ");
        }

        private void AddAppointment()
        {
            Console.WriteLine("\n--- Add New Appointment ---");
            Appointment newAppointment = new Appointment();

            Console.Write("Enter Patient Name: ");
            newAppointment.PatientName = Console.ReadLine() ?? string.Empty;

            Console.Write("Enter Patient Age: ");
            int age;
            while (!int.TryParse(Console.ReadLine(), out age) || age <= 0)
            {
                Console.Write("Invalid age. Please enter a positive integer: ");
            }
            newAppointment.PatientAge = age;

            Console.Write("Enter Appointment Date and Time (DD-MM-YYYY HH:MM) like (21-05-2025 15:30): ");
            DateTime appointmentDateTime;
            // paesing date
            while (!DateTime.TryParseExact(Console.ReadLine(), "dd-MM-yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out appointmentDateTime))
            {
                Console.Write("Invalid date/time format. Please use DD-MM-YYYY HH:MM (eg: 21-05-2025 15:30): ");
            }
            newAppointment.AppointmentDate = appointmentDateTime;

            Console.Write("Enter Reason for Visit: ");
            newAppointment.ReasonForVisit = Console.ReadLine() ?? string.Empty; 

            int newId = _appointmentService.AddAppointment(newAppointment);

            if (newId != -1)
            {
                Console.WriteLine($"Appointment added successfully! Appointment ID: {newId}");
            }
            else
            {
                // Error message already printed by the service layer
                Console.WriteLine("Failed to add appointment. Please review the details.");
            }
        }

        private void SearchAppointments()
        {
            Console.WriteLine("\n--- Search Appointments ---");
            AppointmentSearchModel searchModel = new AppointmentSearchModel();

            Console.WriteLine("Enter search criteria (leave blank for no filter):");

            Console.Write("Search by Patient Name: ");
            string? patientNameInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(patientNameInput))
            {
                searchModel.PatientName = patientNameInput;
            }

            Console.Write("Search by Appointment Date (DD-MM-YYYY): ");
            string? dateInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(dateInput) && DateTime.TryParseExact(dateInput, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime appDate))
            {
                searchModel.AppointmentDate = appDate.Date;
            }

            Console.WriteLine("Search by Patient Age Range:");
            Console.Write("  Minimum Age (integer): ");
            string? minAgeInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(minAgeInput) && int.TryParse(minAgeInput, out int minAge))
            {
                if (searchModel.AgeRange == null) searchModel.AgeRange = new Range<int>();
                searchModel.AgeRange.MinVal = minAge;
            }

            Console.Write("  Maximum Age (integer): ");
            string? maxAgeInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(maxAgeInput) && int.TryParse(maxAgeInput, out int maxAge))
            {
                if (searchModel.AgeRange == null) searchModel.AgeRange = new Range<int>();
                searchModel.AgeRange.MaxVal = maxAge;
            }

            List<Appointment>? searchResults = _appointmentService.SearchAppointments(searchModel);

            if (searchResults != null && searchResults.Any())
            {
                Console.WriteLine("\n--- Search Results ---");
                Console.WriteLine("{0,-5} {1,-20} {2,-5} {3,-20} {4,-30}", "ID", "Patient Name", "Age", "Appointment Date", "Reason");
                Console.WriteLine("----------------------------------------------------------------------------------");
                foreach (var appointment in searchResults)
                {
                    Console.WriteLine("{0,-5} {1,-20} {2,-5} {3,-20:dd-MM-yyyy HH:mm} {4,-30}",
                        appointment.Id,
                        appointment.PatientName,
                        appointment.PatientAge,
                        appointment.AppointmentDate,
                        appointment.ReasonForVisit);
                }
            }
            else
            {
                Console.WriteLine("No appointments found matching the criteria.");
            }
        }


        public void Run()
        {
            Console.WriteLine("Welcome to Appointment Management System!");

            bool exit = false;
            while (!exit)
            {
                DisplayMainMenu();
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddAppointment();
                        break;
                    case "2":
                        SearchAppointments();
                        break;
                    case "3":
                        Console.WriteLine("Exiting application. Bye!");
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }

        }
    }
}

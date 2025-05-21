using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatientManagement.Exceptions;
using PatientManagement.Models;

namespace PatientManagement.Repositories
{
    public class AppointmentRepository : Repository<int, Appointment>
    {
        public AppointmentRepository() : base()
        { 
        }

        protected override int GenerateID()
        {
            if(_items.Count == 0)
            {
                return 101;
            }
            else
            {
                return _items.Max(a => a.Id) + 1;
            }
        }

        public override ICollection<Appointment> GetAll()
        {
            if(_items.Count == 0)
            {
                throw new CollectionEmptyException("No appointments found in the collection.");
            }
            return _items;
        }

        public override Appointment GetById(int id)
        {
            var appointment = _items.FirstOrDefault(a => a.Id == id);
            if(appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with ID {id} not found.");

            }
            return appointment;
        }
    }
}

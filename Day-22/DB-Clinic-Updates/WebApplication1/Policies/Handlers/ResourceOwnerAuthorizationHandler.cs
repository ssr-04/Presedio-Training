using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Object>
{
    private readonly IUserRepository _userRepository;

    public ResourceOwnerAuthorizationHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    protected override async Task HandleRequirementAsync
    (AuthorizationHandlerContext context,
    OperationAuthorizationRequirement requirement,
    object resource)
    {
        //Admins are allowed to perform any operation
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        //Getting the UserId from the authenticated user's claim
        var UserIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier); //email in our case
        if (UserIdClaim == null)
        {
            return;
        }

        var user = await _userRepository.GetUserByUsernameAsync(UserIdClaim.Value);
        if (user == null)
        {
            return; //User not found in DB
        }

        // Handling resource
        if (resource is Patient patient)
        {
            // if user if Paient and associated PatientId matches resource's Id
            if (user.Role == "Patient" && user.PatientId.HasValue && user.PatientId.Value == patient.Id)
            {
                // if read/update their own profile
                if (requirement == ResourceOperations.Read || requirement == ResourceOperations.Update || requirement == ResourceOperations.Create)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }

        // Handling Doctor resource
        else if (resource is Doctor doctor)
        {
            // If the user is a Doctor and their associated DoctorId matches the resource's Id
            if (user.Role == "Doctor" && user.DoctorId.HasValue && user.DoctorId.Value == doctor.Id)
            {
                // Doctor can read/update their own profile
                if (requirement == ResourceOperations.Read || requirement == ResourceOperations.Update)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }

        else if (resource is AppointmentCreateDto appointmentdto)
        {
            if (user.Role == "Patient" && user.PatientId.HasValue && user.PatientId.Value == appointmentdto.PatientId)
            {
                // Patient can create, read, update (reschedule) their own appointments
                if (requirement == ResourceOperations.Read || requirement == ResourceOperations.Update || requirement == ResourceOperations.Create)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }

        // Handling Appointments
        else if (resource is Appointment appointment)
        {
            // Patient can interact with their own appointments
            if (user.Role == "Patient" && user.PatientId.HasValue && user.PatientId.Value == appointment.PatientId)
            {
                // Patient can create, read, update (reschedule) their own appointments
                if (requirement == ResourceOperations.Read || requirement == ResourceOperations.Update || requirement == ResourceOperations.Create)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
            // Doctor can interact with appointments assigned to them
            else if (user.Role == "Doctor" && user.DoctorId.HasValue && user.DoctorId.Value == appointment.DoctorId)
            {
                // Doctor can read, update (reschedule/status) their own appointments
                if (requirement == ResourceOperations.Read || requirement == ResourceOperations.Update)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }

        else if (resource is AppointmentResponseDto appointmentResponse)
        {
            // Patient can interact with their own appointments
            if (user.Role == "Patient" && user.PatientId.HasValue && appointmentResponse.Patient != null && user.PatientId.Value == appointmentResponse.Patient.Id)
            {
                // Patient can create, read, update (reschedule) their own appointments
                if (requirement == ResourceOperations.Read || requirement == ResourceOperations.Update || requirement == ResourceOperations.Create)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
            // Doctor can interact with appointments assigned to them
            else if (user.Role == "Doctor" && user.DoctorId.HasValue && appointmentResponse.Doctor != null && user.DoctorId.Value == appointmentResponse.Doctor.Id)
            {
                // Doctor can read, update (reschedule/status) their own appointments
                if (requirement == ResourceOperations.Read || requirement == ResourceOperations.Update)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
        
         // If no condition matched
        return;
    }
}
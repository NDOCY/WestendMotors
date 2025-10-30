// Services/AppointmentEmailService.cs
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Configuration;
using WestendMotors.Models;

namespace WestendMotors.Services
{
    public class AppointmentEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;
        private readonly string _fromEmail;

        public AppointmentEmailService()
        {
            // Get settings from Web.config
            _smtpHost = WebConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(WebConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            _smtpUsername = WebConfigurationManager.AppSettings["SmtpUsername"] ?? "***********";
            _smtpPassword = WebConfigurationManager.AppSettings["SmtpPassword"] ?? "**********";
            _enableSsl = bool.Parse(WebConfigurationManager.AppSettings["EnableSsl"] ?? "true");
            _fromEmail = WebConfigurationManager.AppSettings["FromEmail"] ?? "**************";
        }

        public async Task SendAppointmentStatusUpdateAsync(Appointment appointment, string oldStatus, string adminNotes = null)
        {
            if (appointment.Customer == null || string.IsNullOrEmpty(appointment.Customer.Email))
                return;

            var subject = $"Appointment #{appointment.AppointmentId} Status Update";
            var body = $@"
                <h3>Dear {appointment.Customer.FullName},</h3>
                <p>Your appointment status has been updated:</p>
                
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <p><strong>Appointment ID:</strong> #{appointment.AppointmentId}</p>
                    <p><strong>Type:</strong> {appointment.AppointmentType}</p>
                    <p><strong>Date & Time:</strong> {appointment.AppointmentDate:MMMM dd, yyyy at h:mm tt}</p>
                    <p><strong>Previous Status:</strong> {oldStatus}</p>
                    <p><strong>New Status:</strong> {appointment.Status}</p>
                    {(appointment.Vehicle != null ? $"<p><strong>Vehicle:</strong> {appointment.Vehicle.Title}</p>" : "")}
                    {(appointment.RescheduledDate.HasValue ? $"<p><strong>Rescheduled Date:</strong> {appointment.RescheduledDate.Value:MMMM dd, yyyy at h:mm tt}</p>" : "")}
                    {(!string.IsNullOrEmpty(adminNotes) ? $"<p><strong>Admin Notes:</strong> {adminNotes}</p>" : "")}
                </div>

                <p>Please log in to your account to view more details or contact us if you have any questions.</p>
                
                <br>
                <p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(appointment.Customer.Email, subject, body);
        }

        public async Task SendAppointmentConfirmationAsync(Appointment appointment)
        {
            if (appointment.Customer == null || string.IsNullOrEmpty(appointment.Customer.Email))
                return;

            var subject = $"Appointment #{appointment.AppointmentId} Confirmation";
            var body = $@"
                <h3>Dear {appointment.Customer.FullName},</h3>
                <p>Your appointment has been successfully booked!</p>
                
                <div style='background-color: #e8f5e8; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <p><strong>Appointment ID:</strong> #{appointment.AppointmentId}</p>
                    <p><strong>Type:</strong> {appointment.AppointmentType}</p>
                    <p><strong>Date & Time:</strong> {appointment.AppointmentDate:MMMM dd, yyyy at h:mm tt}</p>
                    <p><strong>Status:</strong> {appointment.Status}</p>
                    {(appointment.Vehicle != null ? $"<p><strong>Vehicle:</strong> {appointment.Vehicle.Title}</p>" : "")}
                    {(!string.IsNullOrEmpty(appointment.Notes) ? $"<p><strong>Your Notes:</strong> {appointment.Notes}</p>" : "")}
                </div>

                <p>We look forward to seeing you! Please arrive on time for your appointment.</p>
                
                <br>
                <p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(appointment.Customer.Email, subject, body);
        }

        public async Task SendRescheduleRequestAsync(Appointment originalAppointment, Appointment newAppointment, string rescheduleNotes)
        {
            if (originalAppointment.Customer == null || string.IsNullOrEmpty(originalAppointment.Customer.Email))
                return;

            var subject = $"Reschedule Request for Appointment #{originalAppointment.AppointmentId}";
            var body = $@"
                <h3>Dear {originalAppointment.Customer.FullName},</h3>
                <p>Your reschedule request has been received and is being processed.</p>
                
                <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <p><strong>Original Appointment:</strong></p>
                    <p>ID: #{originalAppointment.AppointmentId}</p>
                    <p>Date & Time: {originalAppointment.AppointmentDate:MMMM dd, yyyy at h:mm tt}</p>
                    
                    <p><strong>Requested New Appointment:</strong></p>
                    <p>Date & Time: {newAppointment.AppointmentDate:MMMM dd, yyyy at h:mm tt}</p>
                    <p><strong>Reason:</strong> {rescheduleNotes}</p>
                </div>

                <p>We will review your request and confirm the new appointment time shortly.</p>
                
                <br>
                <p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(originalAppointment.Customer.Email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(_fromEmail);
                    message.To.Add(toEmail);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                        smtpClient.EnableSsl = _enableSsl;

                        await smtpClient.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't break the application
                System.Diagnostics.Debug.WriteLine($"Email sending failed: {ex.Message}");
            }
        }

        public async Task SendAppointmentStaffAssignmentAsync(Appointment appointment, User staff)
        {
            if (appointment.Customer == null || string.IsNullOrEmpty(appointment.Customer.Email))
                return;

            var subject = $"Your Westend Motors Representative - Appointment #{appointment.AppointmentId}";
            var body = $@"
<h3>Dear {appointment.Customer.FullName},</h3>
<p>We're pleased to inform you that a representative has been assigned to assist you with your appointment.</p>

<div style='background-color: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0;'>
    <h4 style='color: #2c5aa0; margin-bottom: 15px;'>Your Assigned Representative</h4>
    
    <div style='display: flex; align-items: center; margin-bottom: 15px;'>
        <div style='background-color: #2c5aa0; color: white; width: 50px; height: 50px; border-radius: 50%; 
                    display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 18px; 
                    margin-right: 15px;'>
            {staff.FullName.Substring(0, 1).ToUpper()}
        </div>
        <div>
            <strong style='font-size: 18px;'>{staff.FullName}</strong>
            <br>
            <span style='color: #666;'>{staff.Title ?? "Appointment Specialist"}</span>
        </div>
    </div>
    
    <div style='background-color: white; padding: 15px; border-radius: 6px; border-left: 4px solid #2c5aa0;'>
        <p><strong>📧 Email:</strong> <a href='mailto:{staff.Email}' style='color: #2c5aa0; text-decoration: none;'>{staff.Email}</a></p>
        {(!string.IsNullOrEmpty(staff.Phone) ? $"<p><strong>📞 Phone:</strong> <a href='tel:{staff.Phone}' style='color: #2c5aa0; text-decoration: none;'>{staff.Phone}</a></p>" : "")}
        {(!string.IsNullOrEmpty(staff.Department) ? $"<p><strong>🏢 Department:</strong> {staff.Department}</p>" : "")}
    </div>
</div>

<div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
    <h4 style='color: #2c5aa0; margin-bottom: 15px;'>Appointment Details</h4>
    <p><strong>Appointment ID:</strong> #{appointment.AppointmentId}</p>
    <p><strong>Type:</strong> {appointment.AppointmentType}</p>
    <p><strong>Date & Time:</strong> {appointment.AppointmentDate:MMMM dd, yyyy at h:mm tt}</p>
    <p><strong>Status:</strong> {appointment.Status}</p>
    {(appointment.Vehicle != null ? $"<p><strong>Vehicle:</strong> {appointment.Vehicle.Title}</p>" : "")}
</div>

<p>Your representative {staff.FullName} will be your main point of contact for this appointment.</p>

<br>
<p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(appointment.Customer.Email, subject, body);
        }

        public async Task SendInternalStaffAssignmentNotificationAsync(Appointment appointment, User staff)
        {
            if (staff == null || string.IsNullOrEmpty(staff.Email))
                return;

            var subject = $"New Appointment Assignment: #{appointment.AppointmentId}";
            var body = $@"
<h3>Hello {staff.FullName},</h3>
<p>You have been assigned to assist with a new appointment.</p>

<div style='background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0;'>
    <h4 style='color: #856404; margin-bottom: 15px;'>Appointment Details</h4>
    
    <p><strong>Appointment ID:</strong> #{appointment.AppointmentId}</p>
    <p><strong>Customer:</strong> {appointment.Customer?.FullName}</p>
    <p><strong>Type:</strong> {appointment.AppointmentType}</p>
    <p><strong>Date & Time:</strong> {appointment.AppointmentDate:MMMM dd, yyyy at h:mm tt}</p>
    <p><strong>Status:</strong> {appointment.Status}</p>
    {(appointment.Vehicle != null ? $"<p><strong>Vehicle:</strong> {appointment.Vehicle.Title}</p>" : "")}
    {(!string.IsNullOrEmpty(appointment.Notes) ? $"<p><strong>Customer Notes:</strong> {appointment.Notes}</p>" : "")}
</div>

<div style='background-color: #e8f5e8; padding: 15px; border-radius: 8px; margin: 20px 0;'>
    <h4 style='color: #2c5aa0; margin-bottom: 15px;'>Customer Contact Information</h4>
    <p><strong>Email:</strong> {appointment.Customer?.Email}</p>
    {(!string.IsNullOrEmpty(appointment.Customer?.Phone) ? $"<p><strong>Phone:</strong> {appointment.Customer.Phone}</p>" : "")}
</div>

<p>Please reach out to the customer within 24 hours to confirm the appointment details.</p>

<br>
<p>Best regards,<br>Westend Motors Management</p>";

            await SendEmailAsync(staff.Email, subject, body);
        }
    }
}
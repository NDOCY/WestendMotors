// Services/VehicleAssignmentEmailService.cs
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Configuration;
using WestendMotors.Models;

namespace WestendMotors.Services
{
    public class VehicleAssignmentEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;
        private readonly string _fromEmail;

        public VehicleAssignmentEmailService()
        {
            // Get settings from Web.config
            _smtpHost = WebConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(WebConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            _smtpUsername = WebConfigurationManager.AppSettings["SmtpUsername"] ?? "**************";
            _smtpPassword = WebConfigurationManager.AppSettings["SmtpPassword"] ?? "*************";
            _enableSsl = bool.Parse(WebConfigurationManager.AppSettings["EnableSsl"] ?? "true");
            _fromEmail = WebConfigurationManager.AppSettings["FromEmail"] ?? "***************";
        }

        public async Task SendVehicleAssignmentAsync(User user, Vehicle vehicle, UserVehicle userVehicle, ServiceSchedule serviceSchedule)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || vehicle == null)
                return;

            var subject = $"Vehicle Assigned: {vehicle.Title}";
            var body = $@"
                <h3>Dear {user.FullName},</h3>
                <p>Congratulations! A vehicle has been assigned to your account.</p>
                
                <div style='background-color: #e8f5e8; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <p><strong>Vehicle:</strong> {vehicle.Title}</p>
                    <p><strong>Purchase Date:</strong> {userVehicle.PurchaseDate:MMMM dd, yyyy}</p>
                    <p><strong>Assigned On:</strong> {DateTime.Now:MMMM dd, yyyy}</p>
                    {(vehicle.Specs != null ? $"<p><strong>Year:</strong> {vehicle.Specs.Year}</p>" : "")}
                    {(vehicle.Specs != null ? $"<p><strong>Mileage:</strong> {vehicle.Specs.Mileage:N0} miles</p>" : "")}
                    {(!string.IsNullOrEmpty(userVehicle.Notes) ? $"<p><strong>Assignment Notes:</strong> {userVehicle.Notes}</p>" : "")}
                </div>";

            // Add service schedule information if available
            if (serviceSchedule != null)
            {
                body += $@"
                <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <h4>Service Schedule</h4>
                    <p><strong>Next Service Date:</strong> {serviceSchedule.NextServiceDate:MMMM dd, yyyy}</p>
                    <p><strong>Recurrence:</strong> {serviceSchedule.RecurrenceType}</p>
                    {(!string.IsNullOrEmpty(serviceSchedule.Notes) ? $"<p><strong>Service Notes:</strong> {serviceSchedule.Notes}</p>" : "")}
                </div>";
            }

            body += $@"
                <p>You can now view this vehicle in your account dashboard and manage its service schedule.</p>
                
                <br>
                <p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendVehicleUnassignmentAsync(User user, Vehicle vehicle, string reason = null)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || vehicle == null)
                return;

            var subject = $"Vehicle Unassigned: {vehicle.Title}";
            var body = $@"
                <h3>Dear {user.FullName},</h3>
                <p>The following vehicle has been unassigned from your account:</p>
                
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <p><strong>Vehicle:</strong> {vehicle.Title}</p>
                    <p><strong>Unassigned On:</strong> {DateTime.Now:MMMM dd, yyyy}</p>
                    {(!string.IsNullOrEmpty(reason) ? $"<p><strong>Reason:</strong> {reason}</p>" : "")}
                </div>

                <p>If you believe this is an error, please contact our support team.</p>
                
                <br>
                <p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendServiceReminderAsync(User user, Vehicle vehicle, ServiceSchedule serviceSchedule)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || vehicle == null || serviceSchedule == null)
                return;

            var subject = $"Service Reminder: {vehicle.Title}";
            var daysUntilService = (serviceSchedule.NextServiceDate - DateTime.Today).Days;

            var body = $@"
                <h3>Dear {user.FullName},</h3>
                <p>This is a friendly reminder about your upcoming vehicle service:</p>
                
                <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <p><strong>Vehicle:</strong> {vehicle.Title}</p>
                    <p><strong>Next Service Date:</strong> {serviceSchedule.NextServiceDate:MMMM dd, yyyy}</p>
                    <p><strong>Days Remaining:</strong> {daysUntilService} day{(daysUntilService != 1 ? "s" : "")}</p>
                    <p><strong>Service Type:</strong> {serviceSchedule.RecurrenceType} Maintenance</p>
                    {(!string.IsNullOrEmpty(serviceSchedule.Notes) ? $"<p><strong>Notes:</strong> {serviceSchedule.Notes}</p>" : "")}
                </div>

                <p>Please schedule your service appointment at your earliest convenience.</p>
                
                <br>
                <p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(user.Email, subject, body);
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
    }
}
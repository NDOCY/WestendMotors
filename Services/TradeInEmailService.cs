// Services/TradeInEmailService.cs
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Configuration;
using WestendMotors.Models;

namespace WestendMotors.Services
{
    public class TradeInEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;
        private readonly string _fromEmail;

        public TradeInEmailService()
        {
            // Get settings from Web.config
            _smtpHost = WebConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(WebConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            _smtpUsername = WebConfigurationManager.AppSettings["SmtpUsername"] ?? "***********";
            _smtpPassword = WebConfigurationManager.AppSettings["SmtpPassword"] ?? "************";
            _enableSsl = bool.Parse(WebConfigurationManager.AppSettings["EnableSsl"] ?? "true");
            _fromEmail = WebConfigurationManager.AppSettings["FromEmail"] ?? "***************";
        }

        public async Task SendTradeInStatusUpdateAsync(TradeInRequest tradeIn, string oldStatus, string adminNotes = null)
        {
            if (tradeIn.Customer == null || string.IsNullOrEmpty(tradeIn.Customer.Email))
                return;

            var subject = $"Trade-In Request #{tradeIn.TradeInRequestId} Status Update";
            var body = $@"
                <h3>Dear {tradeIn.Customer.FullName},</h3>
                <p>Your trade-in request status has been updated:</p>
                
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <p><strong>Trade-In ID:</strong> #{tradeIn.TradeInRequestId}</p>
                    <p><strong>Vehicle:</strong> {tradeIn.Year} {tradeIn.Make} {tradeIn.Model}</p>
                    <p><strong>Previous Status:</strong> {oldStatus}</p>
                    <p><strong>New Status:</strong> {tradeIn.Status}</p>
                    {(tradeIn.FinalOffer.HasValue ? $"<p><strong>Final Offer:</strong> {tradeIn.FinalOffer.Value:C}</p>" : "")}
                    {(tradeIn.ScheduledAppointment.HasValue ? $"<p><strong>Scheduled Appointment:</strong> {tradeIn.ScheduledAppointment.Value:MMMM dd, yyyy at h:mm tt}</p>" : "")}
                    {(!string.IsNullOrEmpty(adminNotes) ? $"<p><strong>Admin Notes:</strong> {adminNotes}</p>" : "")}
                </div>

                <p>Please log in to your account to view more details or contact us if you have any questions.</p>
                
                <br>
                <p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(tradeIn.Customer.Email, subject, body);
        }

        public async Task SendTradeInAppointmentScheduledAsync(TradeInRequest tradeIn, DateTime appointmentDate, string notes = null)
        {
            if (tradeIn.Customer == null || string.IsNullOrEmpty(tradeIn.Customer.Email))
                return;

            var subject = $"Appointment Scheduled for Trade-In #{tradeIn.TradeInRequestId}";
            var body = $@"
                <h3>Dear {tradeIn.Customer.FullName},</h3>
                <p>An appointment has been scheduled for your trade-in request:</p>
                
                <div style='background-color: #e8f5e8; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <p><strong>Trade-In ID:</strong> #{tradeIn.TradeInRequestId}</p>
                    <p><strong>Vehicle:</strong> {tradeIn.Year} {tradeIn.Make} {tradeIn.Model}</p>
                    <p><strong>Appointment Date:</strong> {appointmentDate:MMMM dd, yyyy at h:mm tt}</p>
                    <p><strong>Final Offer:</strong> {(tradeIn.FinalOffer?.ToString("C") ?? "To be determined")}</p>
                    {(!string.IsNullOrEmpty(notes) ? $"<p><strong>Notes:</strong> {notes}</p>" : "")}
                </div>

                <p>Please bring your vehicle and all relevant documentation to the appointment.</p>
                
                <br>
                <p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(tradeIn.Customer.Email, subject, body);
        }

        public async Task SendTradeInConvertedAsync(TradeInRequest tradeIn, Vehicle vehicle, bool assignedToCustomer)
        {
            if (tradeIn.Customer == null || string.IsNullOrEmpty(tradeIn.Customer.Email))
                return;

            var subject = $"Your Trade-In Has Been Converted to Vehicle Listing";
            var body = $@"
                <h3>Dear {tradeIn.Customer.FullName},</h3>
                <p>Great news! Your trade-in request has been successfully processed.</p>
                
                <div style='background-color: #e8f5e8; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <p><strong>Trade-In ID:</strong> #{tradeIn.TradeInRequestId}</p>
                    <p><strong>Your Vehicle:</strong> {tradeIn.Year} {tradeIn.Make} {tradeIn.Model}</p>
                    <p><strong>Final Offer:</strong> {(tradeIn.FinalOffer?.ToString("C") ?? "N/A")}</p>
                    <p><strong>New Vehicle Listing:</strong> {vehicle.Title}</p>
                    <p><strong>Listing Price:</strong> {vehicle.Price:C}</p>
                    {(assignedToCustomer ? "<p><strong>Status:</strong> Assigned to your account</p>" : "<p><strong>Status:</strong> Available for sale</p>")}
                </div>

                {(assignedToCustomer ?
                "<p>The vehicle has been added to your account. You can view it in your dashboard.</p>" :
                "<p>The vehicle is now available for sale in our inventory.</p>")}
                
                <br>
                <p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(tradeIn.Customer.Email, subject, body);
        }

        public async Task SendTradeInConfirmationAsync(TradeInRequest tradeIn)
        {
            if (tradeIn.Customer == null || string.IsNullOrEmpty(tradeIn.Customer.Email))
                return;

            var subject = $"Trade-In Request #{tradeIn.TradeInRequestId} Confirmation";
            var body = $@"
                <h3>Dear {tradeIn.Customer.FullName},</h3>
                <p>Thank you for submitting your trade-in request!</p>
                
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <p><strong>Trade-In ID:</strong> #{tradeIn.TradeInRequestId}</p>
                    <p><strong>Vehicle:</strong> {tradeIn.Year} {tradeIn.Make} {tradeIn.Model}</p>
                    <p><strong>Estimated Value:</strong> {(tradeIn.EstimatedValue?.ToString("C") ?? "To be determined")}</p>
                    <p><strong>Status:</strong> {tradeIn.Status}</p>
                    <p><strong>Submission Date:</strong> {tradeIn.RequestDate:MMMM dd, yyyy}</p>
                </div>

                <p>We will review your request and get back to you within 2 business days. You can check the status of your request by logging into your account.</p>
                
                <br>
                <p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(tradeIn.Customer.Email, subject, body);
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

        public async Task SendTradeInStaffAssignmentAsync(TradeInRequest tradeIn, User staff)
        {
            if (tradeIn.Customer == null || string.IsNullOrEmpty(tradeIn.Customer.Email) || staff == null)
                return;

            var subject = $"Your Westend Motors Representative - Trade-In #{tradeIn.TradeInRequestId}";
            var body = $@"
        <h3>Dear {tradeIn.Customer.FullName},</h3>
        <p>We're pleased to inform you that a dedicated representative has been assigned to assist you with your trade-in request.</p>
        
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
                    <span style='color: #666;'>{staff.Title ?? "Trade-In Specialist"}</span>
                </div>
            </div>
            
            <div style='background-color: white; padding: 15px; border-radius: 6px; border-left: 4px solid #2c5aa0;'>
                <p><strong>📧 Email:</strong> <a href='mailto:{staff.Email}' style='color: #2c5aa0; text-decoration: none;'>{staff.Email}</a></p>
                {(!string.IsNullOrEmpty(staff.Phone) ? $"<p><strong>📞 Phone:</strong> <a href='tel:{staff.Phone}' style='color: #2c5aa0; text-decoration: none;'>{staff.Phone}</a></p>" : "")}
                {(!string.IsNullOrEmpty(staff.Department) ? $"<p><strong>🏢 Department:</strong> {staff.Department}</p>" : "")}
            </div>
        </div>

        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
            <h4 style='color: #2c5aa0; margin-bottom: 15px;'>Your Trade-In Details</h4>
            <p><strong>Trade-In ID:</strong> #{tradeIn.TradeInRequestId}</p>
            <p><strong>Vehicle:</strong> {tradeIn.Year} {tradeIn.Make} {tradeIn.Model}</p>
            <p><strong>Status:</strong> {tradeIn.Status}</p>
            {(tradeIn.ScheduledAppointment.HasValue ? $"<p><strong>Appointment:</strong> {tradeIn.ScheduledAppointment.Value:MMMM dd, yyyy at h:mm tt}</p>" : "")}
        </div>

        <p>Your representative {staff.FullName} will be your main point of contact throughout the trade-in process.</p>
        
        <br>
        <p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(tradeIn.Customer.Email, subject, body);
        }

        public async Task SendInternalAssignmentNotificationAsync(TradeInRequest tradeIn, User staff)
        {
            if (staff == null || string.IsNullOrEmpty(staff.Email))
                return;

            var subject = $"New Trade-In Assignment: #{tradeIn.TradeInRequestId}";
            var body = $@"
        <h3>Hello {staff.FullName},</h3>
        <p>You have been assigned to assist with a new trade-in request.</p>
        
        <div style='background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0;'>
            <h4 style='color: #856404; margin-bottom: 15px;'>Trade-In Details</h4>
            
            <p><strong>Trade-In ID:</strong> #{tradeIn.TradeInRequestId}</p>
            <p><strong>Customer:</strong> {tradeIn.Customer?.FullName}</p>
            <p><strong>Vehicle:</strong> {tradeIn.Year} {tradeIn.Make} {tradeIn.Model}</p>
            <p><strong>Status:</strong> {tradeIn.Status}</p>
            <p><strong>Estimated Value:</strong> {(tradeIn.EstimatedValue?.ToString("C") ?? "To be determined")}</p>
            {(tradeIn.ScheduledAppointment.HasValue ? $"<p><strong>Appointment:</strong> {tradeIn.ScheduledAppointment.Value:MMMM dd, yyyy at h:mm tt}</p>" : "")}
            {(!string.IsNullOrEmpty(tradeIn.ConditionNotes) ? $"<p><strong>Condition Notes:</strong> {tradeIn.ConditionNotes}</p>" : "")}
        </div>

        <div style='background-color: #e8f5e8; padding: 15px; border-radius: 8px; margin: 20px 0;'>
            <h4 style='color: #2c5aa0; margin-bottom: 15px;'>Customer Contact Information</h4>
            <p><strong>Email:</strong> {tradeIn.Customer?.Email}</p>
            {(!string.IsNullOrEmpty(tradeIn.Customer?.Phone) ? $"<p><strong>Phone:</strong> {tradeIn.Customer.Phone}</p>" : "")}
        </div>

        <p>Please reach out to the customer within 24 hours to introduce yourself and discuss next steps.</p>
        
        <br>
        <p>Best regards,<br>Westend Motors Management</p>";

            await SendEmailAsync(staff.Email, subject, body);
        }

        public async Task SendTradeInAdminAssignmentAsync(TradeInRequest tradeIn, User admin)
        {
            // Send email to customer notifying them of admin assignment
            if (tradeIn.Customer == null || string.IsNullOrEmpty(tradeIn.Customer.Email))
                return;

            var subject = $"Your Westend Motors Administrator - Trade-In #{tradeIn.TradeInRequestId}";
            var body = $@"
<h3>Dear {tradeIn.Customer.FullName},</h3>
<p>We're pleased to inform you that an Administrator has been assigned to assist you with your trade-in request.</p>

<div style='background-color: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0;'>
    <h4 style='color: #2c5aa0; margin-bottom: 15px;'>Your Assigned Administrator</h4>
    
    <div style='display: flex; align-items: center; margin-bottom: 15px;'>
        <div style='background-color: #2c5aa0; color: white; width: 50px; height: 50px; border-radius: 50%; 
                    display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 18px; 
                    margin-right: 15px;'>
            {admin.FullName.Substring(0, 1).ToUpper()}
        </div>
        <div>
            <strong style='font-size: 18px;'>{admin.FullName}</strong>
            <br>
            <span style='color: #666;'>Administrator</span>
        </div>
    </div>
</div>

<p>Your administrator {admin.FullName} will be your main point of contact throughout the trade-in process.</p>

<br>
<p>Best regards,<br>Westend Motors Team</p>";

            await SendEmailAsync(tradeIn.Customer.Email, subject, body);
        }

        public async Task SendInternalAdminAssignmentNotificationAsync(TradeInRequest tradeIn, User admin)
        {
            if (admin == null || string.IsNullOrEmpty(admin.Email))
                return;

            var subject = $"New Trade-In Administrator Assignment: #{tradeIn.TradeInRequestId}";
            var body = $@"
<h3>Hello {admin.FullName},</h3>
<p>You have been assigned as the Administrator for a trade-in request.</p>

<div style='background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0;'>
    <h4 style='color: #856404; margin-bottom: 15px;'>Trade-In Details</h4>
    
    <p><strong>Trade-In ID:</strong> #{tradeIn.TradeInRequestId}</p>
    <p><strong>Customer:</strong> {tradeIn.Customer?.FullName}</p>
    <p><strong>Vehicle:</strong> {tradeIn.Year} {tradeIn.Make} {tradeIn.Model}</p>
    <p><strong>Status:</strong> {tradeIn.Status}</p>
    <p><strong>Estimated Value:</strong> {(tradeIn.EstimatedValue?.ToString("C") ?? "To be determined")}</p>
</div>

<p>Please contact the customer to discuss the trade-in process and schedule any necessary inspections.</p>

<br>
<p>Best regards,<br>Westend Motors Management</p>";

            await SendEmailAsync(admin.Email, subject, body);
        }
    }
}
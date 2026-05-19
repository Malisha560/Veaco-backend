using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Veaco.api.Services;

// uses of gmail SMTP from mailkit library
// All controllers that need to send emails will inject this service
public class EmailService
{
    // connected bhako mail
    private readonly string _fromEmail = "veaco73@gmail.com";
    private readonly string _fromName = "Veaco System";
    private readonly string _appPassword = "zhml ocks chdo hfgk";

   
    // Core method jasle connects garcha Gmail lai ani sends the email
    // sab aru method in this service calls yo 

    private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        // concept of making the email as html
        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        // Connect to Gmail SMTP, authenticate, send, then disconnect
        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_fromEmail, _appPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

 
    //  Send invoice email to a customer wala feature
    // sales invoice create bhayesi call garcha
    // Includes all item details, subtotal, discount, and grand total

    public async Task SendInvoiceEmailAsync(
        string toEmail,
        string customerName,
        int invoiceId,
        decimal subTotal,
        decimal discountAmount,
        decimal grandTotal,
        List<string> itemDescriptions)
    {
        var subject = $"Your Invoice #{invoiceId} from Veaco";

        // Build each item as an HTML table row
        var itemRows = string.Join("", itemDescriptions.Select(item =>
            $"<tr><td style='padding:8px;border-bottom:1px solid #eee;'>{item}</td></tr>"));

        var htmlBody = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                <h2 style='background:#0b1b35;color:white;padding:20px;margin:0;'>Veaco - Sales Invoice</h2>
                <div style='padding:20px;'>
                    <p>Dear <strong>{customerName}</strong>,</p>
                    <p>Thank you for your purchase! Here is your invoice summary:</p>

                    <h3>Invoice #{invoiceId}</h3>
                    <table style='width:100%;border-collapse:collapse;'>
                        <thead>
                            <tr style='background:#f5f5f5;'>
                                <th style='padding:8px;text-align:left;'>Item</th>
                            </tr>
                        </thead>
                        <tbody>{itemRows}</tbody>
                    </table>

                    <div style='margin-top:20px;text-align:right;'>
                        <p>Subtotal: <strong>Rs. {subTotal:F2}</strong></p>
                        <p>Discount: <strong>Rs. {discountAmount:F2}</strong></p>
                        <p style='font-size:18px;color:#0b1b35;'>Grand Total: <strong>Rs. {grandTotal:F2}</strong></p>
                    </div>

                    <p style='margin-top:30px;color:#666;'>If you have any questions, please contact us.</p>
                    <p>Thank you for choosing Veaco!</p>
                </div>
            </div>";

        await SendEmailAsync(toEmail, customerName, subject, htmlBody);
    }


    // Send low stock alert email to admin wala feature
    // Called when a vehicle part has stock below 10 units

    public async Task SendLowStockNotificationAsync(string partName, int currentStock)
    {
        var subject = $"Low Stock Alert: {partName}";

        var htmlBody = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                <h2 style='background:#dc2626;color:white;padding:20px;margin:0;'>⚠ Low Stock Alert</h2>
                <div style='padding:20px;'>
                    <p>This is an automated notification from the Veaco system.</p>
                    <p>The following part has fallen below the minimum stock level of 10 units:</p>
                    <div style='background:#fee2e2;padding:16px;border-radius:8px;margin:20px 0;'>
                        <p><strong>Part Name:</strong> {partName}</p>
                        <p><strong>Current Stock:</strong> {currentStock} units remaining</p>
                    </div>
                    <p>Please restock this item as soon as possible.</p>
                </div>
            </div>";

        // Send the alert to the admin email (same as sender in this case)
        await SendEmailAsync(_fromEmail, "Veaco Admin", subject, htmlBody);
    }

    
    //  unpaid credit reminder email to a customer wala feature
    // Called for customers whose credit balance has been unpaid for more than 1 month
    
    public async Task SendCreditReminderEmailAsync(string toEmail, string customerName, int creditBalance)
    {
        var subject = "Payment Reminder - Outstanding Credit Balance";

        var htmlBody = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                <h2 style='background:#d97706;color:white;padding:20px;margin:0;'>Payment Reminder</h2>
                <div style='padding:20px;'>
                    <p>Dear <strong>{customerName}</strong>,</p>
                    <p>This is a friendly reminder that you have an outstanding credit balance with Veaco that is overdue by more than one month.</p>
                    <div style='background:#fef3c7;padding:16px;border-radius:8px;margin:20px 0;'>
                        <p><strong>Outstanding Balance:</strong> Rs. {creditBalance}</p>
                        <p><strong>Status:</strong> Overdue (more than 1 month)</p>
                    </div>
                    <p>Please settle your balance at your earliest convenience.</p>
                    <p>If you have already made the payment, please ignore this message.</p>
                    <p style='margin-top:30px;'>Thank you,<br/><strong>Veaco Team</strong></p>
                </div>
            </div>";

        await SendEmailAsync(toEmail, customerName, subject, htmlBody);
    }
}
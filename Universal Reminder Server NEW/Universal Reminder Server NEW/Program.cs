using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net.Mail;
using System.Net;

namespace Universal_Reminder_Server_NEW
{
    class Program
    {
        static List<String> reminders;
        static List<DateTime> reminderDates;
        static List<String> reminderTitles;
        static List<String> emails;
        static List<String> phoneNumbers;

        static void Main(string[] args)
        {
            reminderTitles = new List<String>();
            reminders = new List<String>();
            reminderDates = new List<DateTime>();
            emails = new List<String>();
            phoneNumbers = new List<String>();

            ThreadStart clientStart = new ThreadStart(AcceptClients);
            Thread clientThread = new Thread(clientStart);

            ThreadStart remindStart = new ThreadStart(CheckForReminding);
            Thread remindThread = new Thread(remindStart);

            clientThread.Start();
            remindThread.Start();
        }

        static void AcceptClients()
        {
            TcpListener listener = new TcpListener(9000);

            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                StreamReader reader = new StreamReader(client.GetStream());
                StreamWriter writer = new StreamWriter(client.GetStream());

                try
                {
                    Console.WriteLine("Client connected...");

                    //Send response to verify connection
                    byte[] toBeSent = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK");
                    writer.BaseStream.Write(toBeSent, 0, toBeSent.Length);
                    writer.Flush();

                    //Wait for title
                    byte[] buffer = new byte[client.Client.ReceiveBufferSize];
                    client.Client.Receive(buffer);
                    String response = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', '\0' });
                    Console.WriteLine("Title received...");
                    Console.WriteLine("Title: " + response);
                    reminderTitles.Add(response);

                    //Send response to verify title
                    toBeSent = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK");
                    writer.BaseStream.Write(toBeSent, 0, toBeSent.Length);
                    writer.Flush();

                    //Wait for contents
                    buffer = new byte[client.Client.ReceiveBufferSize];
                    client.Client.Receive(buffer);
                    response = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', '\0' });
                    Console.WriteLine("Contents received...");
                    Console.WriteLine("Contents: " + response);
                    reminders.Add(response);

                    //Send response to verify contents
                    toBeSent = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK");
                    writer.BaseStream.Write(toBeSent, 0, toBeSent.Length);
                    writer.Flush();

                    //Wait for urgency
                    buffer = new byte[client.Client.ReceiveBufferSize];
                    client.Client.Receive(buffer);
                    response = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', '\0' });
                    Console.WriteLine("Urgency received...");
                    Console.WriteLine("Urgency: " + response);

                    //Send response to verify urgency
                    toBeSent = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK");
                    writer.BaseStream.Write(toBeSent, 0, toBeSent.Length);
                    writer.Flush();

                    //Wait for date
                    buffer = new byte[client.Client.ReceiveBufferSize];
                    client.Client.Receive(buffer);
                    response = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', '\0' });
                    Console.WriteLine("Date received...");
                    Console.WriteLine("Date: " + response);

                    DateTime datePortion = Convert.ToDateTime(response);

                    //Send response to verify date
                    toBeSent = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK");
                    writer.BaseStream.Write(toBeSent, 0, toBeSent.Length);
                    writer.Flush();

                    //Wait for time
                    buffer = new byte[client.Client.ReceiveBufferSize];
                    client.Client.Receive(buffer);
                    response = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', '\0' });
                    Console.WriteLine("Time received...");
                    Console.WriteLine("Time: " + response);

                    DateTime timePortion = Convert.ToDateTime(response);
                    reminderDates.Add(new DateTime(datePortion.Year, datePortion.Month, datePortion.Day, timePortion.Hour, timePortion.Minute, timePortion.Second));

                    //Send response to verify time
                    toBeSent = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK");
                    writer.BaseStream.Write(toBeSent, 0, toBeSent.Length);
                    writer.Flush();

                    //Wait for email
                    buffer = new byte[client.Client.ReceiveBufferSize];
                    client.Client.Receive(buffer);
                    response = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', '\0' });
                    Console.WriteLine("Email received...");
                    Console.WriteLine("Email: " + response);
                    emails.Add(response);

                    //Send response to verify email
                    toBeSent = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK");
                    writer.BaseStream.Write(toBeSent, 0, toBeSent.Length);
                    writer.Flush();

                    //Wait for phone number
                    buffer = new byte[client.Client.ReceiveBufferSize];
                    client.Client.Receive(buffer);
                    response = Encoding.ASCII.GetString(buffer).TrimEnd(new char[] { '\n', '\r', '\0' });
                    Console.WriteLine("Phone number received...");
                    Console.WriteLine("Phone number: " + response);
                    phoneNumbers.Add(response);

                    //Send response to verify phone number
                    toBeSent = Encoding.ASCII.GetBytes("HTTP/1.0 200 OK");
                    writer.BaseStream.Write(toBeSent, 0, toBeSent.Length);
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                try
                {
                    reader.Close();
                    writer.Close();
                }
                catch (Exception ex) { }
            }
        }

        static void CheckForReminding()
        {
            while (true)
            {
                for (int i = 0; i < reminderDates.Count; i++)
                {
                    if (reminderDates[i].Year == DateTime.Now.Year && reminderDates[i].Month == DateTime.Now.Month && reminderDates[i].Day == DateTime.Now.Day &&
                        reminderDates[i].Hour == DateTime.Now.Hour && reminderDates[i].Minute == DateTime.Now.Minute)
                    {
                        Console.WriteLine("Reminder: " + reminderTitles[i]);

                        if (emails[i] != String.Empty)
                        {
                            try
                            {
                                SmtpClient client = new SmtpClient("smtp.gmail.com");

                                MailMessage message = new MailMessage();
                                message.To.Add(emails[i]);
                                message.Subject = reminderTitles[i];
                                message.Body = reminders[i];
                                message.From = new MailAddress("universalreminderapp@gmail.com");

                                client.Port = 587;
                                client.UseDefaultCredentials = false;
                                client.Credentials = new NetworkCredential("universalreminderapp@gmail.com", "universalReminder2017");
                                client.EnableSsl = true;
                                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                                client.Send(message);

                                Console.WriteLine("Email sent..");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }

                        reminderTitles.RemoveAt(i);
                        reminders.RemoveAt(i);
                        reminderDates.RemoveAt(i);
                        emails.RemoveAt(i);
                        phoneNumbers.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
}
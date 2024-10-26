﻿using Libreria_Elia_V0._0.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;


namespace Libreria_Elia_V0._0.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private MySqlConnection HomeDbConn;
        private readonly string _smtpServer;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            HomeDbConn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            _smtpUsername = _configuration["EmailSettings:Username"];
            _smtpPassword = _configuration["EmailSettings:Password"];

        }
        public IActionResult Index(int value)
        {
            switch (value)
            {
                case 1:
                    HttpContext.Session.Clear();
                    return View();
                case 2:
                    HttpContext.Session.Clear();
                    return View();
                default:
                    return View();
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult RegForm()
        {
            return View();
        }
        public IActionResult LogForm()
        {
            return View();
        }
        public IActionResult EmailWarning()
        {
            return View();
        }

        private bool MailExists(string mailToCheck)
        {
            string query = "SELECT users.email FROM users where users.email = @mail union all SELECT ghost_users.email from ghost_users where ghost_users.email = @mail;";
            using (HomeDbConn)
            using (MySqlCommand cmd = new MySqlCommand(query, HomeDbConn))
            {
                cmd.Parameters.AddWithValue("@mail", mailToCheck.Trim());

                try
                {
                    HomeDbConn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            HomeDbConn.Close();
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error
                }
                finally
                {
                    HomeDbConn.Close();
                }
            }
            return false;
        }




        public bool SendVerificationMail(string rAdress)
        {
            string senderAddress = _smtpUsername;
            string recipientAddress = rAdress;  //indirizzo del destinatario @mail.com
            string subject = "no-reply";
            string body = "Richiesta di registrazione presa in considerazione dall'admin.\nTi invieremo una email quando la richiesta sarà approvata.";

            //credenziali SMTP per il mittente
            string smtpServer = _smtpServer;
            string username = _smtpUsername;
            string password = _smtpPassword;

            SmtpClient smtpClient = new SmtpClient(smtpServer, 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(username, password);

            MailMessage verificationEmail = new MailMessage(senderAddress, recipientAddress, subject, body);

            try
            {
                if (!MailExists(rAdress))
                {
                    smtpClient.Send(verificationEmail);
                    ViewBag.mailError = "Verification mail sent";
                    return true;
                }
                else
                {
                    ViewBag.signUpError = "Indirizzo eMail già in uso";
                    return false;
                }
            }
            catch (Exception ex)
            {
                //Error Log
                Console.WriteLine(ex.Message);
                ViewBag.signUpError = $"An error occurred:\n{ex.Message}";
                return false;
            }
        }
        public IActionResult ProfileMainPage()
        {
            switch (HttpContext.Session.GetInt32("UserTypeLogged"))
            {
                case 1:
                    return RedirectToAction("AdminMainPage", "Admin", new LoggedUser(HttpContext.Session.GetString("AdminMail"), HttpContext.Session.GetString("AdminPassword")));
                case 0:
                    return RedirectToAction("UserMainPage", "User", new LoggedUser(HttpContext.Session.GetString("UserMail"), HttpContext.Session.GetString("UserPwd")));
                default:
                    return RedirectToAction("LogForm");
            }
        }
        [HttpPost]
        public IActionResult LogForm(LoggedUser toLog)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ViewBag.logInError = null;
                    HomeDbConn.Open();
                    string query = "SELECT * FROM users WHERE email=@Email AND pwd=@Password;";

                    using (var cmd = new MySqlCommand(query, HomeDbConn))
                    {
                        cmd.Parameters.AddWithValue("@Email", toLog.Email);
                        cmd.Parameters.AddWithValue("@Password", toLog.Password);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            switch (Convert.ToInt32(reader["is_admin"]))
                            {
                                case 0:
                                    reader.Close();
                                    HomeDbConn.Close();
                                    HttpContext.Session.SetString("UserMail", toLog.Email);
                                    HttpContext.Session.SetString("UserPassword", toLog.Password);
                                    HttpContext.Session.SetInt32("UserTypeLogged", 0);
                                    return RedirectToAction("UserMainPage", "User", new LoggedUser(HttpContext.Session.GetString("UserMail"), HttpContext.Session.GetString("UserPwd")));
                                case 1:
                                    reader.Close();
                                    HomeDbConn.Close();
                                    HttpContext.Session.SetString("AdminMail", toLog.Email);
                                    HttpContext.Session.SetString("AdminPassword", toLog.Password);
                                    HttpContext.Session.SetInt32("UserTypeLogged", 1);
                                    return RedirectToAction("AdminMainPage", "Admin", new LoggedUser(HttpContext.Session.GetString("AdminMail"), HttpContext.Session.GetString("AdminPassword")));
                                default:
                                    HomeDbConn.Close();
                                    return View();
                            }
                        }

                    }
                }
                catch
                {
                    ViewBag.logInError = "Credenziali non valide";
                    HomeDbConn.Close();
                    return View();
                }
                finally
                {
                    HomeDbConn.Close();
                }
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public IActionResult RegForm(User data)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {
                ViewBag.signUpError = null;
                try
                {
                    string queryInsert = "INSERT INTO ghost_users (email, name, surname, pwd, is_admin) VALUES (@Email, @Name, @Surname, @Password, '0')";
                    if (SendVerificationMail(data.Email))
                    {
                        using (var conn = new MySqlConnection(HomeDbConn.ConnectionString))
                        {
                            conn.Open();
                            using (var cmd = new MySqlCommand(queryInsert, conn))
                            {
                                cmd.Parameters.AddWithValue("@Email", data.Email);
                                cmd.Parameters.AddWithValue("@Name", data.Name);
                                cmd.Parameters.AddWithValue("@Surname", data.Surname);
                                cmd.Parameters.AddWithValue("@Password", data.Password);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        return RedirectToAction("EmailWarning");
                    }
                }
                catch (Exception ex)
                {
                    // Log dell'errore
                    Console.WriteLine(ex.Message);
                    return View();
                }
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
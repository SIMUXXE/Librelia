using Libreria_Elia_V0._0.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Mail;

namespace Libreria_Elia_V0._0.Controllers
{
    public class UserController : Controller
    {

        private readonly IConfiguration _configuration;
        private MySqlConnection UserDbConn;
        private readonly string _smtpServer;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            UserDbConn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            _smtpUsername = _configuration["EmailSettings:Username"];
            _smtpPassword = _configuration["EmailSettings:Password"];

        }

        [Route("User/MainPage")]
        public IActionResult UserMainPage(LoggedUser data)
        {
            if (HttpContext.Session.GetString("UserMail") != null)
                return View(data);
            else
                return RedirectToAction("LogForm", "Home");
        }
        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Home", new { value = 2 });
        }
        public IActionResult ReservationForm()
        {
            if (HttpContext.Session.GetString("UserMail") != null)
            {
                string q = "SELECT * FROM `books`;";
                List<Book> bookList = new List<Book>();
                using (MySqlCommand cmd = new MySqlCommand(q, UserDbConn))
                {
                    try
                    {
                        UserDbConn.Open();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Book x = new Book()
                                {
                                    Book_id = reader.GetInt32(0),
                                    Isbn = reader["isbn"].ToString(),
                                    Title = reader["title"].ToString(),
                                    Author01 = reader["author01"].ToString(),
                                    Author02 = reader["author02"].ToString(),
                                    Author03 = reader["author03"].ToString(),
                                    PublishingHouse = reader["publishing_house"].ToString(),
                                    ReleaseYear = reader["release_year"].ToString(),
                                    Placement = reader["placement"].ToString(),
                                    Genre = reader["genre"].ToString(),
                                    Availability = Convert.ToInt32((reader["availability"]))
                                };
                                bookList.Add(x);
                            }
                        }
                        UserDbConn.Close();
                        return View(bookList);
                    }
                    catch
                    {
                        UserDbConn.Close();
                        return RedirectToAction("UserMainPage");
                    }
                    finally
                    {
                        UserDbConn.Close();
                    }
                }
            }
            else
                return RedirectToAction("LogForm", "Home");

        }
        public IActionResult FoundBooks(string searchTerm)
        {
            if (HttpContext.Session.GetString("UserMail") != null)
            {
                List<Book> bookList = new List<Book>();
                string q = @$"SELECT * FROM books WHERE title LIKE '%{searchTerm}%'
           UNION ALL SELECT * FROM books WHERE publishing_house LIKE '%{searchTerm}%' 
           UNION ALL SELECT * FROM books WHERE isbn LIKE '%{searchTerm}%'
           UNION ALL SELECT * FROM books WHERE author01 LIKE '%{searchTerm}%'
           UNION ALL SELECT * FROM books WHERE author02 LIKE '%{searchTerm}%'
           UNION ALL SELECT * FROM books WHERE author03 LIKE '%{searchTerm}%' 
           UNION ALL SELECT * FROM books WHERE genre LIKE '%{searchTerm}%'
           UNION ALL SELECT * FROM books WHERE isbn LIKE '%{searchTerm}%';";
                using (MySqlCommand srcCmd = new MySqlCommand(q, UserDbConn))
                {
                    try
                    {
                        UserDbConn.Open();
                        using (MySqlDataReader reader = srcCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Book book = new Book();
                                book.Book_id = Convert.ToInt32(reader["book_id"]);
                                book.Isbn = reader["isbn"].ToString();
                                book.Title = reader["title"].ToString();
                                book.Author01 = reader["author01"].ToString();
                                book.PublishingHouse = reader["publishing_house"].ToString();
                                book.Genre = reader["genre"].ToString();
                                book.Availability = Convert.ToInt32(reader["availability"]);
                                bookList.Add(book);
                            }
                        }
                        ViewBag.srcError = null;
                    }
                    catch { }
                    finally
                    {
                        UserDbConn.Close();
                    }
                }
                return View(bookList);
            }
            else
                return RedirectToAction("LogForm", "Home");
        }
        public void SendReceiptMail(string rAdress, Reservation res)
        {
            string senderAddress = _smtpUsername;
            string recipientAddress = rAdress;  //indirizzo del destinatario @mail.com
            string subject = "no-reply";
            string body = $"La tua prenotazione è stata registrata con Successo.\nHai a disposizione tempo fino al {res.ExpDate.Day}/{res.ExpDate.Month}/{res.ExpDate.Year} per ritirare il volume presso la sede istituzionale.\nPassata la data di scadenza la prenotazione verrà annullata";

            //credenziali SMTP per il mittente
            string smtpServer = _smtpServer;
            string username = "libreriaelia@gmail.com";
            string password = _smtpPassword;

            SmtpClient smtpClient = new SmtpClient(smtpServer, 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(username, password);

            MailMessage verificationEmail = new MailMessage(senderAddress, recipientAddress, subject, body);

            try
            {
                smtpClient.Send(verificationEmail);
                ViewBag.mailError = "Verification mail sent";
            }
            catch (Exception ex)
            {
                ViewBag.logInError = $"An error occurred:\n{ex.Message}";
            }
        }

        [Route("Res/{BookId}")]
        public IActionResult Reserve(int BookId)
        {
            Reservation reservation = new Reservation();
            reservation.RegDate = DateTime.Now;
            reservation.ExpDate = DateTime.Now.AddDays(5);
            reservation.UserMail = HttpContext.Session.GetString("UserMail");
            reservation.BookId = BookId;
            try
            {
                string q = "INSERT INTO reservations (regDate, expDate, email_fk, book_id_fk) VALUES (@rDate , @eDate, @uMail, @bId); UPDATE books SET availability = 1 WHERE book_id = @ID;";
                using (MySqlCommand reserveCmd = new MySqlCommand(q, UserDbConn))
                {
                    reserveCmd.Parameters.AddWithValue("@rDate", reservation.RegDate);
                    reserveCmd.Parameters.AddWithValue("@eDate", reservation.ExpDate);
                    reserveCmd.Parameters.AddWithValue("@uMail", reservation.UserMail);
                    reserveCmd.Parameters.AddWithValue("@bId", reservation.BookId);
                    reserveCmd.Parameters.AddWithValue("@ID", reservation.BookId);
                    UserDbConn.Open();
                    reserveCmd.ExecuteNonQuery();
                    SendReceiptMail(HttpContext.Session.GetString("UserMail"), reservation);
                }
                UserDbConn.Close();
            }
            catch
            {
            }
            finally
            {
                UserDbConn.Close();
            }
            return RedirectToAction("SuccessfullReservation");

        }
        public IActionResult SuccessfullReservation()
        {
            return View();
        }

        public IActionResult ActivityManager()
        {
            if (HttpContext.Session.GetString("UserMail") != null)
            {
                string q = @"SELECT reservations.id, reservations.email_fk, reservations.regDate, reservations.expDate, books.title, books.availability, books.book_id FROM reservations INNER JOIN books ON books.book_id = reservations.book_id_FK WHERE reservations.email_fk = @UserMail;";

                List<Reservation> reservationsList = new List<Reservation>();
                try
                {
                    UserDbConn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(q, UserDbConn))
                    {
                        cmd.Parameters.AddWithValue("@UserMail", HttpContext.Session.GetString("UserMail"));
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Reservation x = new Reservation
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    RegDate = Convert.ToDateTime(reader["regDate"]),
                                    ExpDate = Convert.ToDateTime(reader["expDate"]),
                                    UserMail = reader["email_fk"].ToString(),
                                    BookTitle = reader["title"].ToString(),
                                    BookState = Convert.ToInt32(reader["availability"]),
                                    BookId = Convert.ToInt32(reader["book_id"])
                                };
                                reservationsList.Add(x);
                            }

                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    UserDbConn.Close();
                }
                return View(reservationsList);
            }
            else
                return RedirectToAction("LogForm", "Home");
        }

        [Route("DeleteReservation/{ResId}/{BookId}")]
        public IActionResult DeleteUserRes(int ResId, int BookId)
        {
            string q = "UPDATE books SET books.availability = 0 WHERE books.book_id = @value1; DELETE FROM reservations WHERE id = @value2;";
            try
            {
                UserDbConn.Open();
                using (MySqlCommand DelResCmd = new MySqlCommand(q, UserDbConn))
                {
                    DelResCmd.Parameters.AddWithValue("@value1", BookId);
                    DelResCmd.Parameters.AddWithValue("@value2", ResId);
                    DelResCmd.ExecuteNonQuery();
                }
            }
            catch
            {

            }
            finally
            {
                UserDbConn.Close();
            }
            return RedirectToAction("ActivityManager");

        }
    }
}
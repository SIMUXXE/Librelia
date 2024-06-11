using Libreria_Elia_V0._0.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Mail;

namespace Libreria_Elia_V0._0.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private MySqlConnection AdminDbConn;
        private readonly string _smtpServer;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
            AdminDbConn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            _smtpUsername = _configuration["EmailSettings:Username"];
            _smtpPassword = _configuration["EmailSettings:Password"];

        }

        public IActionResult BookSuccessfullyRegistered()
        {
            if (HttpContext.Session.GetString("AdminMail") != null)
                return View();
            else
                return RedirectToAction("LogForm", "Home");
        }

        [Route("Admin/MainPage")]
        public IActionResult AdminMainPage(LoggedUser data)
        {
            if (HttpContext.Session.GetString("AdminMail") != null)
                return View(data);
            else
                return RedirectToAction("LogForm", "Home");
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Home", new { value = 1 });
        }

        public IActionResult BookForm()
        {
            if (HttpContext.Session.GetString("AdminMail") != null)
                return View();
            else
                return RedirectToAction("LogForm", "Home");
        }
        public void SendResponseMail(string rAdress, int type)
        {
            if (type == 1)
            {
                try
                {
                    /*--------------------------------------------------------------------Manda Mail di approvazione--------------------------------------------------------------------*/
                    string senderAddress = _smtpUsername;
                    string recipientAddress = rAdress;  //indirizzo del destinatario @gmail.com
                    string subject = "no-reply";
                    string body = $"Ciao {rAdress},\nla tu richiesta è stata approvata dall'admin.\nBenvenuto/a in ELIA'S Library.";

                    //credenziali SMTP per il mittente
                    string smtpServer = _smtpServer;
                    string username = "libreriaelia@gmail.com";
                    string password = _smtpPassword;

                    SmtpClient smtpClient = new SmtpClient(smtpServer, 587);
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(username, password);

                    MailMessage verificationEmail = new MailMessage(senderAddress, recipientAddress, subject, body);
                    smtpClient.Send(verificationEmail);
                    ViewBag.ghostUsersError = "Mail di approvazione inviata";
                    /*------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
                }
                catch (Exception ex)
                {
                    ViewBag.ghostUsersError = ex.Message;
                }
            }
            else
            {
                try
                {
                    /*--------------------------------------------------------------------Manda Mail di rifiuto--------------------------------------------------------------------*/
                    string senderAddress = _smtpUsername;
                    string recipientAddress = rAdress;  //indirizzo del destinatario @gmail.com
                    string subject = "no-reply";
                    string body = $"Ciao {rAdress},ci dispiace.\nLa tu richiesta è stata rifiutata dall'admin.";

                    //credenziali SMTP per il mittente
                    string smtpServer = _smtpServer;
                    string username = "libreriaelia@gmail.com";
                    string password = _smtpPassword;

                    SmtpClient smtpClient = new SmtpClient(smtpServer, 587);
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(username, password);

                    MailMessage verificationEmail = new MailMessage(senderAddress, recipientAddress, subject, body);
                    smtpClient.Send(verificationEmail);
                    Console.WriteLine("Mail mandata");
                    ViewBag.ghostUsersError = "Mail di approvazione inviata";
                    /*------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
                }
                catch (Exception ex)
                {
                    ViewBag.ghostUsersError = ex.Message;
                }
            }
        }

        public IActionResult UsersToAccept()
        {
            if (HttpContext.Session.GetString("AdminMail") != null)
            {
                ViewBag.ghostUsersError = null;
                string q = "SELECT * FROM ghost_users;";
                List<User> ghosts = new List<User>();
                using (MySqlCommand cmd = new MySqlCommand(q, AdminDbConn))
                {
                    try
                    {
                        AdminDbConn.Open();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                User aUser = new User();
                                aUser.Email = reader["email"].ToString();
                                aUser.Name = reader["name"].ToString();
                                aUser.Surname = reader["surname"].ToString();
                                aUser.Password = reader["pwd"].ToString();
                                aUser.IsAdmin = Convert.ToInt32(reader["is_admin"]);
                                ghosts.Add(aUser);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.ghostUsersError = ex.Message;
                    }
                    finally { AdminDbConn.Close(); }
                }
                return View(ghosts);
            }
            else
                return RedirectToAction("LogForm", "Home");
        }

        private int GetCopyNumber(string title)
        {
            int count = 0;
            string q = "SELECT COUNT(title) FROM books WHERE title = @value;";
            using (var conn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@value", title);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                                count += reader.GetInt32(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Error log
                    Console.WriteLine("GETCOPYNUMBER EX: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return count;
        }

        public IActionResult BookListView()
        {
            if (HttpContext.Session.GetString("AdminMail") != null)
            {
                string q = "SELECT *, COUNT(isbn) From books GROUP BY title;";
                List<Book> bookList = new List<Book>();
                using (MySqlCommand cmd = new MySqlCommand(q, AdminDbConn))
                {
                    try
                    {
                        AdminDbConn.Open();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Book x = new Book()
                                {
                                    Book_id = (int)reader["book_id"],
                                    Isbn = reader["isbn"].ToString(),
                                    Title = reader["title"].ToString(),
                                    Author01 = reader["author01"].ToString(),
                                    Author02 = reader["author02"].ToString(),
                                    Author03 = reader["author03"].ToString(),
                                    PublishingHouse = reader["publishing_house"].ToString(),
                                    ReleaseYear = reader["release_year"].ToString(),
                                    Placement = reader["placement"].ToString(),
                                    Genre = reader["genre"].ToString(),
                                    NumberOfcopys = reader.GetInt32(17),
                                    Availability = Convert.ToInt32((reader["availability"]))
                                };
                                bookList.Add(x);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Error Log
                        Console.WriteLine("BOOKLISTVIEW EX: " + ex.Message);
                    }
                    finally
                    {
                        AdminDbConn.Close();
                    }
                    return View(bookList);
                }

            }
            else
                return RedirectToAction("LogForm", "Home");
        }

        [Route("{email}/{name}/{surname}/{pwd}/{isA}")]
        public IActionResult ApproveUser(string email, string name, string surname, string pwd, int isA)
        {
            ViewBag.ghostUsersError = null;
            string query = "DELETE FROM `ghost_users` WHERE email = @mail; INSERT INTO users (email, name, surname, pwd, is_admin) VALUES (@email, @name, @surname, @password, @is_admin);";

            try
            {
                AdminDbConn.Open();
                using (MySqlCommand Cmd = new MySqlCommand(query, AdminDbConn))
                {
                    Cmd.Parameters.AddWithValue("@mail", email);        //Aggiunge i valori di DELETE
                    Cmd.Parameters.AddWithValue("@email", email);       //Aggiunge i valori di INSERT
                    Cmd.Parameters.AddWithValue("@name", name);         //Aggiunge i valori di INSERT
                    Cmd.Parameters.AddWithValue("@surname", surname);   //Aggiunge i valori di INSERT
                    Cmd.Parameters.AddWithValue("@password", pwd);      //Aggiunge i valori di INSERT
                    Cmd.Parameters.AddWithValue("@is_admin", isA);      //Aggiunge i valori di INSERT
                    Cmd.ExecuteNonQuery();                              //Esegue lo statement
                }

                SendResponseMail(email, 1);
            }
            catch (Exception ex)
            {
                ViewBag.ghostUsersError = ex.Message;
            }
            finally { AdminDbConn.Close(); }
            return RedirectToAction("UsersToAccept");
        }

        [Route("{email}")]
        public IActionResult DeclineUser(string email)
        {
            Console.WriteLine(email);
            ViewBag.ghostUsersError = null;
            string queryDel = "DELETE FROM `ghost_users` WHERE email = @mail";

            try
            {
                AdminDbConn.Open();
                using (MySqlCommand delCmd = new MySqlCommand(queryDel, AdminDbConn))
                {
                    delCmd.Parameters.AddWithValue("@mail", email);     //Aggiunge i valori allo statement di DELETE
                    delCmd.ExecuteNonQuery();                           //Esegue lo statement di DELETE
                }
                AdminDbConn.Close();

                SendResponseMail(email, 0);
            }
            catch (Exception ex)
            {
                ViewBag.ghostUsersError = ex.Message;
            }
            finally { AdminDbConn.Close(); }
            return RedirectToAction("UsersToAccept");

        }

        [HttpPost]
        public IActionResult BookForm(Book newBookDatas)
        {
            if (HttpContext.Session.GetString("AdminMail") != null)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        ViewBag.BookError = null;
                        string q = @$"INSERT INTO `books` (isbn, title, subtitle, author01, author02, author03, genre, publishing_house, release_year, lang, volume, inventory_number, placement, operator, availability, abstract) 
											VALUES ('{newBookDatas.Isbn}', '{newBookDatas.Title}', '{newBookDatas.Subtitle}', '{newBookDatas.Author01}', '{newBookDatas.Author02}', '{newBookDatas.Author03}', '{newBookDatas.PublishingHouse}', '{newBookDatas.Genre}', 
											'{newBookDatas.ReleaseYear}', '{newBookDatas.Language}', '{newBookDatas.Volume}', '{newBookDatas.InventoryNumber}', '{newBookDatas.Placement}', '{newBookDatas.Operator}', '{newBookDatas.Availability}', '{newBookDatas.Abstract}')";

                        AdminDbConn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(q, AdminDbConn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.BookError = ex.Message;
                        Console.WriteLine(ex.Message);
                    }
                    finally { AdminDbConn.Close(); }
                    return RedirectToAction("BookSuccessfullyRegistered");
                }
                else
                    return View();
            }
            else
                return RedirectToAction("LogForm", "Home");
        }

        public IActionResult ReservationListView()
        {
            if (HttpContext.Session.GetString("AdminMail") != null)
            {
                string q = @"SELECT reservations.id, reservations.email_fk, reservations.regDate, reservations.expDate, books.title, books.availability
                         FROM reservations INNER JOIN books ON books.book_id = reservations.book_id_FK";

                List<Reservation> reservationsList = new List<Reservation>();
                using (MySqlCommand cmd = new MySqlCommand(q, AdminDbConn))
                {
                    try
                    {
                        AdminDbConn.Open();
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
                                    BookState = Convert.ToInt32(reader["availability"])
                                };
                                reservationsList.Add(x);
                            }
                        }
                    }
                    catch
                    {
                        AdminDbConn.Close();
                        RedirectToAction("AdminMainPage");
                    }
                    finally { AdminDbConn.Close(); }
                }
                return View(reservationsList);
            }
            else
                return RedirectToAction("LogForm", "Home");
        }

        [Route("Manager/{id}")]
        public IActionResult ResManager(int id)
        {
            if (HttpContext.Session.GetString("AdminMail") != null)
            {
                string q = @"SELECT reservations.id, reservations.regDate, reservations.expDate, reservations.email_fk, reservations.book_id_FK, books.title, books.availability 
                        FROM reservations INNER JOIN books ON reservations.book_id_FK = books.book_id WHERE reservations.id=@value;";
                using (MySqlCommand cmd = new MySqlCommand(q, AdminDbConn))
                {
                    cmd.Parameters.AddWithValue("@value", id);
                    Reservation obj = new Reservation();
                    try
                    {
                        AdminDbConn.Open();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            obj.Id = Convert.ToInt32(reader["id"]);
                            obj.RegDate = Convert.ToDateTime(reader["regDate"]);
                            obj.ExpDate = Convert.ToDateTime(reader["expDate"]);
                            obj.UserMail = Convert.ToString(reader["email_fk"]);
                            obj.BookTitle = Convert.ToString(reader["title"]);
                            obj.BookState = Convert.ToInt32(reader["availability"]);
                            obj.BookId = Convert.ToInt32(reader["book_id_FK"]);
                            AdminDbConn.Close();
                            return View(obj);
                        }
                    }
                    catch
                    {
                        AdminDbConn.Close();
                        return RedirectToAction("AdminMainPage");
                    }
                }

            }
            else
                return RedirectToAction("LogForm", "Home");
        }

        [Route("MarkAsCollected/{id}/{bId}")]
        public IActionResult MarkAsCollected(int ResId, int bId)
        {
            string q1 = @"UPDATE books SET books.availability = 2 WHERE books.book_id = @value;
            UPDATE reservations SET expDate = expDate + INTERVAL 30 DAY WHERE reservation.id = @id";

            using (MySqlCommand cmd = new MySqlCommand(q1, AdminDbConn))
            {
                cmd.Parameters.AddWithValue("@value", bId);
                cmd.Parameters.AddWithValue("@Bid", bId);
                cmd.Parameters.AddWithValue("@id", ResId);
                try
                {
                    AdminDbConn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch { }
                finally { AdminDbConn.Close(); }
            }
            return RedirectToAction("ReservationListView");
        }


        [Route("DeleteRes/{id}/{bId}")]
        public IActionResult DeleteRes(int id, int bId)
        {
            string q1 = @"UPDATE books SET books.availability = 0 WHERE books.book_id = @value1;
            DELETE FROM reservations WHERE id = @value2";
            MySqlCommand cmd = new MySqlCommand(q1, AdminDbConn);

            cmd.Parameters.AddWithValue("@value1", bId);
            cmd.Parameters.AddWithValue("@value2", id);

            try
            {
                AdminDbConn.Open();
                cmd.ExecuteNonQuery();
            }
            catch { }
            finally { AdminDbConn.Close(); }
            return RedirectToAction("ReservationListView");
        }

        public IActionResult FoundBooks(string searchTerm)
        {
            List<Book> bookList = new List<Book>();
            string q = @$"SELECT *, COUNT(isbn) FROM books WHERE title LIKE '%{searchTerm}%' GROUP BY title
           UNION ALL SELECT *, COUNT(isbn) FROM books WHERE publishing_house LIKE '%{searchTerm}%' GROUP BY title
           UNION ALL SELECT *, COUNT(isbn) FROM books WHERE isbn LIKE '%{searchTerm}%' GROUP BY title
           UNION ALL SELECT *, COUNT(isbn) FROM books WHERE author01 LIKE '%{searchTerm}%' GROUP BY title
           UNION ALL SELECT *, COUNT(isbn) FROM books WHERE author02 LIKE '%{searchTerm}%' GROUP BY title
           UNION ALL SELECT *, COUNT(isbn) FROM books WHERE author03 LIKE '%{searchTerm}%' GROUP BY title
           UNION ALL SELECT *, COUNT(isbn) FROM books WHERE genre LIKE '%{searchTerm}%' GROUP BY title
           UNION ALL SELECT *, COUNT(isbn) FROM books WHERE isbn LIKE '%{searchTerm}%' GROUP BY title";
            using (MySqlCommand srcCmd = new MySqlCommand(q, AdminDbConn))
            {
                try
                {
                    AdminDbConn.Open();
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
                            book.Availability = Convert.ToInt32(reader["availability"]);
                            book.Genre = reader["genre"].ToString();
                            book.NumberOfcopys = reader.GetInt32(17);
                            bookList.Add(book);
                        }
                    }
                    ViewBag.srcError = null;
                }
                catch { }
                finally
                {
                    AdminDbConn.Close();
                }
            }
            return View(bookList);

        }

        [Route("Edit/{id}/{nCopys}")]
        public IActionResult BookManager(int Id, int nCopys)
        {
            Book bookToUpdate = new Book();
            string q = "SELECT * FROM books WHERE books.book_id = @id ;";
            try
            {
                AdminDbConn.Open();
                using (var cmd = new MySqlCommand(q, AdminDbConn))
                {
                    cmd.Parameters.AddWithValue("@id", Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookToUpdate.Book_id = (int)reader["book_id"];
                            bookToUpdate.Isbn = reader["isbn"].ToString();
                            bookToUpdate.Title = reader["title"].ToString();
                            bookToUpdate.Subtitle = reader["subtitle"].ToString();
                            bookToUpdate.Author01 = reader["author01"].ToString();
                            bookToUpdate.Author02 = reader["author02"].ToString();
                            bookToUpdate.Author03 = reader["author03"].ToString();
                            bookToUpdate.PublishingHouse = reader["publishing_house"].ToString();
                            bookToUpdate.ReleaseYear = reader["release_year"].ToString();
                            bookToUpdate.Language = reader["lang"].ToString();
                            bookToUpdate.Volume = reader["volume"].ToString();
                            bookToUpdate.InventoryNumber = reader["inventory_number"].ToString();
                            bookToUpdate.Placement = reader["placement"].ToString();
                            bookToUpdate.Operator = HttpContext.Session.GetString("AdminMail");
                            bookToUpdate.NumberOfcopys = nCopys;
                            bookToUpdate.Availability = Convert.ToInt32(reader["availability"]);
                            bookToUpdate.Abstract = reader["abstract"].ToString();
                        }
                    }
                }
            }
            catch { }
            finally
            {
                AdminDbConn.Close();
            }
            return View(bookToUpdate);
        }

        public IActionResult UpdateBook(Book book)
        {
            string query = @$"UPDATE books SET
			isbn = '{book.Isbn}', title = '{book.Title}', subtitle = '{book.Subtitle}', author01 = '{book.Author01}', author02 = '{book.Author02}', author03 = '{book.Author03}',
			publishing_house = '{book.PublishingHouse}', release_year = '{book.ReleaseYear}', lang = '{book.Language}', volume = '{book.Volume}', inventory_number = '{book.InventoryNumber}',
			placement = '{book.Placement}', operator = '{book.Operator}', availability = '{book.Availability}', abstract = '{book.Abstract}' WHERE isbn = {book.Isbn};";

            try
            {
                AdminDbConn.Open();
                using (var cmd = new MySqlCommand(query, AdminDbConn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally { AdminDbConn.Close(); }
            return RedirectToAction("BookListView");
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartBoardLockerServer.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        
        [HttpGet("[action]")]
        public IActionResult Login([FromQuery(Name = "username")] string username, [FromQuery(Name = "password")] string password)
        {
            
            MySqlCommand command = new MySqlCommand("SELECT * FROM user WHERE username=@username;", Program.dbConnection);
            command.Parameters.AddWithValue("@username", username);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            
            if (table.Rows.Count > 0)
            {
                string storedPassword = table.Rows[0]["password"].ToString();

                
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    password = builder.ToString();
                }

                
                if (storedPassword == password)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(Program.configJson.JwtKey);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, username),
                            new Claim(ClaimTypes.Role, Convert.ToBoolean(table.Rows[0]["admin"]) ? "Admin" : "User")
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);
                    Response.Cookies.Append("jwt", tokenString, new CookieOptions
                    {
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(7)
                    });
                    return Redirect("/");
                }
            }

            
            return Unauthorized();
        }

        
        [HttpPost("[action]")]
        public IActionResult Register([FromForm] string username, [FromForm] string password, [FromForm] string nameSurname)
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM user WHERE username=@username;", Program.dbConnection);
            command.Parameters.AddWithValue("@username", username);
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(command);
            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            
            if (dataTable.Rows.Count > 0)
            {
                return BadRequest("Bu kullanıcı adı zaten kullanılıyor!");
            }

            
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                password = builder.ToString();
            }

            
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO user (username, password, admin, name_surname) VALUES (\"{username}\", \"{password}\", 0 \"{nameSurname}\");", Program.dbConnection);
            cmd.ExecuteNonQuery();
            return Redirect("/Login");
        }

        
        [HttpPatch("[action]")]
        [Authorize]
        public IActionResult ChangePassword([FromForm] string oldPassword, [FromForm] string newPassword)
        {
            string username = User.Identity.Name;
            MySqlCommand command = new MySqlCommand("SELECT * FROM user WHERE username=@username;", Program.dbConnection);
            command.Parameters.AddWithValue("@username", username);
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            
            if (table.Rows.Count > 0)
            {
                string storedPassword = table.Rows[0]["password"].ToString();

                
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(oldPassword));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    oldPassword = builder.ToString();
                }

                
                if (storedPassword == oldPassword)
                {
                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(newPassword));
                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            builder.Append(bytes[i].ToString("x2"));
                        }
                        newPassword = builder.ToString();
                    }

                    MySqlCommand cmd = new MySqlCommand($"UPDATE user SET password=\"{newPassword}\" WHERE username=\"{username}\";", Program.dbConnection);
                    cmd.ExecuteNonQuery();
                    return Ok();
                }
                else return Unauthorized();
            }

            
            return StatusCode(500);
        }

        
        [Authorize]
        [HttpGet("[action]")]
        public IActionResult Me()
        {
            return Ok(User.Identity.Name);
        }
    }
}
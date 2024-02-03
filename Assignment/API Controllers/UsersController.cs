using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Assignment.Provider;
using Assignment.UserRepository;

namespace Assignment.API_Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private UserMgntEntities db = new UserMgntEntities();
        //==================================================================
        //1.POST is used to login user 

        [HttpPost]
        [AllowAnonymous]
        [Route("login")]

        public IHttpActionResult Login([FromBody] User user)
        {
            using (UserRepo repo = new UserRepo())
            {
                var userinfo = repo.IsValidUser(user.Username, user.Password);
                if (userinfo != null)
                {

                    var username = userinfo.Username;
                    string[] roles = userinfo.Roles?.Split(',') ?? new string[0];

                    var token = JwtHelper.GenerateToken(username, roles);
                    
                    return Ok(new { Token = token });
                }
                else
                {
                    return Unauthorized();
                }
            }
        }

        //===================================================================
        // 2.GET is used to get all persons (Access only for Admin User)

        [JwtTokenValidation]
        [Authorize(Roles = "Admin")]
        [Route("getusers")]
        public IHttpActionResult GetUsers()
        {          
            var users = db.Users.ToList();
            return Ok(users);
        }


        //===================================================================
        //3.GET single user

        [HttpGet]
        [JwtTokenValidation]
        [ResponseType(typeof(User))]
        [Authorize(Roles = "Admin")]
        [Route("getuser/{id}")]
        public IHttpActionResult GetUser(string id)
        {
            if (!Guid.TryParse(id, out Guid userId))
            {
                return BadRequest("Invalid userId format");
            }

            User user = db.Users.Find(userId);//Guid pk column

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        //=======================================================================
        //4.POST is used to create record about new user (Access only for Admin Users)

        [HttpPost]
        [JwtTokenValidation]
        [ResponseType(typeof(User))]
        [Authorize(Roles = "Admin")]
        [Route("postuser")]
        public IHttpActionResult PostUser(User user)
        {

            if (user == null ||
                string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.Password) ||
                user.Age <= 0 ||
                string.IsNullOrWhiteSpace(user.Hobbies))
            {
                return BadRequest("Username,Password,Age,and Hobbies are required fields");
            }
            if (IsUsernameExist(user.Username))
            {
                return Content(HttpStatusCode.Conflict, "Username is already taken..");
            }

            db.Users.Add(user);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (UserExists(user.Id))
                {
                    return Content(HttpStatusCode.Conflict, "Id is already exist..");
                }
                else
                {
                    throw;
                }
            }

            // return CreatedAtRoute("DefaultApi", new { id = user.Id }, user);
            return Ok(user);
        }


        //=================================================================
        //5.PUT is used to update existing users 
        [JwtTokenValidation]
        [Authorize(Roles = "Admin")]
        [HttpPut]
        [ResponseType(typeof(User))]
        [Route("putuser")]
        public IHttpActionResult PutUser([FromBody] User user)
        {
            if (!Guid.TryParse(Convert.ToString(user.Id), out Guid userId))
            {
                return BadRequest("Invalid userId format");
            }
            if (string.IsNullOrWhiteSpace(user.Username) ||
               string.IsNullOrWhiteSpace(user.Password) ||
               user.Age <= 0 ||
               string.IsNullOrWhiteSpace(user.Hobbies))
            {
                return BadRequest("Username,Password,Age,and Hobbies are required fields");
            }

            var existingUser = db.Users.Find(userId);//Guid pk column
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.Username = user.Username;
            existingUser.Password = user.Password;
            existingUser.Age = user.Age;
            existingUser.Hobbies = user.Hobbies;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }

            return Ok(existingUser);
        }


        //=============================================================
        //6.DELETE is used to delete existing user from database(Access only for Admin Users)
        [JwtTokenValidation]
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(User))]
        [Route("deleteuser/{id}")]
        public IHttpActionResult DeleteUser(Guid id)
        {

            if (!Guid.TryParse(Convert.ToString(id), out Guid userId))
            {
                return BadRequest("Invalid userId format");
            }

            User user = db.Users.Find(id);//Guid pk column
            if (user == null)
            {
                return NotFound();
            }

            db.Users.Remove(user);
            db.SaveChanges();

            return Content(HttpStatusCode.NoContent, "Record is found and deleted");
        }


        //========================================================================
        //Common methods
        //=============================For Authorization============================================

        private bool UserExists(Guid id)
        {
            return db.Users.Count(e => e.Id == id) > 0;
        }
        private bool IsUsernameExist(string username)
        {
            var user = db.Users.SingleOrDefault(e => e.Username == username);
            return user != null;
        }

        //========================================================================
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace ComplexTypeWS
{
    /// <summary>
    /// http://localhost:3414/TestService.asmx
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class TestService : System.Web.Services.WebService
    {

        [WebMethod]
        public void Silent()
        {
            // Do something
        }

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public string SingleValue(string name)
        {
            return string.Format("Hello World, {0}", name);
        }

        [WebMethod]
        public string SimpleArray(string[] names)
        {
            return string.Format("Hello World, {0}", string.Join(",", names));
        }

        [WebMethod]
        public string SimpleObj(User user)
        {
            return string.Format("Hello UserInfo, {0}-{1}", user.Id, user.Name);
        }

        [WebMethod]
        public User GetUser(int id, string name)
        {
            return new User() { Id = id, Name = name };
        }

        [WebMethod]
        public List<User> GetDepUsers(Dep dep)
        {
            return dep.Users;
        }

        [WebMethod]
        public List<User> GetUsers(User[] users)
        {
            List<User> result = new List<User>();
            foreach (User usr in users)
            {
                result.Add(usr);
            }
            return result;
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Dep
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<User> Users { get; set;}
    }
}

using System;
using System.Text;
using System.Web;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace WebLibrary
{
    public class ApplicationServices
    {
        //CHECKS IF USER EXISTS
        public bool userExists(string username, string password, string source)
        {
            bool correctUsername = false;
            bool correctPassword = false;
            string path;

            //used to determine what file to read. this will be used to differentiate members and staff
            if (source == "member")
                path = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, @"App_Data\Members.xml");
            else
                path = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, @"App_Data\Staff.xml");

            //Reads all nodes
            XmlTextReader reader = new XmlTextReader(path); 
            string type = "";
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: 
                        type = reader.Name;
                        break;
                    case XmlNodeType.Text:
                        //checks if username matches the element's text, if element is <username>
                        if (type == "username")
                        {
                            if (reader.Value == username)
                                correctUsername = true;
                        }
                        //checks if password matches the element's text, if element is <password>
                        if (type == "password")
                        {
                            if (this.Decrypt(reader.Value) == password)
                                correctPassword = true;

                        }
                        break;
          
                    case XmlNodeType.EndElement: 
                        //if end element is </uses>, then check whether password and username match. if not, revert bool values to false
                        if (reader.Name == "users")
                        {
                            if (correctPassword == true && correctUsername == true)
                                return true;
                            else
                            {
                                correctPassword = false;
                                correctUsername = false;
                            }
                        }
                        break;
                }
            }
            reader.Close();
            return false;
        }

        //ADDS A USER TO MEMBERS.XML
        public bool addUser(string username, string password)
        {
            //First, we check whether the username already exists.

            string fLocation = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, @"App_Data\Members.xml");

            XmlTextReader reader = new XmlTextReader(fLocation);
            bool exists = false;
            string type = "";

            string newP = this.Encrypt(password);
            while (reader.Read())
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            type = reader.Name;
                            break;
        
                        case XmlNodeType.Text:
                            //checks if username matches the element's text, if element is <username>
                            if (type == "username")
                            {
                                if (reader.Value == username)
                                    exists = true;
                            }
                            break;
                    }
                }
            }
            reader.Close();
            //if username exists, return false and don't add the new member
            if (exists == true)
                return false;

            //adds user to Members.xml
            else
            {
                XDocument doc = XDocument.Load(fLocation);
                XNamespace nameSpace = "http://tempuri.org/Authorizedxsd.xsd";
                doc.Root.Element(nameSpace + "All").
                    Add(new XElement(nameSpace + "users",
                    new XElement(nameSpace + "username", username),
                    new XElement(nameSpace + "password", newP)
                    ));

                doc.Save(fLocation);

                return true;
            }
        }


        //used to encrpt a user's password. this will be used when storing the password when a new user is created
        public string Encrypt(string text)
        {
            byte[] vs = Encoding.UTF8.GetBytes(text);
            for (int i = 0; i < vs.Length; i++)
                vs[i] = (byte)(vs[i] ^ 0x12);
            return Convert.ToBase64String(vs);
        }

        //used to decrypt a user's password. this will be used when checking if the user's credentials are correct
        public string Decrypt(string text)
        {
            byte[] vs = Convert.FromBase64String(text);
            for (int i = 0; i < vs.Length; i++)
                vs[i] = (byte)(vs[i] ^ 0x12);
            return Encoding.UTF8.GetString(vs);
        }
    }
}


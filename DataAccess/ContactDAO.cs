using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ContactDAO
    {
        private readonly poolcomvnContext _context;
        public ContactDAO(poolcomvnContext context)
        {
            _context = context;
        }
        public void AddContact(Contact contact)
        {
            try
            {
                 _context.Contacts.Add(contact);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}

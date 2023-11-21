using Microsoft.AspNetCore.Mvc;
using myContacts.Models;
using System.Net;

namespace myContactsWeb.Services.Interfaces
{
    public interface IContactsApiService
    {
        Task<IEnumerable<Contact>?> GetContacts();
        Task<Contact?> GetContact(int id);
        Task<IActionResult> PutContact(int id, Contact contact);
        Task<Contact?> PostContact(Contact contact);
        Task<HttpStatusCode> DeleteContact(int id);
        bool ContactExists(int id);
    }
}

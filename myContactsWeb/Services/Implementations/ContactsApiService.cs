using Microsoft.AspNetCore.Mvc;
using myContacts.Models;
using myContactsWeb.Services.Interfaces;
using System.Net;

namespace myContactsWeb.Services.Implementations
{
    public class ContactsApiService : IContactsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ContactsApiService> _logger;

        public ContactsApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ContactsApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public bool ContactExists(int id)
        {
            try
            {
                var response = _httpClient.GetAsync($"/api/Contacts/{id}").Result;
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ContactsApiService.ContactExists()");
                return false;
            }
        }

        public async Task<HttpStatusCode> DeleteContact(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/Contacts/{id}");
                return response.StatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ContactsApiService.DeleteContact()");
                return HttpStatusCode.InternalServerError;
            }
        }

        public async Task<Contact?> GetContact(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Contacts/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Contact>();

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ContactsApiService.GetContact()");
                return null;
            }
        }

        public async Task<IEnumerable<Contact>?> GetContacts()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Contacts");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<IEnumerable<Contact>>();
                }
                else
                {
                    throw new Exception(string.Format("There was an Error. This is the StatusCode = {0}", response.StatusCode));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ContactsApiService.GetContacts()");
                return null;
            }
            throw new NotImplementedException();
        }

        public async Task<Contact?> PostContact(Contact contact)
        {
            try
            {
                if (contact != null && contact.IsValid())
                {

                    var response = await _httpClient.PostAsJsonAsync($"/api/Contacts", contact);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadFromJsonAsync<Contact>();
                    }
                    else
                    {
                        throw new Exception(string.Format("There was an Error. This is the StatusCode = {0}", response.StatusCode));
                    }
                }
                else
                {
                    throw new Exception("Contact is null or invalid");
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ContactsApiService.PostContact()");
                return null;
            }
        }

        public async Task<IActionResult> PutContact(int id, Contact contact)
        {
            try
            {
                if (contact != null && contact.IsValid())
                {
                    var response = await _httpClient.PutAsJsonAsync($"/api/Contacts/{id}", contact);
                    if (response.IsSuccessStatusCode)
                    {
                        return new OkResult();
                    }
                    else
                    {
                        throw new Exception(string.Format("There was an Error. This is the StatusCode = {0}", response.StatusCode));
                    }
                }
                else
                {
                    throw new Exception("Contact is null or invalid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ContactsApiService.PutContact()");
                return new BadRequestResult();
            }
        }
    }
}

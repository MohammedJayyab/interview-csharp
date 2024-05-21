using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortenerService.Application.Common.Exceptions;

public class ShortUrlCreationException : Exception
{
    /// <summary>
    /// This exception is thrown when the application command can not create a short URL in the database.
    /// </summary>
    /// <param name="message">The message describes the error.</param>
    public ShortUrlCreationException(string message)
        : base(message)
    {
    }
}

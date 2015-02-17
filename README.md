This is a .NET SDK for interacting with the Firebrand Cloud Services APIs.

# Install with NuGet

[![Join the chat at https://gitter.im/FirebrandTech/fcs-sdk-net](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/FirebrandTech/fcs-sdk-net?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

In Visual Studio, go to Tools > NuGet Package Manager > Manage NuGet Packages for Solution
Search the Online packages for FcsSdkNet.  Click "Install."
When the ServiceStack dependency is automatically installed, you will be prompted to accept a license agreement.

# Instantiating the Client

You'll need the Service URL, API Key, and API Secret, provided by Firebrand.  You can instantiate the client like this:
```c#
// You'll need these packages
using Fcs;
using Cloud.Api.V2.Model;
// ... Inside your code somewhere
    const string serviceUrl = "PUT THE SERVICE URL HERE";
    const string apiKey = "PUT YOUR API KEY HERE";
    const string apiSecret = "PUT YOUR API SECRET HERE";
    var client = new FcsClient(serviceUrl, apiKey, apiSecret);
```

# Authenticating for widgets

If your site is embedding some of Firebrand's widgets, you can use the FcsClient to authenticate your users like this before displaying the widget:

```c#
client.AuthOrRegister(new AuthRequest
                                   {
                                       UserName = email
                                   });
```

When a user logs out of your site, you should call Unauth:

```c#
client.Unauth();
```

# Posting a new order

Here is a simple console application which posts an order to the Firebrand Cloud Services APIs and prints the JSON response.  You will need to substitute your own values for apiKey, apiSecret, and serviceUrl.

```c#
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Fcs;
using Cloud.Api.V2.Model;

namespace ConsoleApplication1 {
    class Program {
        static void Main(string[] args) {

            const string serviceUrl = "PUT THE SERVICE URL HERE";
            const string apiKey = "PUT YOUR API KEY HERE";
            const string apiSecret = "PUT YOUR API SECRET HERE";
            var client = new FcsClient(serviceUrl, apiKey, apiSecret);

            // Add all the items for the order
            var items = new List<OrderItemDto> {
                new OrderItemDto {
                    Ean = "9781416564874",
                    Title = "Test Item #1",
                    Author = "Test Author #1",
                    Price = 12.34m,
                    Quantity = 1
                }
            };

            var user = new UserDto
            {
                Email = "user@email.com"
            };

            var order = new OrderDto
            {
                TotalPrice = 12.34m,
                Items = items,
                ReferenceId = "999",
                User = user,
                ThrowErrors = false
            };

            Console.WriteLine("Sending an order to {0}", serviceUrl);

            var response = (HttpWebResponse) client.PlaceOrder(order);

            // Display the response
            using (Stream stream = response.GetResponseStream()) {
                var reader = new StreamReader(stream, Encoding.UTF8);
                var responseString = reader.ReadToEnd();
                Console.WriteLine("Response:");
                Console.WriteLine(responseString);
            }
        }
    }
}
```

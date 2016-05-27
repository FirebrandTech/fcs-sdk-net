This is a .NET SDK for interacting with the Firebrand Cloud Services APIs.

# Install with NuGet

This SDK can be included in your project using [NuGet](https://www.nuget.org/packages/FcsSdkNet/).  In Visual Studio:

- go to Tools > NuGet Package Manager > Manage NuGet Packages for Solution
- Search the Online packages for FcsSdkNet.  Click "Install."

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

If your site is embedding some of Firebrand's widgets, such as the Library widget, you can use the FcsClient to authenticate your users like this before displaying the widget:

```c#
client.Auth(email);
```

When a user logs out of your site, you should call Unauth:

```c#
client.Unauth();
```

The FcsClient implements IDisposable, so you can also use it like this:

```c#
using (var fcs = new FcsClient(serviceUrl, apiKey, apiSecret)) {
    fcs.Auth(email);
}
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
            var items = new List<OrderItem> {
                new OrderItem {
                    Ean = "9781416564874",
                    Title = "Test Item #1",
                    Author = "Test Author #1",
                    Price = 12.34m,
                    Quantity = 1
                }
            };

            var user = new User
            {
                Email = "user@email.com"
            };

            var order = new Order
            {
                TotalPrice = 12.34m,
                Items = items,
                ExternalOrderId = "999",
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

# Library widget example

The Library widget shows your users the books they own.  Here's how that works:

## A user makes a purchase from your site.

When the purchase completes, you do something like this:

```c#
using (var fcs = new FcsClient(serviceUrl, apiKey, apiSecret)) {
    using (var fcs = new FcsClient(clientId: ApiKey, clientSecret: ApiSecret, apiUrl: ServiceUrl)) {
        var items = new List<OrderItem> {
            new OrderItem {
                Ean = ean,
                Title = "", // FILL IN
                Author = "", // FILL IN
                Price = 12.34m, // FILL IN
                Quantity = 1
            }
        };

        var order = new Order {
            TotalPrice = 12.34m, // FILL IN
            Items = items,
            ReferenceId = "999", // FILL IN
            User = new User {Email = email}
        };

        var response = (Order) fcs.PlaceOrder(order);
}
```

When you use this PlaceOrder API, you tell Firebrand that the user has purchased a certain title.

## The user clicks to view their library.

First, authenticate the user with Firebrand by passing their email to the Auth API, like this:

```c#
using (var fcs = new FcsClient(clientId: ApiKey, clientSecret: ApiSecret, apiUrl: ServiceUrl)) {
    fcs.Auth(email);
}
```

That will set some authentication cookies that the widget needs.  Then, you can render the library like this:

```c#
    <div data-ng-app="widgets" data-fc-api="@ViewBag.ApiKey">
        <h2>My Ebook Library</h2>
        <div data-ng-app="widgets" data-fc-library="view:table"></div>
        <script src="@ViewBag.ServiceDomain/widgets/scripts/fcw.js"> </script>
    </div>
```

(Assuming you set ViewBag.ServiceDomain and ViewBag.ApiKey to the right values.)

## The user logs out.

To remove the authentication keys for a user, call Unauth:

```c#
    fcs.Unauth();
```

# Downloading ebooks without the library widget

If you prefer to build your own library interface, it's possible to use the SDK simply to request links that your users can click to download ebooks they have purchased.  In this case, Firebrand does not need to maintain user accounts or ownership records, so you do not need to use the Auth or PlaceOrder APIs.

Firebrand's download links embed a record of the sale price, since our Direct 2 Reader contracts typically require this.  They also embed a user identifier, typically an email address.  Therefore, the sale price and email should be passed when requesting download links.  Here's how that looks:

```c#

using (var fcs = new FcsClient(clientId: ApiKey, clientSecret: ApiSecret, apiUrl: ServiceUrl)) {
    // build this using whatever ISBNs the user owns:
    var ownedEans = new EanSalesDetails();
    ownedEans.Add("9780123456789", 12.34m); // the price is a "decimal" type
    ownedEans.Add("9780111111111", 9.99m);

    var userIdentifier = "user@example.com";

    var isbnDetails = client.GetDownloadUris(ownedEans, userIdentifier);
}
```

Note that the links returned by GetDownloadUris will expire after 30 minutes by default.  You can override this.  For example, if you want the links to expire in 1 hour, you can do this:

```c#
    var isbnDetails = client.GetDownloadUris(ownedEans, userIdentifier, 60); // links expire in 60 minutes
````

A single ISBN may have multiple download links - one for an EPUB, one for a PDF, and one for a Mobi file, for example.  For this reason, the API returns a dictionary, where each key is an ISBN, and its value is a list of objects with details about a specific file for that ISBN.  Here is an example response for a request with two ISBNs.  Note that each ISBN has 3 downloadable files, so there are a total of 6 URLs in the response:

```js
{
    "9780123456789": [
        {
            "ean": "9780123456789",
            "title": "First Example Title",
            "type": "Kindle",
            "uri": "https://cloud.firebrandtech.com/api/v2/asset-files/7ce0cf7f-55c7-48bd-9539-a01d0139f5a3?fcs_token=..."
        },
        {
            "ean": "9780123456789",
            "title": "First Example Title",
            "type": "ePub",
            "uri": "https://cloud.firebrandtech.com/api/v2/asset-files/ce205570-d706-4439-b364-a01d015597fe?fcs_token=..."
        },
        {
            "ean": "9780123456789",
            "title": "First Example Title",
            "type": "PDF",
            "uri": "https://cloud.firebrandtech.com/api/v2/asset-files/75299aff-00ba-4982-96d2-a01d016275ff?fcs_token=..."
        }
    ],
    "9780111111111": [
        {
            "ean": "9780111111111",
            "title": "Second Example Title",
            "type": "Kindle",
            "uri": "https://cloud.firebrandtech.com/api/v2/asset-files/b04a5614-1713-4922-8d65-a01c01428449?fcs_token=..."
        },
        {
            "ean": "9780111111111",
            "title": "Second Example Title",
            "type": "ePub",
            "uri": "https://cloud.firebrandtech.com/api/v2/asset-files/14af67b2-83ef-4440-b8f5-a01d01574eac?fcs_token=..."
        },
        {
            "ean": "9780111111111",
            "title": "Second Example Title",
            "type": "PDF",
            "uri": "https://cloud.firebrandtech.com/api/v2/asset-files/860df016-8d23-43b7-b023-a01d016436a2?fcs_token=..."
        }
    ]
}
```

In this scenario, you might request the download links, then loop over your ISBNs when displaying the user's library, and look up the download links for the current ISBN inside the loop.  Here's a very simple example that would work in a Razor template:

@{
    foreach (var isbn in isbns) {
        if (!isbnDetails.ContainsKey(isbn)) {continue;}
        <ul>
            <h3>@isbnDetails[ean][0].Title</h3><!-- or use the Title from your own database -->
            @foreach (var details in isbnDetails[ean]) {
            <li><a href="@details.Uri">@details.Type</a></li>
            }
        </ul>
    }
}
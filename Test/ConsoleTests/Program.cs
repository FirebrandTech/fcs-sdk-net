// Copyright © 2010-2015 Firebrand Technologies

using System;
using Fcs;
using Fcs.Model;

namespace ConsoleTests {
    public static class Program {
        public static void Main(string[] args) {
            try {
                var clientId = Environment.GetEnvironmentVariable("FcsSdk_ClientId");
                var clientSecret = Environment.GetEnvironmentVariable("FcsSdk_ClientSecret");
                const string appId = "ConsoleTest";
                using (var client = new FcsClient(clientId, clientSecret, appId, "http://cloud.local/api/v2")) {
                    var catalog = client.PublishCatalog(new Catalog
                                                        {
                                                            Name = "TestCatalog"
                                                        });

                    Console.WriteLine("Catalog {0} Published...", catalog.Id);
                }
            }
            catch (Exception e) {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString());
                Console.ForegroundColor = color;
            }
        }
    }
}
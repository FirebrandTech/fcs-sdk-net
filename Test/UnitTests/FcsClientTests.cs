// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Collections.Generic;
using System.Web;
using FakeItEasy;
using Fcs;
using Fcs.Framework;
using Fcs.Model;
using FluentAssertions;
using Xunit;

namespace UnitTests {
    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    public class FcsClientTests {
        private const string ClientId = "CLIENTID";
        private const string ClientSecret = "CLIENTSECRET";
        private const string AppId = "APPID";
        private const string Token = "TOKEN";
        private const string Token2 = "TOKEN2";
        //private const string Url = "https://cloud.firebrandtech.com/api/v2";
        private readonly DateTime _expiration;
        private readonly DateTime _expiration2;

        public FcsClientTests() {
            this._expiration = DateTime.UtcNow.AddDays(1);
            this._expiration2 = DateTime.UtcNow.AddDays(2);
            FcsClient.Reset();
        }

        [Fact]
        public void NonWeb_NewAuth_NoUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token,
                          Expires = this._expiration
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppId)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            FcsClient.Token.Should().Be(Token);
            FcsClient.TokenExpires.Should().Be(this._expiration);

            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName == null),
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId)))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs.PublishCatalog(new Catalog());

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                       h["Authorization"] == "Bearer " + Token)))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void NonWeb_ExistingAuth_NoUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token,
                          Expires = this._expiration
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppId)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            fcs.Auth();

            FcsClient.Token.Should().Be(Token);
            FcsClient.TokenExpires.Should().Be(this._expiration);

            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName == null),
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId)))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs.PublishCatalog(new Catalog());

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                       h["Authorization"] == "Bearer " + Token)))
             .MustHaveHappened(Repeated.Exactly.Once);
            fcs.Auth();
        }

        [Fact]
        public void NonWeb_NewAuth_NewUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token,
                          Expires = this._expiration
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppId)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth("testuser");

            FcsClient.Token.Should().Be(Token);
            FcsClient.TokenExpires.Should().Be(this._expiration);
            FcsClient.User.Should().Be("testuser");

            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName == "testuser"),
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId)))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs.PublishCatalog(new Catalog());

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                       h["Authorization"] == "Bearer " + Token)))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void NonWeb_ExistingAppAuth_NewUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName == null),
                                       A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token,
                          Expires = this._expiration
                      });
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                        r.ClientSecret == null &&
                                                                        r.UserName != null),
                                       A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token2,
                          Expires = this._expiration2
                      });

            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            using (var fcs = new FcsClient(ClientId, ClientSecret, AppId)
                             {
                                 Context = context,
                                 ServiceClientFactory = factory
                             }) {
                fcs.Auth();

                fcs.Auth("testuser");

                FcsClient.Token.Should().Be(Token2);
                FcsClient.TokenExpires.Should().Be(this._expiration2);
                FcsClient.User.Should().Be("testuser");

                A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                            r.ClientSecret == ClientSecret &&
                                                                            r.UserName == null),
                                           A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId)))
                 .MustHaveHappened(Repeated.Exactly.Once);

                A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                            r.ClientSecret == null &&
                                                                            r.UserName == "testuser"),
                                           A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                           h["Authorization"] == "Bearer " + Token)))
                 .MustHaveHappened(Repeated.Exactly.Once);

                fcs.PublishCatalog(new Catalog());

                A.CallTo(() => client.Post(A<Catalog>._,
                                           A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                           h["Authorization"] == "Bearer " + Token2)))
                 .MustHaveHappened(Repeated.Exactly.Once);
            }
        }

        [Fact]
        public void NonWeb_ExistingUserAuth_NewUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName != null),
                                       A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token,
                          Expires = this._expiration
                      });
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                        r.ClientSecret == null &&
                                                                        r.UserName != null),
                                       A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token2,
                          Expires = this._expiration2
                      });

            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppId)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth("testuser1");

            fcs.Auth("testuser2");

            FcsClient.Token.Should().Be(Token2);
            FcsClient.TokenExpires.Should().Be(this._expiration2);
            FcsClient.User.Should().Be("testuser2");

            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName == "testuser1"),
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId)))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                        r.ClientSecret == null &&
                                                                        r.UserName == "testuser2"),
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                       h["Authorization"] == "Bearer " + Token)))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs.PublishCatalog(new Catalog());

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                       h["Authorization"] == "Bearer " + Token2)))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Web_NewAuth_NoUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token,
                          Expires = this._expiration
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppId)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            A.CallTo(() => context.SetResponseCookie(AppId + "-token", Token, this._expiration))
             .MustHaveHappened(Repeated.Exactly.Once);

            var context2 = A.Fake<IContext>();
            A.CallTo(() => context2.CurrentUserName).Returns(null);
            A.CallTo(() => context2.GetRequestCookie(AppId + "-token"))
             .Returns(new HttpCookie(AppId + "-token", Token)
                      {
                          Expires = this._expiration
                      });

            var fcs2 = new FcsClient(ClientId, ClientSecret, AppId)
                       {
                           Context = context2,
                           ServiceClientFactory = factory
                       };


            fcs2.PublishCatalog(new Catalog());

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                       h["Authorization"] == "Bearer " + Token)))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Web_ExistingAuth_NoUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token,
                          Expires = this._expiration
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppId)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            var context2 = A.Fake<IContext>();
            A.CallTo(() => context2.CurrentUserName).Returns(null);
            A.CallTo(() => context2.GetRequestCookie(AppId + "-token"))
             .Returns(new HttpCookie(AppId + "-token", Token)
                      {
                          Expires = this._expiration
                      });

            var fcs2 = new FcsClient(ClientId, ClientSecret, AppId)
                       {
                           Context = context2,
                           ServiceClientFactory = factory
                       };


            fcs2.Auth();
            fcs2.PublishCatalog(new Catalog());

            A.CallTo(() => context2.SetResponseCookie(A<string>._, A<string>._, A<DateTime>._))
             .MustNotHaveHappened();

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                       h["Authorization"] == "Bearer " + Token)))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Web_NewAuth_NewUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns("testuser");

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token,
                          Expires = this._expiration
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppId)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            A.CallTo(() => context.SetResponseCookie(AppId + "-token", Token, this._expiration))
             .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => context.SetResponseCookie(AppId + "-user", "testuser", this._expiration))
             .MustHaveHappened(Repeated.Exactly.Once);

            var context2 = A.Fake<IContext>();
            A.CallTo(() => context2.CurrentUserName).Returns("testuser");
            A.CallTo(() => context2.GetRequestCookie(AppId + "-token"))
             .Returns(new HttpCookie(AppId + "-token", Token)
                      {
                          Expires = this._expiration
                      });
            A.CallTo(() => context2.GetRequestCookie(AppId + "-user"))
             .Returns(new HttpCookie(AppId + "-user", "testuser"));

            var fcs2 = new FcsClient(ClientId, ClientSecret, AppId)
                       {
                           Context = context2,
                           ServiceClientFactory = factory
                       };


            fcs2.PublishCatalog(new Catalog());
            FcsClient.Token.Should().Be(Token);
            FcsClient.TokenExpires.Should().Be(this._expiration);
            FcsClient.User.Should().Be("testuser");

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                       h["Authorization"] == "Bearer " + Token)))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Web_ExistingAppAuth_NewUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName == null),
                                       A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token,
                          Expires = this._expiration
                      });
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                        r.ClientSecret == null &&
                                                                        r.UserName != null),
                                       A<Dictionary<string, string>>._))
             .Returns(new AuthResponse
                      {
                          Token = Token2,
                          Expires = this._expiration2
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppId)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            A.CallTo(() => context.SetResponseCookie(AppId + "-token", Token, this._expiration))
             .MustHaveHappened(Repeated.Exactly.Once);

            var context2 = A.Fake<IContext>();
            A.CallTo(() => context2.CurrentUserName).Returns("testuser");
            A.CallTo(() => context2.GetRequestCookie(AppId + "-token"))
             .Returns(new HttpCookie(AppId + "-token", Token)
                      {
                          Expires = this._expiration
                      });
            A.CallTo(() => context2.GetRequestCookie(AppId + "-user"))
             .Returns(null);

            var fcs2 = new FcsClient(ClientId, ClientSecret, AppId)
                       {
                           Context = context2,
                           ServiceClientFactory = factory
                       };

            fcs2.Auth();
            A.CallTo(() => context2.SetResponseCookie(AppId + "-user", "testuser", this._expiration2))
             .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => context2.SetResponseCookie(AppId + "-token", Token2, this._expiration2))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs2.PublishCatalog(new Catalog());
            FcsClient.Token.Should().Be(Token2);
            FcsClient.TokenExpires.Should().Be(this._expiration2);
            FcsClient.User.Should().Be("testuser");

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                       h["Authorization"] == "Bearer " + Token2)))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Web_ExistingUserAuth_NewUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns("testuser1");

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName != null),
                                       A<Dictionary<string, string>>._))
             .Returns(new AuthResponse {
                 Token = Token,
                 Expires = this._expiration
             });
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                        r.ClientSecret == null &&
                                                                        r.UserName != null),
                                       A<Dictionary<string, string>>._))
             .Returns(new AuthResponse {
                 Token = Token2,
                 Expires = this._expiration2
             });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppId) {
                Context = context,
                ServiceClientFactory = factory
            };
            fcs.Auth();

            A.CallTo(() => context.SetResponseCookie(AppId + "-token", Token, this._expiration))
             .MustHaveHappened(Repeated.Exactly.Once);

            var context2 = A.Fake<IContext>();
            A.CallTo(() => context2.CurrentUserName).Returns("testuser2");
            A.CallTo(() => context2.GetRequestCookie(AppId + "-token"))
             .Returns(new HttpCookie(AppId + "-token", Token) {
                 Expires = this._expiration
             });
            A.CallTo(() => context2.GetRequestCookie(AppId + "-user"))
             .Returns(null);

            var fcs2 = new FcsClient(ClientId, ClientSecret, AppId) {
                Context = context2,
                ServiceClientFactory = factory
            };

            fcs2.Auth();
            A.CallTo(() => context2.SetResponseCookie(AppId + "-user", "testuser2", this._expiration2))
             .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => context2.SetResponseCookie(AppId + "-token", Token2, this._expiration2))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs2.PublishCatalog(new Catalog());
            FcsClient.Token.Should().Be(Token2);
            FcsClient.TokenExpires.Should().Be(this._expiration2);
            FcsClient.User.Should().Be("testuser2");

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Dictionary<string, string>>.That.Matches(h => h["X-Fcs-App"] == AppId &&
                                                                                       h["Authorization"] == "Bearer " + Token2)))
             .MustHaveHappened(Repeated.Exactly.Once);
        }
    }

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming
}
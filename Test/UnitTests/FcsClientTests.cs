// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Web;
using FakeItEasy;
using Fcs;
using Fcs.Framework;
using Fcs.Model;
using FluentAssertions;
//using JWT;
using Jose;
using Xunit;

namespace UnitTests {
    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    public class FcsClientTests {
        private const string ClientId = "CLIENTID";
        private const string ClientSecret = "CLIENTSECRET";
        private const string AppPrefix = "tst";
        private const string UserName1 = "testuser1";
        private const string UserName2 = "testuser2";
        private const string Session1 = "SESSION";
        //private const string Token = "TOKEN";
        //private const string Token2 = "TOKEN2";
        //private const string Url = "https://cloud.firebrandtech.com/api/v2";
        private readonly Guid _appId = Guid.NewGuid();
        private readonly string _appToken;
        private readonly DateTime _expiration1;
        private readonly DateTime _expiration2;
        private readonly Guid _userId1 = Guid.NewGuid();
        private readonly Guid _userId2 = Guid.NewGuid();
        private readonly string _userToken1;
        private readonly string _userToken2;

        public FcsClientTests() {
            this._expiration1 = DateTime.UtcNow.AddDays(1);
            this._expiration2 = DateTime.UtcNow.AddDays(2);
            this._appToken =  Jose.JWT.Encode(new AuthToken
                                                 {
                                                     ApplicationId = this._appId,
                                                     Expires = this._expiration1,
                                                     AppPrefix = AppPrefix,
                                                     SessionId = Session1
                                                 },
                                                 ClientSecret, 
                                                 JwsAlgorithm.HS256);

            this._userToken1 = Jose.JWT.Encode(new AuthToken
                                                   {
                                                       ApplicationId = this._appId,
                                                       UserId = this._userId1,
                                                       UserName = UserName1,
                                                       Expires = this._expiration1,
                                                       AppPrefix = AppPrefix,
                                                       SessionId = Session1
                                                   },
                                                   ClientSecret,
                                                   JwsAlgorithm.HS256);

            this._userToken2 = Jose.JWT.Encode(new AuthToken
                                                   {
                                                       ApplicationId = this._appId,
                                                       UserId = this._userId2,
                                                       UserName = UserName2,
                                                       Expires = this._expiration2,
                                                       AppPrefix = AppPrefix,
                                                       SessionId = Session1
                                                   },
                                                   ClientSecret,
                                                   JwsAlgorithm.HS256);

            FcsClient.Reset();
        }

        [Fact]
        public void NonWeb_NewAuth_NoUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Headers>._, A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._appToken,
                          Expires = this._expiration1
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppPrefix)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            fcs.Access.Token.Should().Be(this._appToken);
            fcs.Access.Expires.Should().Be(this._expiration1);

            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName == null),
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs.UpdateCatalog(new Catalog());

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                    h["Authorization"] == "Bearer " + this._appToken),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void NonWeb_ExistingAuth_NoUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Headers>._, A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._appToken,
                          Expires = this._expiration1
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppPrefix)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            fcs.Auth();

            fcs.Access.Token.Should().Be(this._appToken);
            fcs.Access.Expires.Should().Be(this._expiration1);

            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName == null),
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs.UpdateCatalog(new Catalog());

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                    h["Authorization"] == "Bearer " + this._appToken),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);
            fcs.Auth();
        }

        [Fact]
        public void NonWeb_NewAuth_NewUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Headers>._, A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._userToken1,
                          Expires = this._expiration1,
                          UserName = UserName1
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppPrefix)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth(UserName1);

            fcs.Access.Token.Should().Be(this._userToken1);
            fcs.Access.Expires.Should().Be(this._expiration1);
            fcs.Access.User.Should().Be(UserName1);

            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName == UserName1),
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs.UpdateCatalog(new Catalog());

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                    h["Authorization"] == "Bearer " + this._userToken1),
                                       A<Headers>._))
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
                                       A<Headers>._,
                                       A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._appToken,
                          Expires = this._expiration1
                      });
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                        r.ClientSecret == null &&
                                                                        r.UserName != null),
                                       A<Headers>._,
                                       A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._userToken1,
                          Expires = this._expiration1,
                          UserName = UserName1
                      });

            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            using (var fcs = new FcsClient(ClientId, ClientSecret, AppPrefix)
                             {
                                 Context = context,
                                 ServiceClientFactory = factory
                             }) {
                fcs.Auth();

                fcs.Auth("testuser");

                fcs.Access.Token.Should().Be(this._userToken1);
                fcs.Access.Expires.Should().Be(this._expiration1);
                fcs.Access.User.Should().Be(UserName1);

                A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                            r.ClientSecret == ClientSecret &&
                                                                            r.UserName == null),
                                           A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix),
                                           A<Headers>._))
                 .MustHaveHappened(Repeated.Exactly.Once);

                A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                            r.ClientSecret == null &&
                                                                            r.UserName == "testuser"),
                                           A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                        h["Authorization"] == "Bearer " + this._appToken),
                                           A<Headers>._))
                 .MustHaveHappened(Repeated.Exactly.Once);

                fcs.UpdateCatalog(new Catalog());

                A.CallTo(() => client.Post(A<Catalog>._,
                                           A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                        h["Authorization"] == "Bearer " + this._userToken1),
                                           A<Headers>._))
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
                                       A<Headers>._,
                                       A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._userToken1,
                          Expires = this._expiration1,
                          UserName = UserName1
                      });
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                        r.ClientSecret == null &&
                                                                        r.UserName != null),
                                       A<Headers>._,
                                       A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._userToken2,
                          Expires = this._expiration2,
                          UserName = UserName2
                      });

            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppPrefix)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth(UserName1);

            fcs.Auth(UserName2);

            fcs.Access.Token.Should().Be(this._userToken2);
            fcs.Access.Expires.Should().Be(this._expiration2);
            fcs.Access.User.Should().Be("testuser2");

            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName == UserName1),
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                        r.ClientSecret == null &&
                                                                        r.UserName == UserName2),
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                    h["Authorization"] == "Bearer " + this._userToken1),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs.UpdateCatalog(new Catalog());

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                    h["Authorization"] == "Bearer " + this._userToken2),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Web_NewAuth_NoUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Headers>._, A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._appToken,
                          Expires = this._expiration1
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppPrefix)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            A.CallTo(() => context.SetResponseCookie(AppPrefix + "-token", this._appToken, null))
             .MustHaveHappened(Repeated.Exactly.Once);

            var context2 = A.Fake<IContext>();
            A.CallTo(() => context2.CurrentUserName).Returns(null);
            A.CallTo(() => context2.GetRequestCookie(AppPrefix + "-token"))
             .Returns(new HttpCookie(AppPrefix + "-token", this._appToken));

            var fcs2 = new FcsClient(ClientId, ClientSecret, AppPrefix)
                       {
                           Context = context2,
                           ServiceClientFactory = factory
                       };


            fcs2.UpdateCatalog(new Catalog());

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                    h["Authorization"] == "Bearer " + this._appToken),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Web_ExistingAuth_NoUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(null);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Headers>._, A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._appToken,
                          Expires = this._expiration1
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppPrefix)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            var context2 = A.Fake<IContext>();
            A.CallTo(() => context2.CurrentUserName).Returns(null);
            A.CallTo(() => context2.GetRequestCookie(AppPrefix + "-token"))
             .Returns(new HttpCookie(AppPrefix + "-token", this._userToken1));

            var fcs2 = new FcsClient(ClientId, ClientSecret, AppPrefix)
                       {
                           Context = context2,
                           ServiceClientFactory = factory
                       };


            fcs2.UpdateCatalog(new Catalog());

            //A.CallTo(() => context2.SetResponseCookie(A<string>._, A<string>._, A<DateTime>._))
            // .MustNotHaveHappened();

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                    h["Authorization"] == "Bearer " + this._userToken1),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Web_NewAuth_NewUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(UserName1);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>._, A<Headers>._, A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._userToken1,
                          Expires = this._expiration1,
                          UserName = "testuser"
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppPrefix)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            A.CallTo(() => context.SetResponseCookie(AppPrefix + "-token", this._userToken1, null))
             .MustHaveHappened(Repeated.Exactly.Once);

            var context2 = A.Fake<IContext>();
            A.CallTo(() => context2.CurrentUserName).Returns(UserName1);
            A.CallTo(() => context2.GetRequestCookie(AppPrefix + "-token"))
             .Returns(new HttpCookie(AppPrefix + "-token", this._userToken1));

            var fcs2 = new FcsClient(ClientId, ClientSecret, AppPrefix)
                       {
                           Context = context2,
                           ServiceClientFactory = factory
                       };


            fcs2.UpdateCatalog(new Catalog());
            fcs2.Access.Token.Should().Be(this._userToken1);
            //fcs2.Access.Expires.Should().Be(this._expiration);
            fcs2.Access.User.Should().Be(UserName1);

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                    h["Authorization"] == "Bearer " + this._userToken1),
                                       A<Headers>._))
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
                                       A<Headers>._,
                                       A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._appToken,
                          Expires = this._expiration1
                      });
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                        r.ClientSecret == null &&
                                                                        r.UserName != null),
                                       A<Headers>._,
                                       A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._userToken1,
                          Expires = this._expiration2,
                          UserName = "testuser"
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppPrefix)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            A.CallTo(() => context.SetResponseCookie(AppPrefix + "-token", this._appToken, null))
             .MustHaveHappened(Repeated.Exactly.Once);

            var context2 = A.Fake<IContext>();
            A.CallTo(() => context2.CurrentUserName).Returns(UserName1);
            A.CallTo(() => context2.GetRequestCookie(AppPrefix + "-token"))
             .Returns(new HttpCookie(AppPrefix + "-token", this._userToken1));

            var fcs2 = new FcsClient(ClientId, ClientSecret, AppPrefix)
                       {
                           Context = context2,
                           ServiceClientFactory = factory
                       };

            //fcs2.Auth();
            fcs2.UpdateCatalog(new Catalog());
            A.CallTo(() => context2.SetResponseCookie(AppPrefix + "-token", this._userToken1, null))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs2.Access.Token.Should().Be(this._userToken1);
            fcs2.Access.Expires.Should().BeCloseTo(this._expiration1, 1000);
            fcs2.Access.User.Should().Be(UserName1);

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                    h["Authorization"] == "Bearer " + this._userToken1),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Web_ExistingUserAuth_NewUser() {
            var context = A.Fake<IContext>();
            A.CallTo(() => context.CurrentUserName).Returns(UserName1);

            var client = A.Fake<IServiceClient>();
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == ClientId &&
                                                                        r.ClientSecret == ClientSecret &&
                                                                        r.UserName != null),
                                       A<Headers>._,
                                       A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._userToken1,
                          Expires = this._expiration1,
                          UserName = UserName1
                      });
            A.CallTo(() => client.Post(A<AuthRequest>.That.Matches(r => r.ClientId == null &&
                                                                        r.ClientSecret == null &&
                                                                        r.UserName != null),
                                       A<Headers>._,
                                       A<Headers>._))
             .Returns(new AuthResponse
                      {
                          Token = this._userToken2,
                          Expires = this._expiration2,
                          UserName = UserName2
                      });
            var factory = A.Fake<IServiceClientFactory>();
            A.CallTo(() => factory.CreateClient(A<string>._))
             .Returns(client);

            var fcs = new FcsClient(ClientId, ClientSecret, AppPrefix)
                      {
                          Context = context,
                          ServiceClientFactory = factory
                      };
            fcs.Auth();

            A.CallTo(() => context.SetResponseCookie(AppPrefix + "-token", this._userToken1, null))
             .MustHaveHappened(Repeated.Exactly.Once);

            var context2 = A.Fake<IContext>();
            A.CallTo(() => context2.CurrentUserName).Returns(UserName2);
            A.CallTo(() => context2.GetRequestCookie(AppPrefix + "-token"))
             .Returns(new HttpCookie(AppPrefix + "-token", this._userToken2));

            var fcs2 = new FcsClient(ClientId, ClientSecret, AppPrefix)
                       {
                           Context = context2,
                           ServiceClientFactory = factory
                       };

            fcs2.Auth();
            A.CallTo(() => context2.SetResponseCookie(AppPrefix + "-token", this._userToken2, null))
             .MustHaveHappened(Repeated.Exactly.Once);

            fcs2.UpdateCatalog(new Catalog());
            fcs2.Access.Token.Should().Be(this._userToken2);
            fcs2.Access.Expires.Should().BeCloseTo(this._expiration2, 1000);
            fcs2.Access.User.Should().Be(UserName2);

            A.CallTo(() => client.Post(A<Catalog>._,
                                       A<Headers>.That.Matches(h => h["X-Fcs-App"] == AppPrefix &&
                                                                    h["Authorization"] == "Bearer " + this._userToken2),
                                       A<Headers>._))
             .MustHaveHappened(Repeated.Exactly.Once);
        }
    }

    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming
}
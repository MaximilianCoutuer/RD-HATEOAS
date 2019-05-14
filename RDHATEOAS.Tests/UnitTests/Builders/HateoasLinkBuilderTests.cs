﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using RDHATEOAS.Builders;
using RDHATEOAS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Xunit;

namespace RDHATEOAS.Tests.UnitTests.Builders
{
    public class LinkBuilderFixture : IDisposable
    {
        public readonly HateoasLinkBuilder _linkBuilder;
        public readonly ActionContext _mockContext;
        public readonly UrlHelper _urlHelper;
        public readonly RouteData _routeData;

        public LinkBuilderFixture()
        {
            // "We all know how painful it is to mock a HttpContext"
            _mockContext = new ActionContext();
            _mockContext.HttpContext = new DefaultHttpContext();
            _routeData = new RouteData();
            //_routeData.Values.Add("")
            _mockContext.RouteData = _routeData;

    //        var routes = new RouteCollection();
    //        routes.MapRoute(
    //name: "default",
    //template: "api/{controller=Home}/{action=Index}/{id?}");

            _urlHelper = new UrlHelper(_mockContext);
            _linkBuilder = new HateoasLinkBuilder(_urlHelper);
        }

        public void Dispose()
        {
        }
    }

    public class HateoasLinkBuilderTests : IClassFixture<LinkBuilderFixture>
    {
        LinkBuilderFixture _fixture;

        public HateoasLinkBuilderTests(LinkBuilderFixture fixture)
        {
            this._fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(ValidLinkData))]
        public void BuildLink_ShouldBuildLink(string routeUrl, string routeUrlController, string routeUrlAction, string linkRef, HttpMethod linkMethod, int linkId)
        {
            // arrange

            // act
            HateoasLink link = _fixture._linkBuilder.Build(_fixture._mockContext, routeUrl, routeUrlController, routeUrlAction, linkRef, linkMethod, linkId);

            // assert
            //Uri uriResult;
            //bool refIsLink = Uri.TryCreate(link.Href, UriKind.RelativeOrAbsolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;

            // TODO: Crash in UrlHelper.RouteUrl, presumably because routing is null??

            Assert.True(Uri.IsWellFormedUriString(link.Href, UriKind.RelativeOrAbsolute));
            Assert.Equal(link.Method, linkMethod.Method);
        }

        [Theory]
        [MemberData(nameof(NullLinkData))]
        public void BuildLink_NullData_ShouldThrow(string routeUrl, string routeUrlController, string routeUrlAction, string linkRef, HttpMethod linkMethod, int linkId)
        {
            // arrange

            // act

            // assert
            Assert.Throws<ArgumentNullException>(() => _fixture._linkBuilder.Build(_fixture._mockContext, routeUrl, routeUrlController, routeUrlAction, linkRef, linkMethod, linkId));
        }

        #region helpers

        public static IEnumerable<object[]> ValidLinkData
        {
            get
            {
                return new[]
                {
                    new object[] { "default", "Person", "", "details", HttpMethod.Get, 5 },
                    new object[] { "default", "person", "", "list", HttpMethod.Get, null },
            };
            }
        }

        public static IEnumerable<object[]> NullLinkData
        {
            get
            {
                return new[]
                {
                    new object[] { null, "Person", "", "details", HttpMethod.Get, 5 },
                    new object[] { "default", null, "", "details", HttpMethod.Get, 5 },
                    new object[] { "default", "Person", null, "details", HttpMethod.Get, 5 },
                    new object[] { "default", "Person", "",  null, HttpMethod.Get, 5 },
                    new object[] { "default", "Person", null, "details", null, 5 },
            };
            }
        }

        #endregion
    }
}

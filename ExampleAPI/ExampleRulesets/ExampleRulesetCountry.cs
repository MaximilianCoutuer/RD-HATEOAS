﻿using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Rhateoas.Models;

namespace Rhateoas.Rulesets
{
    /// <summary>
    /// An example Ruleset that adds a number of relevant links to a Country entity.
    /// As there is no Country controller, we pretend for educational purposes that it uses query string parameters.
    /// <list type="bullet">
    /// <item>
    /// <term>Index</term>
    /// <description>A link to the list of Persons. Note the use of a custom domain name.</description>
    /// </item>
    /// <item>
    /// <term>Edit</term>
    /// <description>A link to Edit this Country. Note the use of ExtendQueryString() and item["Id"].</description>
    /// </item>
    /// <item>
    /// <term>Delete</term>
    /// <description>A link to Delete this Country. Note the use of ExtendQueryString() and item["Id"].</description>
    /// </item>
    /// </list>
    /// </summary>
    public class ExampleRulesetCountry : HateoasRulesetBase
    {
        public override bool AppliesToEachListItem { get; set; } = true;

        public override List<HateoasLink> GetLinks(JToken item)
        {
            return new List<HateoasLink>
            {
                HateoasLinkBuilder.Build(Context, "default", "Country", string.Empty, "list", HttpMethod.Get, null, "www.exampledomainname.com")
                    .AddHreflang("nl-be")
                    .AddTitle("List of countries")
                    .AddType("application/json+hal"),
                HateoasLinkBuilder.Build(Context, "default", "Country", "Edit", "edit", HttpMethod.Post)
                    .AddHreflang("nl-be")
                    .AddTitle("Edit this country")
                    .AddType("application/json+hal")
                    .ExtendQueryString("id", item["Id"].ToString()),
                HateoasLinkBuilder.Build(Context, "default", "Country", "Delete", "delete", HttpMethod.Delete)
                    .AddHreflang("nl-be")
                    .AddTitle("Delete this country")
                    .AddType("application/json+hal")
                    .ExtendQueryString("id", item["Id"].ToString()),
            };
        }
    }
}

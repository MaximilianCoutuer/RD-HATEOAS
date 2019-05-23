﻿namespace RDHATEOAS.Rulesets
{
    using System.Collections.Generic;
    using System.Net.Http;
    using ExampleAPI.Models;
    using Newtonsoft.Json.Linq;
    using RDHATEOAS.Models;

    public class ExampleRulesetFullLinksPerson : HateoasRulesetBase
    {
        public override bool AppliesToEachListItem { get; set; } = true;

        public override List<HateoasLink> GetLinks(JToken item)
        {
            return new List<HateoasLink>
            {
                HateoasLinkBuilder.BuildSelfLink(Context)
                    .AddHreflang("be-nl")
                    .AddTitle("Properties of this requested person")
                    .AddType("json"),
                HateoasLinkBuilder.Build(Context, "default", "Person", string.Empty, "list", HttpMethod.Get)
                    .AddHreflang("be-nl")
                    .AddTitle("List of persons")
                    .AddType("json"),
                HateoasLinkBuilder.Build(Context, "default", "Person", "Edit", "edit", HttpMethod.Post, item["Id"])
                    .AddHreflang("be-nl")
                    .AddTitle("Edit this person")
                    .AddType("json"),
                HateoasLinkBuilder.Build(Context, "default", "Person", "Delete", "delete", HttpMethod.Delete)
                    .AddHreflang("be-nl")
                    .AddTitle("Delete this person")
                    .AddType("json")
                    .ExtendQueryString("id", item["Id"].ToString()),
            };
            // Parameters.GetValueOrDefault("RD-ListId")
        }
    }
}
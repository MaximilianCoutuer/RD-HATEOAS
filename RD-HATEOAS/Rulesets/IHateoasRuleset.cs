﻿using Microsoft.AspNetCore.Mvc.Filters;
using RDHATEOAS.Models;
using System;
using System.Collections.Generic;

namespace RDHATEOAS.Rulesets
{
    public interface IHateoasRuleset
    {
        object Parameter { get; set; }
        void AddDescribedLink(ref IsHateoasEnabled item, ResultExecutingContext context, dynamic data);
    }
}

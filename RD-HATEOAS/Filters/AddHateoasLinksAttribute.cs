﻿namespace RDHATEOAS.Filters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Routing;
    using RDHATEOAS.Builders;
    using RDHATEOAS.Extensions;
    using RDHATEOAS.Models;
    using RDHATEOAS.Rulesets;

    /// <summary>
    /// This filter is applied to a controller method via an attribute.
    /// It intercepts the response and adds links to it.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AddHateoasLinksAttribute : ResultFilterAttribute
    {
        #region fields

        private readonly string[] _parameterNames;
        private readonly string[] _path;
        private readonly List<IHateoasRuleset> _rulesets = new List<IHateoasRuleset>();
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        private UrlHelper urlHelper;
        private HateoasLinkBuilder hateoasLinkBuilder;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AddHateoasLinksAttribute"/> class.
        /// </summary>
        /// <param name="parameterNames">Any parameters in the result you wish to pass on to the ruleset.</param>
        /// <param name="rulesetNames">Names of the rulesets you wish to apply to the object.</param>
        /// <param name="path">Path to the object to add links for, as a sequence of keys.</param>
        public AddHateoasLinksAttribute(string[] parameterNames, Type[] rulesetNames, string[] path)
        {
            _parameterNames = parameterNames;
            _path = path;
            foreach (var type in rulesetNames)
            {
                _rulesets.Add((IHateoasRuleset)Activator.CreateInstance(type));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddHateoasLinksAttribute"/> class.
        /// </summary>
        /// <param name="parameterNames"></param>
        /// <param name="rulesetName"></param>
        public AddHateoasLinksAttribute(string[] parameterNames, Type rulesetName, string[] path)
            : this(parameterNames, new Type[] { rulesetName }, path)
        {
        }

        #endregion

        #region methods

        /// <summary>
        /// This method is invoked whenever a result is sent from a controller method decorated with this attribute.
        /// </summary>
        /// <param name="context">The result context from the result that caused this to be run.</param>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is OkObjectResult okObjectResult && okObjectResult.StatusCode == 200)
            {
                var item = RecursiveGetObjectFromPath(okObjectResult.Value, 0);

                urlHelper = new UrlHelper(context);
                hateoasLinkBuilder = new HateoasLinkBuilder(urlHelper);

                if (_parameterNames != null)
                {
                    foreach (string parameterName in _parameterNames)
                    {
                        _parameters[parameterName] = context.RouteData.Values[parameterName] ?? null;
                    }
                }

                if (item.GetType().IsList())
                {
                    // TODO: simplify this?
                    var objectList = new ListHateoasEnabled();
                    var list = item as IList;
                    foreach (object listitem in list)
                    {
                        objectList.List.Add(listitem);
                    }
                    AddLinksToList(context, objectList);
                }
                else
                {
                    AddLinksToObject(context, item as IIsHateoasEnabled);
                }
            }

            base.OnResultExecuting(context);
        }

        private object RecursiveGetObjectFromPath(object currentObjectValue, int pathId)
        {
            if (pathId < _path.Length)
            {
                var currentObjectType = currentObjectValue.GetType();
                if (currentObjectType.IsList())
                {
                    foreach (object listitem in currentObjectValue as IList)
                    {
                        currentObjectType = listitem.GetType(); // TODO error handling
                        var key = _path[pathId];
                        var property = currentObjectType.GetProperty(key);
                        var nestedObjectValue = property.GetValue(listitem);
                        return RecursiveGetObjectFromPath(nestedObjectValue, pathId + 1);
                    }
                }
                else
                {
                    var nestedObjectValue = currentObjectType.GetProperty(_path[pathId]).GetValue(currentObjectValue);
                    return RecursiveGetObjectFromPath(nestedObjectValue, pathId + 1);
                }
            }
            return currentObjectValue;
        }

            //var currentObjectType = okObjectResult.Value.GetType();
            //var currentObjectValue = okObjectResult.Value;

            //// drill into object tree
            //foreach (string key in _path ?? new string[] { })
            //{
            //    if (currentObjectType.IsList())
            //    {
            //        foreach (object objectListItemValue in currentObjectValue as IList)
            //        {
            //            currentObjectValue = currentObjectType.GetProperty(key).GetValue(currentObjectType, null);
            //            currentObjectType = currentObjectValue.GetType();
            //        }
            //    } else
            //    {
            //        currentObjectValue = currentObjectType.GetProperty(key).GetValue(currentObjectType, null);
            //        currentObjectType = currentObjectValue.GetType();
            //    }
            //}

            //return currentObjectValue;

        private void AddLinksToObject(ResultExecutingContext context, IIsHateoasEnabled item)
        {
            foreach (IHateoasRuleset ruleset in _rulesets)
            {
                // set fields in ruleset
                ruleset.SetHelpers(context);
                ruleset.Parameters = _parameters;

                // apply links from ruleset
                foreach (HateoasLink link in ruleset.GetLinks(item))
                {
                    item.Links.Add(link);
                }
            }
        }

        private void AddLinksToList(ResultExecutingContext context, ListHateoasEnabled unformattedList)
        {
            var list = unformattedList.List as IList;
            for (int i = 0; i < list.Count; i++)
            {
                foreach (IHateoasRuleset ruleset in _rulesets.Where(r => r.AppliesToEachListItem == true))
                {
                    // set fields in ruleset to help rulesets make the correct decisions
                    ruleset.SetHelpers(context);
                    ruleset.Parameters = _parameters;
                    ruleset.Parameters["RD-ListId"] = i;
                    ruleset.Parameters["RD-ListCount"] = list.Count;
                    if (list[i] is IIsHateoasEnabled listitem)
                    {
                        // apply links from ruleset
                        foreach (HateoasLink link in ruleset.GetLinks(listitem))
                        {
                            listitem.Links.Add(link);
                        }
                    }
                }
            }

            foreach (IHateoasRuleset ruleset in _rulesets.Where(r => r.AppliesToEachListItem == false))
            {
                // set fields in ruleset
                ruleset.SetHelpers(context);
                ruleset.Parameters = _parameters;
                ruleset.Parameters["RD-ListCount"] = list.Count;

                // apply links from ruleset
                foreach (HateoasLink link in ruleset.GetLinks(unformattedList))
                {
                    unformattedList.Links.Add(link);
                }
            }

        }

        #endregion
    }
}
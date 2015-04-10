﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Quickblox.Sdk.GeneralDataModel.Request
{
    public class SortFilter<T> : Filter
    {
        private readonly SortOperator sortOperator;
        private readonly Expression<Func<T>> selectFieldExpression;

        public string ParameterName => "order";

        public string FormatString => "{0}={1}+{2}+{3}";

        public SortFilter(SortOperator sortOperator, Expression<Func<T>> selectFieldExpression)
        {
            this.sortOperator = sortOperator;
            this.selectFieldExpression = selectFieldExpression;
        }

        public override string BuildFilter()
        {
            var memberExpression = (MemberExpression)this.selectFieldExpression.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;

            var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
            var filedTypeString = GetFilterFieldTypeString(propertyInfo);

            return String.Format(this.FormatString, this.ParameterName, this.sortOperator.ToString().ToLower(), filedTypeString, jsonPropertyAttribute.PropertyName);
        }
    }

    public enum SortOperator
    {
        Asc,
        Desc
    }
}

﻿namespace MvcForum.Web.Application.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    public static class HtmlLabelExtensions
    {
        public static MvcLabel BeginLabel(this HtmlHelper html, string expression)
        {
            return BeginLabel(html,
                expression,
                labelText: null);
        }

        public static MvcLabel BeginLabel(this HtmlHelper html, string expression, string labelText)
        {
            return BeginLabel(html, expression, labelText, null, null);
        }

        public static MvcLabel BeginLabel(this HtmlHelper html, string expression, object htmlAttributes)
        {
            return BeginLabel(html, expression, null, htmlAttributes, null);
        }

        public static MvcLabel BeginLabel(this HtmlHelper html, string expression,
            IDictionary<string, object> htmlAttributes)
        {
            return BeginLabel(html, expression, null, htmlAttributes, null);
        }

        public static MvcLabel BeginLabel(this HtmlHelper html, string expression, string labelText,
            object htmlAttributes)
        {
            return BeginLabel(html, expression, labelText, htmlAttributes, null);
        }

        public static MvcLabel BeginLabel(this HtmlHelper html, string expression, string labelText,
            IDictionary<string, object> htmlAttributes)
        {
            return BeginLabel(html, expression, labelText, htmlAttributes, null);
        }

        internal static MvcLabel BeginLabel(this HtmlHelper html, string expression, string labelText,
            object htmlAttributes, ModelMetadataProvider metadataProvider)
        {
            return BeginLabel(html,
                expression,
                labelText,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes),
                metadataProvider);
        }

        internal static MvcLabel BeginLabel(this HtmlHelper html, string expression, string labelText,
            IDictionary<string, object> htmlAttributes, ModelMetadataProvider metadataProvider)
        {
            return LabelHelper(html,
                // REVIEW: This is an internal method
                // ModelMetadata.FromStringExpression(expression, html.ViewData, metadataProvider),
                ModelMetadata.FromStringExpression(expression, html.ViewData),
                expression,
                labelText,
                htmlAttributes);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification =
            "This is an appropriate nesting of generic types")]
        public static MvcLabel BeginLabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            return BeginLabelFor(html, expression, labelText: null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification =
            "This is an appropriate nesting of generic types")]
        public static MvcLabel BeginLabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression, string labelText)
        {
            return BeginLabelFor(html, expression, labelText, null, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification =
            "This is an appropriate nesting of generic types")]
        public static MvcLabel BeginLabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return BeginLabelFor(html, expression, null, htmlAttributes, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification =
            "This is an appropriate nesting of generic types")]
        public static MvcLabel BeginLabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes)
        {
            return BeginLabelFor(html, expression, null, htmlAttributes, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification =
            "This is an appropriate nesting of generic types")]
        public static MvcLabel BeginLabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression, string labelText, object htmlAttributes)
        {
            return BeginLabelFor(html, expression, labelText, htmlAttributes, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification =
            "This is an appropriate nesting of generic types")]
        public static MvcLabel BeginLabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression, string labelText, IDictionary<string, object> htmlAttributes)
        {
            return BeginLabelFor(html, expression, labelText, htmlAttributes, null);
        }

        internal static MvcLabel BeginLabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression, string labelText, object htmlAttributes,
            ModelMetadataProvider metadataProvider)
        {
            return BeginLabelFor(html,
                expression,
                labelText,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes),
                metadataProvider);
        }

        internal static MvcLabel BeginLabelFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression, string labelText, IDictionary<string, object> htmlAttributes,
            ModelMetadataProvider metadataProvider)
        {
            return LabelHelper(html,
                // REVIEW: This is an internal method
                // ModelMetadata.FromLambdaExpression(expression, html.ViewData, metadataProvider),
                ModelMetadata.FromLambdaExpression(expression, html.ViewData),
                ExpressionHelper.GetExpressionText(expression),
                labelText,
                htmlAttributes);
        }

        public static MvcLabel BeginLabelForModel(this HtmlHelper html)
        {
            return BeginLabelForModel(html, labelText: null);
        }

        public static MvcLabel BeginLabelForModel(this HtmlHelper html, string labelText)
        {
            return LabelHelper(html, html.ViewData.ModelMetadata, string.Empty, labelText);
        }

        public static MvcLabel BeginLabelForModel(this HtmlHelper html, object htmlAttributes)
        {
            return LabelHelper(html, html.ViewData.ModelMetadata, string.Empty, null,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcLabel BeginLabelForModel(this HtmlHelper html, IDictionary<string, object> htmlAttributes)
        {
            return LabelHelper(html, html.ViewData.ModelMetadata, string.Empty, null, htmlAttributes);
        }

        public static MvcLabel BeginLabelForModel(this HtmlHelper html, string labelText, object htmlAttributes)
        {
            return LabelHelper(html, html.ViewData.ModelMetadata, string.Empty, labelText,
                HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcLabel BeginLabelForModel(this HtmlHelper html, string labelText,
            IDictionary<string, object> htmlAttributes)
        {
            return LabelHelper(html, html.ViewData.ModelMetadata, string.Empty, labelText, htmlAttributes);
        }

        internal static MvcLabel LabelHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName,
            string labelText = null, IDictionary<string, object> htmlAttributes = null)
        {
            var resolvedLabelText =
                labelText ?? metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            //if (string.IsNullOrWhiteSpace(resolvedLabelText))
            //{
            //    return MvcHtmlString.Empty;
            //}

            var tag = new TagBuilder("label");
            tag.Attributes.Add("for",
                TagBuilder.CreateSanitizedId(
                    html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)));
            // tag.SetInnerText(resolvedLabelText);
            tag.MergeAttributes(htmlAttributes, true);

            //return tag.ToMvcHtmlString(TagRenderMode.Normal);
            html.ViewContext.Writer.Write(tag.ToString(TagRenderMode.StartTag));
            var theLabel = new MvcLabel(html.ViewContext, resolvedLabelText);

            return theLabel;
        }

        //public static void EndLabel(this HtmlHelper htmlHelper)
        //{
        //    EndLabel(htmlHelper.ViewContext);
        //}

        internal static void EndLabel(ViewContext viewContext, string labelText)
        {
            if (!string.IsNullOrWhiteSpace(labelText))
            {
                viewContext.Writer.Write(labelText);
            }

            viewContext.Writer.Write("</label>");
            viewContext.OutputClientValidation();
            viewContext.FormContext = null;
        }
    }

    public class MvcLabel : IDisposable
    {
        private readonly string _labelText;
        private readonly ViewContext _viewContext;
        private bool _disposed;

        public MvcLabel(ViewContext viewContext, string labelText)
        {
            if (viewContext == null)
            {
                throw new ArgumentNullException("viewContext");
            }

            _viewContext = viewContext;
            _labelText = labelText;
        }

        public void Dispose()
        {
            Dispose(true /* disposing */);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                HtmlLabelExtensions.EndLabel(_viewContext, _labelText);
            }
        }

        public void EndLabel()
        {
            Dispose(true);
        }
    }
}
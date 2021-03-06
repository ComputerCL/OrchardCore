using Markdig;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure.Html;
using Xunit;

namespace OrchardCore.Tests.Html
{
    public class HtmlSanitizerTests
    {
        private static readonly HtmlSanitizerService _sanitizer = new HtmlSanitizerService(Options.Create(new HtmlSanitizerOptions()));

        [Theory]
        [InlineData("<script>alert('xss')</script><div onload=\"alert('xss')\">Test<img src=\"test.gif\" style=\"background-image: url(javascript:alert('xss')); margin: 10px\"></div>", "<div>Test<img src=\"test.gif\" style=\"margin: 10px\"></div>")]
        [InlineData("<IMG SRC=javascript:alert(\"XSS\")>", @"<img>")]
        [InlineData("<a href=\"javascript: alert('xss')\">Click me</a>", @"<a>Click me</a>")]
        public void ShouldSanitizeHTML(string html, string sanitized)
        {
            // Setup
            var output = _sanitizer.Sanitize(html);

            // Test
            Assert.Equal(output, sanitized);
        }

        [Fact]
        public void ShouldConfigureSanitizer()
        {
            var services = new ServiceCollection();
            services.Configure<HtmlSanitizerOptions>(o =>
            {
                o.Configure = (sanitizer) =>
                {
                    sanitizer.AllowedAttributes.Add("class");
                };
            });

            services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

            var sanitizer = services.BuildServiceProvider().GetService<IHtmlSanitizerService>();

            var input = @"<a href=""bar"" class=""foo"">baz</a>";
            var sanitized = sanitizer.Sanitize(input);
            Assert.Equal(input, sanitized);
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using OutOfProcessExtension;

[assembly: WebJobsStartup(typeof(CustomBindingExampleStartup))]

namespace OutOfProcessExtension
{
    public class CustomBindingExampleStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<AppendFileExtensionConfigProvider>();
        }
    }

    [Extension("AppendFile")]
    internal class AppendFileExtensionConfigProvider : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            var bindingRule = context.AddBindingRule<AppendFileAttribute>();
            bindingRule.BindToCollector(attribute => new AppendFileAsyncCollector(attribute));
        }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class AppendFileAttribute : Attribute
    {
        public string Path { get; set; }
    }

    public class AppendFileAsyncCollector : IAsyncCollector<string>
    {
        private readonly AppendFileAttribute _appendFileAttribute;

        public AppendFileAsyncCollector(AppendFileAttribute appendFileAttribute)
        {
            _appendFileAttribute = appendFileAttribute;
        }

        public async Task AddAsync(string item, CancellationToken cancellationToken = new CancellationToken())
        {
            File.AppendAllText(_appendFileAttribute.Path, item + Environment.NewLine);
        }

        public Task FlushAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }
    }
}

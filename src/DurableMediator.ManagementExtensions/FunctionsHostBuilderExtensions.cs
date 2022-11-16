using DurableMediator.Analyzer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

namespace DurableMediator.ManagementExtensions;

public static class FunctionsHostBuilderExtensions
{
    public static void AddDurableMediatorManagement(
        this IFunctionsHostBuilder builder)
    {
        var config = builder.GetContext().Configuration;


    }
}


public interface IWorkflowMetadata
{
    public WorkflowMetadata GetWorkflowMetadata<TWorkflowType>();
}

public class WorkflowMetadata : IWorkflowMetadata
{
    private readonly IEnumerable<IWorkflowMetadataProvider> _workflowMetadataProviders;

    public WorkflowMetadata(
        IEnumerable<IWorkflowMetadataProvider> workflowMetadataProviders)
    {
        _workflowMetadataProviders = workflowMetadataProviders;
    }

    public WorkflowMetadata GetWorkflowMetadata<TWorkflowType>()
    {
        throw new NotImplementedException();
    }
}

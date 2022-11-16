using System;
using System.Collections.Generic;

namespace DurableMediator.Analyzer
{
    public sealed class AnalyzeFlowAttribute : Attribute
    {
    }

    public interface IWorkflowMetadataProvider
    {
        WorkflowMetadata GetFlow();
    }

    public class WorkflowMetadata
    {
        public Type WorkflowType { get; set; }
        public List<WorkflowStep> Steps { get; set; }
    }

    public abstract class WorkflowStep
    {
        public abstract string Name { get; }
    }

    public class SendStep : WorkflowStep
    {
        public override string Name => "Send";

        public Type? Request { get; set; }
        public int Retries { get; set; }
    }

    public abstract class StepDescription
    {

    }

    public class SendStepDescription : StepDescription
    {
        public SendStepDescription(string requestType, int retries)
        {
            RequestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
            Retries = retries;
        }

        public string RequestType { get; set; }
        public int Retries { get; set; }
    }

    public class TimerStep : WorkflowStep
    {
        public override string Name => "Timer";
    }

    public class SubWorkflowStep : WorkflowStep
    {
        public override string Name => "SubWorkflow";
    }

    public class LoopStep : WorkflowStep
    {
        public override string Name => "Loop";

        public List<WorkflowStep> Steps { get; set; }
    }

    public class BranchStep : WorkflowStep
    {
        public override string Name => "Branch";

        public List<List<WorkflowStep>> BranchedSteps { get; set; }
    }

    public class TryStep : WorkflowStep
    {
        public override string Name => "Try";

        public List<WorkflowStep> TrySteps { get; set; }

        public List<WorkflowStep> CatchSteps { get; set; }

        public List<WorkflowStep> FinallySteps { get; set; }
    }
}

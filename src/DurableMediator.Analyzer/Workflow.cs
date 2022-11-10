using System;
using System.Collections.Generic;
using System.Text;

namespace DurableMediator.Analyzer
{
    public sealed class AnalyzeFlowAttribute : Attribute
    {
    }

    public interface IWorkflowMetadataProvider<TWorkflow>
    {
        WorkflowFlow<TWorkflow> GetFlow();
    }

    public class WorkflowFlow<TWorkflow>
    {
        public List<WorkflowStep> Steps { get; set; }
    }

    public abstract class WorkflowStep
    {
        public string Name { get; set; }
    }

    public class SendStep : WorkflowStep
    {
        public Type Request { get; set; }
        public Type Response { get; set; } // TODO: should be dynamic
        public int Retries { get; set; }
    }

    public class TimerStep : WorkflowStep
    {

    }

    public class SubWorkflowStep : WorkflowStep
    {

    }

    public class LoopStep : WorkflowStep
    {
        public List<WorkflowStep> Steps { get; set; }
    }

    public class BranchStep : WorkflowStep
    {
        public List<List<WorkflowStep>> BranchedSteps { get; set; }
    }
}

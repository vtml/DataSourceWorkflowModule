using System;
using System.Runtime.Serialization;
using HI.Shared.DataSourceWorkflowModule.Extensions;
using HI.Shared.DataSourceWorkflowModule.Models;
using Sitecore.Data.Validators;

namespace HI.Shared.DataSourceWorkflowModule.Validators
{
    [Serializable]
    public class ComponentsinFinalWorkflow : StandardValidator
    {
        public ComponentsinFinalWorkflow(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        public ComponentsinFinalWorkflow()
        {
        }
        protected override ValidatorResult Evaluate()
        {
            
            var item = this.GetItem();
            var result = ValidatorResult.Valid;
            Text = "";
            

            foreach (var ds in item.GetAllUniqueDataSourceItems())
            {
                var wfModel = new ItemWorkflowModel(ds);
                
                if (wfModel.NoNulls && !wfModel.WorkflowState.FinalState)
                {
                    
                    var path = ds.Paths.ContentPath;
                    Text += GetText("<div>The item in this path <b>\"{0}\"</b> must be in the final workflow state</div>", path);
                    result = ValidatorResult.Warning;
                    
                }

            }

            return result;
        }

        protected override ValidatorResult GetMaxValidatorResult()
        {
            return ValidatorResult.Warning;
        }

        public override string Name
        {
            get { return "Component Workflow State"; }
        }


    }
}
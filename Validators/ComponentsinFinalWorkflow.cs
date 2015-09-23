using System;
using System.Linq;
using System.Runtime.Serialization;
using HI.Shared.DataSourceWorkflowModule.Extensions;
using HI.Shared.DataSourceWorkflowModule.Models;
using Sitecore.Data.Validators;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HI.Shared.DataSourceWorkflowModule.Validators
{
    [Serializable]
    public class ComponentsinFinalWorkflow : StandardValidator
    {
        #region Properties

        public override string Name
        {
            get { return "Component Workflow State"; }
        }

        #endregion

        #region Constructor

        public ComponentsinFinalWorkflow(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ComponentsinFinalWorkflow()
        {
        }

        #endregion

        #region Methods

        protected override ValidatorResult Evaluate()
        {
            var item = GetItem();
            var result = ValidatorResult.Valid;
            var paths = new List<string>();
            foreach (var ds in item.GetAllUniqueDataSourceItems())
            {
                var wfModel = new ItemWorkflowModel(ds);
                if (wfModel.NoNulls && !wfModel.WorkflowState.FinalState)
                {
                    paths.Add(ds.Paths.ContentPath);
                    result = GetMaxValidatorResult();
                }
            }

            this.Text = string.Empty;
            if (paths.Count > 0)
            {
                Text += GetText("The item{0} {1} should be in a final workflow state.", paths.Count > 1 ? "s" : string.Empty, GetPathsString(paths));
            }

            return result;
        }

        private string GetPathsString(List<string> paths)
        {
            string formatString = "\"{0}\"{1}";
            if (paths.Count == 1)
            {
                return string.Format(formatString, paths[0], string.Empty);
            }
            else if (paths.Count == 2)
            {
                return string.Format(formatString, paths[0], " and ") + string.Format(formatString, paths[1], string.Empty);
            }

            string pathsString = string.Empty;
            string separator = ", ";
            for (int i = 0; i < paths.Count; i++)
            {
                if (i == paths.Count - 1)
                {
                    separator = string.Empty;
                }
                if (i == paths.Count - 2)
                {
                    separator = ", and ";
                }
                pathsString += string.Format(formatString, paths[i], separator);
            }
            return pathsString;
        }

        protected override ValidatorResult GetMaxValidatorResult()
        {
            return ValidatorResult.Warning;
        }

        #endregion
    }
}
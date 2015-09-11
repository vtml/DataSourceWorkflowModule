using HI.Shared.DataSourceWorkflowModule.Extensions;
using HI.Shared.DataSourceWorkflowModule.Models;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetContentEditorWarnings;
using Sitecore.Shell.Framework.CommandBuilders;
using Sitecore.Workflows;

namespace HI.Shared.DataSourceWorkflowModule.Pipeline.GetContentEditorWarnings
{
    public class GetDataSourceWorkflowNotification
    {
        #region Methods

        public void Process(GetContentEditorWarningsArgs arguments)
        {
            Assert.ArgumentNotNull((object)arguments, "arguments");
            if (arguments.Item != null)
            {
                foreach (Item ds in arguments.Item.GetAllUniqueDataSourceItems())
                {
                    GetNotifications(arguments, ds);
                }
            }
        }

        public void GetNotifications(GetContentEditorWarningsArgs arguments, Item contextItem)
        {
            if (arguments == null) return;
            var wfModel = new ItemWorkflowModel(contextItem);
            if (wfModel.ShowNotification)
            {
                SetNotification(arguments, wfModel);
            }
        }

        private void SetNotification(GetContentEditorWarningsArgs arguments, ItemWorkflowModel wfModel)
        {
            var editorNotification = arguments.Add();
            editorNotification.Title = "Datasource Item in Workflow";
            editorNotification.Text = wfModel.GetEditorDescription(false);
            editorNotification.Icon = wfModel.WorkflowState.Icon;
            if (wfModel.HasWriteAccess())
            {
                foreach (WorkflowCommand command in wfModel.Commands)
                {
                    editorNotification.AddOption(command.DisplayName, new WorkflowCommandBuilder(wfModel.ContextItem, wfModel.Workflow, command).ToString());
                }
            }
        }

        #endregion

    }
}

using HI.Shared.DataSourceWorkflowModule.Extensions;
using HI.Shared.DataSourceWorkflowModule.Models;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetPageEditorNotifications;
using Sitecore.Shell.Framework.CommandBuilders;
using Sitecore.Workflows;

namespace HI.Shared.DataSourceWorkflowModule.Pipeline.GetPageEditorNotifications
{
    public class GetDataSourceWorkflowNotification : GetPageEditorNotificationsProcessor
    {
        #region Methods

        public override void Process(GetPageEditorNotificationsArgs arguments)
        {
            Assert.ArgumentNotNull((object)arguments, "arguments");
            if (arguments.ContextItem != null)
            {
                foreach (Item ds in arguments.ContextItem.GetAllUniqueDataSourceItems())
                {
                    GetPageEditorNotificationsArgs dataSourceArguments = new GetPageEditorNotificationsArgs(ds);
                    GetNotifications(dataSourceArguments);
                    arguments.Notifications.AddRange(dataSourceArguments.Notifications);
                }
            }
        }

        public void GetNotifications(GetPageEditorNotificationsArgs arguments)
        {
            if (arguments == null) return;
            var wfModel = new ItemWorkflowModel(arguments.ContextItem);
            if (wfModel.ShowNotification)
            {
                SetNotification(arguments, wfModel);
            }
        }

        private void SetNotification(GetPageEditorNotificationsArgs arguments, ItemWorkflowModel wfModel)
        {
            PageEditorNotification editorNotification = new PageEditorNotification(wfModel.GetEditorDescription(), PageEditorNotificationType.Warning)
            {
                Icon = wfModel.WorkflowState.Icon
            };
            // editorNotification.Options.Add(new PageEditorNotificationOption("View in Content Editor", ""));
            if (wfModel.HasWriteAccess())
            {
                foreach (WorkflowCommand command in wfModel.Commands)
                {
                    PageEditorNotificationOption notificationOption = new PageEditorNotificationOption(command.DisplayName, new WorkflowCommandBuilder(wfModel.ContextItem, wfModel.Workflow, command).ToString());
                    editorNotification.Options.Add(notificationOption);
                }
            }
            arguments.Notifications.Add(editorNotification);
        }

        #endregion

    }
}

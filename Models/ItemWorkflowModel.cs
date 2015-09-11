using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Security.AccessControl;
using Sitecore.SecurityModel;
using Sitecore.Workflows;

namespace HI.Shared.DataSourceWorkflowModule.Models
{
    public class ItemWorkflowModel
    {
        #region Fields

        private Item _contextItem;
        private Database _database;
        private IWorkflowProvider _workflowProvider;
        private IWorkflow _workflow;
        private WorkflowState _workflowState;
        private Item _workflowStateItem;
        WorkflowCommand[] _commands;
        private bool? _showWorkflowNoAccessMessage;
        private const string _showWorkflowNoAccessMessageKey = "DatasourceWorkflowNotification.ShowWorkflowNoAccessMessage";

        #endregion

        #region Properties

        public Item ContextItem
        {
            get
            {
                return _contextItem;
            }
        }

        public Database Database
        {
            get
            {
                return _database;
            }
        }

        public IWorkflowProvider WorkflowProvider
        {
            get
            {
                return _workflowProvider;
            }
        }

        public IWorkflow Workflow
        {
            get
            {
                return _workflow;
            }
        }
        public WorkflowState WorkflowState
        {
            get
            {
                return _workflowState;
            }
        }

        public Item WorkflowStateItem
        {
            get
            {
                return _workflowStateItem;
            }
        }

        public WorkflowCommand[] Commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = WorkflowFilterer.FilterVisibleCommands(Workflow.GetCommands(ContextItem));
                    if (_commands == null)
                    {
                        _commands = new WorkflowCommand[0];
                    }
                }
                return _commands;
            }
        }

        public bool ShowWorkflowNoAccessMessage
        {
            get
            {
                if (!_showWorkflowNoAccessMessage.HasValue)
                {
                    _showWorkflowNoAccessMessage = Sitecore.Configuration.Settings.GetBoolSetting(_showWorkflowNoAccessMessageKey, false);
                }
                return _showWorkflowNoAccessMessage.Value;
            }
        }

        public bool ShowNotification
        {
            get
            {
                return NoNulls && !WorkflowState.FinalState && (Commands.Length > 0 && HasWriteAccess() || ShowWorkflowNoAccessMessage);
            }
        }

        public bool NoNulls
        {
            get
            {
                return (Database != null && WorkflowProvider != null && Workflow != null && WorkflowState != null);
            }
        }

        #endregion

        #region Constrcutor

        public ItemWorkflowModel(Item i)
        {
            if (i != null)
            {
                _contextItem = i;
                _database = i.Database;
                if (_database != null)
                {
                    _workflowProvider = _database.WorkflowProvider;
                    if (_workflowProvider != null)
                    {
                        _workflow = _workflowProvider.GetWorkflow(ContextItem);
                        if (_workflow != null)
                        {
                            _workflowState = _workflow.GetState(ContextItem);
                            if (_workflowState != null)
                            {
                                using (new SecurityDisabler())
                                {
                                    _workflowStateItem = _database.GetItem(_workflowState.StateID);
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool HasWriteAccess()
        {
            return AuthorizationManager.IsAllowed(ContextItem, AccessRight.ItemWrite, Sitecore.Context.User);
        }

        public string GetEditorDescription(bool includeContentEditorLink = true)
        {
            string noAccessMessage = string.Empty;
            string displayName = Workflow.Appearance.DisplayName;
            string itemDisplayName = string.Format("<span style=\"font-weight:bold;\">{0}</span>", ContextItem.DisplayName);
            if (includeContentEditorLink)
            {
                itemDisplayName = string.Format("'{0}'", ContextItem.DisplayName); // string.Format("<a href=\"{0}\" target=\"_blank\" style=\"font-weight:bold;\">{1}</a>", ContextItem.GetContentEditorUrl(), ContextItem.Name);
            }
            if (!HasWriteAccess())
            {
                noAccessMessage = " You cannot change the workflow because do not have write access to this item.";
            }
            return Translate.Text("The data source item {0} is in the '{1}' state of the '{2}' workflow. {3}", itemDisplayName, WorkflowState.DisplayName, displayName, noAccessMessage);
        }


        #endregion
    }
}

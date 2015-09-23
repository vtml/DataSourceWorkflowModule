using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Security.AccessControl;
using Sitecore.Workflows;

namespace HI.Shared.DataSourceWorkflowModule.Models
{
    public class ItemWorkflowModel
    { 
        #region Fields

        private readonly Item _contextItem;
        private readonly Database _database;
        private readonly IWorkflowProvider _workflowProvider;
        private readonly IWorkflow _workflow;
        private readonly WorkflowState _workflowState;
        WorkflowCommand[] _commands;
        private bool? _showWorkflowNoAccessMessage;
        private const string ShowWorkflowNoAccessMessageKey = "DatasourceWorkflowNotification.ShowWorkflowNoAccessMessage";

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
                    _showWorkflowNoAccessMessage = Sitecore.Configuration.Settings.GetBoolSetting(ShowWorkflowNoAccessMessageKey, false);
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

        #region Constructor

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
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        public bool HasWriteAccess()
        {
            return AuthorizationManager.IsAllowed(ContextItem, AccessRight.ItemWrite, Sitecore.Context.User);
        }

        public string GetEditorDescription(bool includeContentEditorLink = true)
        {
            var noAccessMessage = string.Empty;
            var displayName = Workflow.Appearance.DisplayName;
            var itemDisplayName = string.Format("<span style=\"font-weight:bold;\">{0}</span>", ContextItem.DisplayName);
            if (includeContentEditorLink)
            {
                itemDisplayName = string.Format("'{0}'", ContextItem.DisplayName); 
                // itemDisplayName = string.Format("<a href=\"{0}\" target=\"_blank\" style=\"font-weight:bold;\">{1}</a>", ContextItem.GetContentEditorUrl(), ContextItem.Name); // < Sitecore 7.5
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

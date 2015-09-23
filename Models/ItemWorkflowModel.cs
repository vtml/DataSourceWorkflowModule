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

        private bool UseHtmlMessaging
        {
            get
            {
                return Sitecore.Configuration.Settings.GetBoolSetting("DatasourceWorkflowNotification.UseHtmlInMessaging", false);
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

        public string GetEditorDescription(bool isPageEditor = true)
        {
            string noAccessMessage = string.Empty;
            string workflowDisplayName = Workflow.Appearance.DisplayName;
            string workflowStateDisplayName = WorkflowState.DisplayName;
            string itemDisplayName = ContextItem.Paths.ContentPath;

            if (UseHtmlMessaging && !isPageEditor)
            {
                workflowDisplayName = BoldValue(workflowDisplayName);
                workflowStateDisplayName = BoldValue(workflowStateDisplayName);
                itemDisplayName = BoldValue(itemDisplayName);
            }
            else
            {
                workflowDisplayName = SingleQuoteValue(workflowDisplayName);
                workflowStateDisplayName = SingleQuoteValue(workflowStateDisplayName);
                itemDisplayName = SingleQuoteValue(itemDisplayName);
            }

            if (!HasWriteAccess())
            {
                noAccessMessage = " You cannot change the workflow because do not have write access to this item.";
            }

            return Translate.Text("The data source item {0} is in the {1} state of the {2} workflow.{3}", itemDisplayName, workflowStateDisplayName, workflowDisplayName, noAccessMessage);
        }

        private static string SingleQuoteValue(string value)
        {
            return string.Format("'{0}'", value);
        }

        private static string BoldValue(string value)
        {
            return string.Format("<b>{0}</b>", value);
        }

        #endregion
    }
}

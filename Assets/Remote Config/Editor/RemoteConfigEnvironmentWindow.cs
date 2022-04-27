using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.RemoteConfig.Editor
{
    internal class RemoteConfigEnvironmentWindow : EditorWindow
    {
        public string environmentId;
        public string environmentName;
        public bool isDefault;

        public bool isDefaultFormSet = false;
        public bool environmentNameFormSet = false;
        public bool isDefaultForm;
        public string environmentNameForm;

        internal enum EnvironmentWindowModes
        {
            Create,
            Update
        }

        public EnvironmentWindowModes mode;
        public RemoteConfigEnvironmentWindowController.EnvironmentWindowState lastAction = RemoteConfigEnvironmentWindowController.EnvironmentWindowState.None;

        RemoteConfigEnvironmentWindowController m_EnvController;
        public event Action crudCompleted;

        //GUI Content
        GUIContent m_EnvironmentsLabelContent = new GUIContent("Environment Name:");
        GUIContent m_EnvironmentsIdContent = new GUIContent("Environment Id:");
        GUIContent m_EnvironmentsIsDefaultDisabledContent = new GUIContent("Default:", "Value is locked for default environment. \nIf you want to change this value, please make another environment default first.");
        GUIContent m_EnvironmentsIsDefaultEnabledContent = new GUIContent("Default:", "");
        GUIContent m_DeleteWarningContent = new GUIContent("Deleting an environment will delete all its rules and configs.");
        GUIContent m_CreatingMessage = new GUIContent("Creating an environment, please wait.");
        GUIContent m_DeletingMessage = new GUIContent("Deleting an environment, please wait.");
        GUIContent m_UpdatingMessage = new GUIContent("Updating an environment, please wait.");
        GUIContent m_CurrentError = new GUIContent("");

        // DialogBox variables
        public readonly string k_EnvDialogDeleteTitle = "Delete Environment";
        public readonly string k_EnvDialogDeleteMessage = "You are about to delete an environment. \n \n" +
                                                          "This action will delete all corresponding rules and settings.\n";
        public readonly string k_EnvDialogDeleteOK = "OK";
        public readonly string k_EnvDialogDeleteCancel = "Cancel";

        //UI Style variables
        const float k_LineHeight = 22f;

        private void OnEnable()
        {
            m_EnvController = new RemoteConfigEnvironmentWindowController();
            m_EnvController.environmentUpdated += OnEnvironmentUpdated;
            m_EnvController.environmentDeleted += OnEnvironmentDeleted;
            m_EnvController.environmentCreated += OnEnvironmentCreated;
            m_EnvController.environmentCRUDError += OnEnvironmentCRUDError;
            m_EnvController.fetchEnvironmentsFinished += OnFetchEnvironmentsFinished;
            m_EnvController.fetchConfigsFinished += OnFetchConfigsFinished;
            m_EnvController.fetchRulesFinished += OnFetchRulesFinished;
            crudCompleted += OnCrudCompleted;
        }

        private void OnEnvironmentUpdated()
        {
            RemoteConfigWebApiClient.FetchEnvironments(Application.cloudProjectId);
        }

        private void OnEnvironmentDeleted()
        {
            RemoteConfigWebApiClient.FetchEnvironments(Application.cloudProjectId);
        }

        private void OnEnvironmentCreated(string newEnvironmentId)
        {
            environmentId = newEnvironmentId;
            RemoteConfigWebApiClient.FetchEnvironments(Application.cloudProjectId);
        }

        private void OnEnvironmentCRUDError(JObject error)
        {
            m_EnvController.currentLoadingState = RemoteConfigEnvironmentWindowController.EnvironmentWindowState.None;
            m_CurrentError.text = " " + error["message"].Value<string>();
        }

        private void OnFetchEnvironmentsFinished(JArray environments)
        {
            m_EnvController.SetEnvironmentData(environments, environmentNameForm);
            if (lastAction == RemoteConfigEnvironmentWindowController.EnvironmentWindowState.Deleting || lastAction == RemoteConfigEnvironmentWindowController.EnvironmentWindowState.Creating)
            {
                RemoteConfigWebApiClient.FetchConfigs(Application.cloudProjectId, m_EnvController.GetCurrentEnvironmentId());
            }
            else
            {
                RemoteConfigWebApiClient.FetchConfigs(Application.cloudProjectId, environmentId);
            }
        }

        private void OnFetchConfigsFinished(JObject config)
        {
            m_EnvController.RefreshDataStore(config);
            if (config.HasValues && !string.IsNullOrEmpty(config["id"].Value<string>()))
            {
                RemoteConfigWebApiClient.FetchRules(Application.cloudProjectId, config["id"].Value<string>());
            }
            else
            {
                m_EnvController.RefreshRulesDataStore(new JArray());
                crudCompleted?.Invoke();
            }
        }

        private void OnFetchRulesFinished(JArray rules)
        {
            m_EnvController.RefreshRulesDataStore(rules);
            crudCompleted?.Invoke();
        }

        private void OnCrudCompleted()
        {
            m_EnvController.currentLoadingState = RemoteConfigEnvironmentWindowController.EnvironmentWindowState.None;
            var currentRCWindow = GetWindow<RemoteConfigWindow>();
            if (currentRCWindow)
            {
                currentRCWindow.SendEvent(EditorGUIUtility.CommandEvent("EnvWindowForcePull"));
            }
            Close();
        }

        private void OnGUI()
        {
            float currentY = 12f;
            DrawEnvironmentDetails(currentY);
            currentY += 3*k_LineHeight;
            if (m_EnvController.currentLoadingState != RemoteConfigEnvironmentWindowController.EnvironmentWindowState.None)
            {
                var currentStateMessage = new GUIContent();
                switch (m_EnvController.currentLoadingState)
                {
                    case RemoteConfigEnvironmentWindowController.EnvironmentWindowState.Creating:
                        currentStateMessage = m_CreatingMessage;
                        break;
                    case RemoteConfigEnvironmentWindowController.EnvironmentWindowState.Updating:
                        currentStateMessage = m_UpdatingMessage;
                        break;
                    case RemoteConfigEnvironmentWindowController.EnvironmentWindowState.Deleting:
                        currentStateMessage = m_DeletingMessage;
                        break;
                }
                GUI.Label(new Rect(0, position.height - k_LineHeight, position.width, k_LineHeight), currentStateMessage);
            }
            else
            {
                if (!String.IsNullOrEmpty(m_CurrentError.text))
                {
                    m_CurrentError.image = EditorGUIUtility.FindTexture("console.erroricon.sml");
                    GUI.Label(new Rect(0, position.height - k_LineHeight, position.width, k_LineHeight), m_CurrentError);
                }
                DrawEnvironmentButtons(currentY);
            }
        }

        private void DrawEnvironmentDetails(float currentY)
        {
            if (!environmentNameFormSet)
            {
                environmentNameForm = environmentName;
                environmentNameFormSet = true;
            }
            if (!isDefaultFormSet)
            {
                isDefaultForm = isDefault;
                isDefaultFormSet = true;
            }

            var totalWidth = position.width;
            GUI.Label(new Rect(0, currentY, 120, 20), m_EnvironmentsLabelContent);
            var envNameRect = new Rect(120, currentY, totalWidth - 120, 20);
            EditorGUIUtility.AddCursorRect(envNameRect, MouseCursor.Text);
            environmentNameForm = EditorGUI.TextField(envNameRect, environmentNameForm);
            currentY += k_LineHeight;

            if (mode == EnvironmentWindowModes.Update)
            {
                var envIdLabelLength = 120;
                var envIdValueLength = 260;
                GUI.Label(new Rect(0, currentY, envIdLabelLength, k_LineHeight), m_EnvironmentsIdContent);
                var envIdRect = new Rect(envIdLabelLength, currentY, envIdValueLength, k_LineHeight);
                EditorGUI.SelectableLabel(envIdRect, environmentId);

                var copyIcon = EditorGUIUtility.FindTexture("Clipboard");
                var copyIconRect = new Rect(envIdLabelLength + envIdValueLength - 10, currentY, 24, k_LineHeight);
                if (GUI.Button(copyIconRect, new GUIContent(copyIcon, "Copy Environment Id")))
                {
                    EditorGUIUtility.systemCopyBuffer = environmentId;
                }

                EditorGUI.BeginDisabledGroup(m_EnvController.IsCurrentEnvironmentDefault());
                var isDefaultRect = new Rect(0, currentY + k_LineHeight, 100, k_LineHeight);
                var isDefaultLabel = m_EnvController.IsCurrentEnvironmentDefault()
                    ? m_EnvironmentsIsDefaultDisabledContent
                    : m_EnvironmentsIsDefaultEnabledContent;
                GUI.Label(isDefaultRect, isDefaultLabel);
                var isDefaultToggleRect = new Rect(envIdLabelLength, currentY + k_LineHeight, 22, k_LineHeight);
                isDefaultForm = GUI.Toggle(isDefaultToggleRect, isDefaultForm, "");
                EditorGUI.EndDisabledGroup();
            }

        }


        private void DrawEnvironmentButtons(float currentY)
        {
            var boundingBoxPadding = 5f;
            var paddedRect = new Rect(position.width - (position.width / 3), currentY, ((position.width / 3)) - 10, 20);
            var buttonWidth = (paddedRect.width / 3);

            if (mode == EnvironmentWindowModes.Update)
            {
                EditorGUI.BeginDisabledGroup(m_EnvController.IsCurrentEnvironmentDefault() || m_EnvController.currentLoadingState != RemoteConfigEnvironmentWindowController.EnvironmentWindowState.None);
                if (GUI.Button(new Rect(paddedRect.x, paddedRect.y, buttonWidth, 20), "Delete"))
                {
                    if (EditorUtility.DisplayDialog(k_EnvDialogDeleteTitle, k_EnvDialogDeleteMessage, k_EnvDialogDeleteOK, k_EnvDialogDeleteCancel))
                    {
                        lastAction = RemoteConfigEnvironmentWindowController.EnvironmentWindowState.Deleting;
                        m_EnvController.DeleteEnvironment(environmentId);
                    }
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(environmentNameForm == environmentName && isDefaultForm == isDefault || m_EnvController.currentLoadingState != RemoteConfigEnvironmentWindowController.EnvironmentWindowState.None);
                if (GUI.Button(new Rect(paddedRect.x + buttonWidth + boundingBoxPadding, paddedRect.y, buttonWidth, 20), "Update"))
                {
                    lastAction = RemoteConfigEnvironmentWindowController.EnvironmentWindowState.Updating;
                    if(environmentNameForm != environmentName)
                    {
                        m_EnvController.UpdateEnvironment(environmentId, environmentNameForm);
                    }
                    if(isDefaultForm != isDefault && isDefaultForm == true)
                    {
                        m_EnvController.SetDefaultEnvironment(environmentId);
                    }
                }
                EditorGUI.EndDisabledGroup();

                if (!m_EnvController.IsCurrentEnvironmentDefault())
                {
                    GUI.Label(new Rect(0, currentY+k_LineHeight, 480, k_LineHeight), m_DeleteWarningContent);
                }
            }

            if (mode == EnvironmentWindowModes.Create)
            {
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(environmentNameForm) || m_EnvController.currentLoadingState != RemoteConfigEnvironmentWindowController.EnvironmentWindowState.None);
                if (GUI.Button(new Rect(paddedRect.x + buttonWidth + boundingBoxPadding, paddedRect.y, buttonWidth, 20), "Create"))
                {
                    lastAction = RemoteConfigEnvironmentWindowController.EnvironmentWindowState.Creating;
                    m_EnvController.CreateEnvironment(environmentNameForm);
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUI.BeginDisabledGroup(m_EnvController.currentLoadingState != RemoteConfigEnvironmentWindowController.EnvironmentWindowState.None);
            if(GUI.Button(new Rect(paddedRect.x + paddedRect.width - buttonWidth + (boundingBoxPadding * 2), paddedRect.y, buttonWidth, 20), "Cancel"))
            {
                Close();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void OnDisable()
        {
            m_EnvController.environmentCreated -= OnEnvironmentCreated;
            m_EnvController.environmentCRUDError -= OnEnvironmentCRUDError;
            m_EnvController.environmentUpdated -= OnEnvironmentUpdated;
            m_EnvController.environmentDeleted -= OnEnvironmentDeleted;
            m_EnvController.fetchEnvironmentsFinished -= OnFetchEnvironmentsFinished;
            m_EnvController.fetchConfigsFinished -= OnFetchConfigsFinished;
            m_EnvController.fetchRulesFinished -= OnFetchRulesFinished;
            crudCompleted -= OnCrudCompleted;
            m_EnvController.currentLoadingState = RemoteConfigEnvironmentWindowController.EnvironmentWindowState.None;
        }

    }
}
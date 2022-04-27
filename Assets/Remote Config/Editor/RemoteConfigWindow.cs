using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.RemoteConfig.Core;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Unity.RemoteConfig.Editor.UIComponents;

namespace Unity.RemoteConfig.Editor
{
    internal class RemoteConfigWindow : EditorWindow
    {
        //Window state
        public bool shouldFetchOnInit;
        public bool windowOpenOnInit;
        [NonSerialized] bool m_Initialized;
        RulesTreeView m_RulesTreeView;
        [SerializeField] TreeViewState m_RulesTreeViewState;
        [SerializeField] MultiColumnHeaderState m_RulesMultiColumnHeaderState;
        [SerializeField] TreeViewState m_SettingsTreeViewState;
        [SerializeField] MultiColumnHeaderState m_SettingsMultiColumnHeaderState;
        ConfigsTreeView m_ConfigsTreeView;
        [SerializeField] TreeViewState m_ConfigsTreeViewState;

        RulesMultiColumnHeader m_RulesMultiColumnHeader;

        string m_SelectedRuleId = null;
        
        RemoteConfigWindowController m_Controller;
        private RemoteConfigEnvironmentWindow m_RCEnvWindow;
        
        //GUI Content
        GUIContent m_pullRulesButtonContent = new GUIContent("Pull");
        GUIContent m_pushRulesButtonContent = new GUIContent("Push");
        GUIContent m_createEnvironmentButtonContent = new GUIContent("Create");
        GUIContent m_editEnvironmentButtonContent = new GUIContent("Edit");
        GUIContent m_createRuleButtonContent = new GUIContent("Add Rule");
        GUIContent m_loadingMessage = new GUIContent("Loading, please wait.");
        GUIContent m_EnvironmentsLabelContent = new GUIContent("Environment Name:");
        GUIContent m_EnvironmentsIdContent = new GUIContent("Environment Id:");
        GUIContent m_EnvironmentsIsDefaultContent = new GUIContent("Default:");
        GUIContent m_AnalyticsNotEnabledContent = new GUIContent("To get started with Unity Remote Config, you must first link your project to a Unity Cloud Project ID. A Unity Cloud Project ID is an online identifier which is used across all Unity Services. These can be created within the Services window itself, or online on the Unity Services website. The simplest way is to use the Services window within Unity, as follows: \nTo open the Services Window, go to Window > General > Services.\nNote: using Unity Remote Config does not require that you turn on any additional, individual cloud services like Analytics, Ads, Cloud Build, etc.");

        //UI Style variables
        const float k_LineHeight = 22f;
        const float k_LineHeightBuffer = k_LineHeight - 2;
        const float k_LinePadding = 5f;

        private const string utcDateFormat = "YYYY-MM-DDThh:mm:ssZ";
        private const string ruleConditionFormat = "JEXL Syntax";
        const string m_NoSettingsContent = "To get started, please add a setting";
        const string m_NoSettingsForTheRuleContent = "Please add at least one setting to your rule";
        private GUIStyle guiStyleLabel = new GUIStyle();
        private GUIStyle guiStyleSubLabel = new GUIStyle();

        private bool isConfigSelected = true;
        private MVRuleSettingsView mvRuleSettingsView;
        SettingsTreeview settingsTreeview;
        SegmentationRulesTreeview segmentationRulesTreeview;

        JsonSerializerSettings rawDateSettings = new JsonSerializerSettings {DateParseHandling = DateParseHandling.None};

        Rect toolbarRect
        {
            get
            {
                return new Rect(0, 0, position.width, (k_LineHeight * 2));
            }
        }

        Rect configsRulesViewRect
        {
            get
            {
                //TODO: Figure out why k_LineHeight * 2.25f is the magic number...
                return new Rect(0, toolbarRect.height + k_LinePadding, position.width * .3f, (position.height - (k_LineHeight * 2.25f)));
            }
        }

        Rect configsViewRect
        {
            get
            {
                return new Rect(0, configsRulesViewRect.y, configsRulesViewRect.width, k_LineHeight * 2);
            }
        }

        Rect configHeaderRect
        {
            get
            {
                return new Rect(0, configsRulesViewRect.y, configsRulesViewRect.width, k_LineHeight);
            }
        }

        Rect configTableRect
        {
            get
            {
                return new Rect(0, configHeaderRect.y + configHeaderRect.height, configsRulesViewRect.width, k_LineHeight);
            }
        }

        Rect ruleViewRect
        {
            get
            {
                return new Rect(0, configsViewRect.y + configsViewRect.height, position.width * .3f, configsRulesViewRect.height - configsViewRect.height);
            }
        }

        Rect ruleHeaderRect
        {
            get
            {
                return new Rect(0, ruleViewRect.y, ruleViewRect.width, k_LineHeight);
            }
        }

        Rect ruleTableRect
        {
            get
            {
                return new Rect(0, ruleViewRect.y + ruleHeaderRect.height, ruleViewRect.width, ruleViewRect.height - k_LineHeight - ruleHeaderRect.height);
            }
        }

        Rect ruleTableFooterRect
        {
            get
            {
                return new Rect(0, ruleTableRect.y + ruleTableRect.height-3f, ruleViewRect.width, k_LineHeight);
            }
        }

        Rect detailsViewRect
        {
            get
            {
                //TODO: Add divider width to this programattically
                return new Rect(configsViewRect.width + 1f, toolbarRect.height + k_LinePadding, position.width - configsRulesViewRect.width, position.height - (k_LineHeight * 2.35f));
            }
        }

        [MenuItem("Window/Remote Config")]
        public static void GetWindow()
        {
            var RSWindow = GetWindow<RemoteConfigWindow>();
            RSWindow.titleContent = new GUIContent("Remote Config");
            RSWindow.minSize = new Vector2(600, 300);
            RSWindow.windowOpenOnInit = true;
            RSWindow.Focus();
            RSWindow.Repaint();
        }
        
        private void OnEnable()
        {
            if (AreServicesEnabled())
            {
                InitIfNeeded();
            }
        }

        private void OnDisable()
        {
            if (m_Controller != null)
            {
                m_Controller.SetDataStoreDirty();

                try
                {
                    m_RulesTreeView.DeleteRule -= OnDeleteRule;
                    m_RulesTreeView.RuleEnabledOrDisabled -= OnRuleEnabledOrDisabled;
                    m_RulesTreeView.RuleAttributesChanged -= OnRuleAttributesChanged;
                    if(settingsTreeview != null)
                    {
                        settingsTreeview.OnSettingChanged -= SettingsTreeview_OnConfigSettingsChanged;
                    }
                    if(segmentationRulesTreeview != null)
                    {
                        segmentationRulesTreeview.OnSettingChanged -= SettingsTreeview_OnConfigSettingsChanged;
                    }

                    m_Controller.rulesDataStoreChanged -= OnRulesDataStoreChanged;
                    m_Controller.remoteSettingsStoreChanged -= OnRemoteSettingsStoreChanged;
                    m_Controller.environmentChanged -= OnEnvironmentChanged;
                    EditorApplication.quitting -= m_Controller.SetDataStoreDirty;
                    EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (NullReferenceException e)
#pragma warning restore CS0168 // Variable is declared but never used
                { }
            }
        }

        private void OnGUI()
        {
            if (AreServicesEnabled(true))
            {
                InitIfNeeded();

                Event e = Event.current;
                if (e.commandName == "EnvWindowForcePull")
                {
                    e.commandName = null;
                    m_Controller.Fetch();
                    m_RulesTreeView.SetSelection(new List<int>());
                }

                EditorGUI.BeginDisabledGroup(IsLoading() || EnvironmentWindowIsOpen());

                DrawToolbar(toolbarRect);

                GUI.Label(configHeaderRect, "Configs", EditorStyles.boldLabel);
                m_ConfigsTreeView.OnGUI(configTableRect);
                if (m_ConfigsTreeView.HasFocus() || string.IsNullOrEmpty(m_SelectedRuleId))
                {
                    isConfigSelected = true;
                    m_RulesTreeView.SetSelection(new List<int>());
                    m_SelectedRuleId = null;
                }
                else
                {
                    if (m_RulesTreeView.HasFocus())
                    {
                        isConfigSelected = false;
                    }
                }

                GUI.Label(ruleHeaderRect, "Rules", EditorStyles.boldLabel);
                m_RulesTreeView.OnGUI(ruleTableRect);
                DrawPaneSeparator(configsRulesViewRect);

                if (!IsLoading())
                {
                    if (GUI.Button(new Rect(k_LinePadding/2, ruleTableFooterRect.y, ruleTableRect.width - k_LinePadding, k_LineHeight),
                    m_createRuleButtonContent))
                    {
                        m_Controller.AddDefaultRule();
                        m_SelectedRuleId = (m_Controller.GetRulesList().Last()["id"]).Value<string>();
                        m_RulesTreeView.SetSelection(m_SelectedRuleId);

                        //TODO: move this logic elsewhere
                        segmentationRulesTreeview.settingsList = m_Controller.GetSettingsList();
                        segmentationRulesTreeview.activeSettingsList = (JArray)m_Controller.GetRuleById(m_SelectedRuleId)["value"];
                    }
                }

                if (isConfigSelected)
                {
                    DrawConfigsSettingsPane(detailsViewRect);
                }
                else
                {
                    if (m_Controller.HasRules() && !string.IsNullOrEmpty(m_SelectedRuleId))
                    {
                        DrawRulesSettingsView(detailsViewRect);
                    }
                    else
                    {
                        //TODO: Show a warning about no rules
                    }
                }

                EditorGUI.EndDisabledGroup();
                AddLoadingMessage();
            }
        }

        private void DrawRulesSettingsView(Rect viewRect)
        {
            var currentRule = m_Controller.GetRuleById(m_SelectedRuleId);
            var detailsRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 7.6f * k_LineHeight);
            var treeViewRect = new Rect(viewRect.x, viewRect.y + detailsRect.height, viewRect.width, viewRect.height - detailsRect.height);

            DrawRulesDetailsPane(currentRule, detailsRect);
            if(currentRule["type"].Value<string>() == "segmentation")
            {
                DrawRuleSettingsRect(treeViewRect);
            }
            else
            {
                var headerRect = new Rect(treeViewRect.x, treeViewRect.y, treeViewRect.width, k_LineHeight);
                var treeViewBodyRect = new Rect(treeViewRect.x, treeViewRect.y + headerRect.height, treeViewRect.width, treeViewRect.height - headerRect.height);
                GUI.Label(headerRect, "Settings", EditorStyles.boldLabel);
                mvRuleSettingsView.UpdateView(m_Controller.GetSettingsList(), m_Controller.GetRuleById(m_SelectedRuleId));
                mvRuleSettingsView.OnGUI(treeViewBodyRect);
            }
        }

        private void DrawToolbar (Rect toolbarSize)
        {
            var currentY = toolbarSize.y;
            DrawEnvironmentDropdown(currentY);
            DrawPushPullButtons(currentY);
            currentY += k_LineHeight;

            DrawEnvironmentDetails(currentY);

            if (GUI.Button(new Rect(position.width - (position.width / 5), currentY + 2, (position.width / 5) - 5, 20), "View in Dashboard"))
            {
                if (string.IsNullOrEmpty(m_Controller.environmentId))
                {
                    Help.BrowseURL(string.Format(RemoteConfigEnvConf.dashboardEnvironmentsPath, Application.cloudProjectId));
                }
                else
                {
                    Help.BrowseURL(string.Format(RemoteConfigEnvConf.dashboardConfigsPath, Application.cloudProjectId, m_Controller.environmentId));
                }
            }
        }

        private void InitIfNeeded()
        {
            if (!m_Initialized)
            {
                mvRuleSettingsView = new MVRuleSettingsView();
                mvRuleSettingsView.MvRuleUpdated += OnMvRuleUpdate;
                settingsTreeview = new SettingsTreeview();
                settingsTreeview.OnSettingChanged += SettingsTreeview_OnConfigSettingsChanged;
                segmentationRulesTreeview = new SegmentationRulesTreeview();
                segmentationRulesTreeview.OnSettingChanged += SettingsTreeview_OnConfigSettingsChanged;
                m_Controller = new RemoteConfigWindowController();
                EditorApplication.quitting += m_Controller.SetDataStoreDirty;
                EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;

                if (m_RulesTreeViewState == null)
                {
                    m_RulesTreeViewState = new TreeViewState();
                }

                bool firstInit = m_RulesMultiColumnHeaderState == null;
                var headerState = CreateRulesMultiColumnHeaderState(ruleHeaderRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_RulesMultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_RulesMultiColumnHeaderState, headerState);
                m_RulesMultiColumnHeaderState = headerState;
                
                foreach(MultiColumnHeaderState.Column column in m_RulesMultiColumnHeaderState.columns)
                {
                    column.autoResize = true;
                }
                
                m_RulesMultiColumnHeader = new RulesMultiColumnHeader(headerState);
                if (firstInit)
                {
                    m_RulesMultiColumnHeader.ResizeToFit();
                }
                m_RulesTreeView = new RulesTreeView(m_RulesTreeViewState, m_RulesMultiColumnHeader, m_Controller.GetRulesList());

                if (m_SettingsTreeViewState == null)
                {
                    m_SettingsTreeViewState = new TreeViewState();
                }
                
                firstInit = m_SettingsMultiColumnHeaderState == null;
                
                m_SettingsMultiColumnHeaderState = headerState;

                if (m_ConfigsTreeViewState == null)
                {
                    m_ConfigsTreeViewState = new TreeViewState();
                }

                m_ConfigsTreeView = new ConfigsTreeView(m_ConfigsTreeViewState);

                m_RulesTreeView.SelectionChangedEvent += selectedRuleId =>
                {
                    this.m_SelectedRuleId = selectedRuleId;

                    if (string.IsNullOrEmpty(m_SelectedRuleId))
                    {
                        settingsTreeview.settingsList = m_Controller.GetSettingsList();
                        settingsTreeview.activeSettingsList = m_Controller.GetSettingsList();
                    }
                    else
                    {
                        isConfigSelected = false;
                        //settingsTreeview.settingsList = m_Controller.GetSettingsList();
                        segmentationRulesTreeview.settingsList = m_Controller.GetSettingsList();
                        if(m_Controller.GetRuleById(m_SelectedRuleId)["type"].Value<string>() == "segmentation")
                        {
                            //settingsTreeview.activeSettingsList = (JArray)m_Controller.GetRuleById(m_SelectedRuleId)["value"].DeepClone();
                            segmentationRulesTreeview.activeSettingsList = (JArray)m_Controller.GetRuleById(m_SelectedRuleId)["value"].DeepClone();
                        }
                    }
                };
                m_RulesTreeView.DeleteRule += OnDeleteRule;
                m_RulesTreeView.RuleEnabledOrDisabled += OnRuleEnabledOrDisabled;
                m_RulesTreeView.RuleAttributesChanged += OnRuleAttributesChanged;

                m_Controller.rulesDataStoreChanged += OnRulesDataStoreChanged;

                m_Controller.remoteSettingsStoreChanged += OnRemoteSettingsStoreChanged;
                m_Controller.environmentChanged += OnEnvironmentChanged;

                m_Initialized = true;
            }
        }

        private void SettingsTreeview_OnConfigSettingsChanged(JObject arg1, JObject arg2)
        {
            if(arg1 == null && arg2 != null)
            {
                //new setting added
                if (isConfigSelected)
                {
                    //new setting added to config
                    m_Controller.AddSetting();
                }
                else
                {
                    //new setting added to rule
                    OnAddSettingToRule(arg2["metadata"]["entityId"].Value<string>(), true);
                }
            }
            else if(arg2 == null && arg1 != null)
            {
                //setting removed/deleted
                if (isConfigSelected)
                {
                    //setting deleted
                    OnDeleteSetting(arg1["metadata"]["entityId"].Value<string>());
                }
                else
                {
                    //remove setting from rule
                    OnAddSettingToRule(arg1["metadata"]["entityId"].Value<string>(), false);
                }
            }
            else if(arg1 != null && arg2 != null)
            {
                //update the setting
                OnUpdateSetting(arg1, arg2);
            }
        }

        private void OnMvRuleUpdate(JObject oldRule, JObject newRule)
        {
            m_Controller.OnRuleUpdated(oldRule, newRule);
        }

        private void OnDestroy()
        {
            if (m_Controller != null)
            {
                if (!(m_Controller.CompareJArraysEquality(m_Controller.GetSettingsList(),
                          m_Controller.GetLastCachedKeyList()) &&
                      m_Controller.CompareJArraysEquality(m_Controller.GetRulesList(),
                          m_Controller.GetLastCachedRulesList())))
                {
                    if (!EditorUtility.DisplayDialog(m_Controller.k_RCDialogUnsavedChangesTitle,
                        m_Controller.k_RCDialogUnsavedChangesMessage,
                        m_Controller.k_RCDialogUnsavedChangesOK,
                        m_Controller.k_RCDialogUnsavedChangesCancel))
                    {
                        CreateNewRCWindow();
                    }
                }
            }
        }

        private void CreateNewRCWindow()
        {
            RemoteConfigWindow newWindow = (RemoteConfigWindow) CreateInstance(typeof(RemoteConfigWindow));
            newWindow.titleContent.text = m_Controller.k_RCWindowName;
            newWindow.shouldFetchOnInit = true;
            newWindow.Show();
        }

        private void CreateNewRCEnvironmentWindow(string environmentId, string environmentName, bool isDefault)
        {
            m_RCEnvWindow = (RemoteConfigEnvironmentWindow) CreateInstance(typeof(RemoteConfigEnvironmentWindow));
            m_RCEnvWindow.titleContent.text = "Manage Environments";
            m_RCEnvWindow.minSize = new Vector2(480, 140);
            m_RCEnvWindow.maxSize = new Vector2(480, 140);
            m_RCEnvWindow.environmentId = environmentId;
            m_RCEnvWindow.environmentName = environmentName;
            m_RCEnvWindow.isDefault = isDefault;
            m_RCEnvWindow.mode = String.IsNullOrEmpty(environmentId)? RemoteConfigEnvironmentWindow.EnvironmentWindowModes.Create : RemoteConfigEnvironmentWindow.EnvironmentWindowModes.Update;
            m_RCEnvWindow.ShowUtility();
        }

        private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            if(obj == PlayModeStateChange.EnteredPlayMode)
            {
                m_Controller.SetDataStoreDirty();
            }
        }

        private bool AreServicesEnabled(bool calledFromOnGui = false)
        {
            if (string.IsNullOrEmpty(CloudProjectSettings.projectId) || string.IsNullOrEmpty(CloudProjectSettings.organizationId))
            {
                if(calledFromOnGui)
                {
                    GUIStyle style = GUI.skin.label;
                    style.wordWrap = true;
                    EditorGUILayout.LabelField(m_AnalyticsNotEnabledContent, style);
                }
                return false;
            }
            return true;
        }

        private void OnEnvironmentChanged()
        {
            m_SelectedRuleId = null;
            isConfigSelected = true;
            m_RulesTreeView.SetSelection(new List<int>());
            m_RulesTreeView.rulesList = m_Controller.GetRulesList();
            m_RulesTreeView.Reload();
            m_ConfigsTreeView.SetFocus();
        }

        private void OnDeleteSetting(string entityId)
        {
            if (string.IsNullOrEmpty(m_SelectedRuleId) || isConfigSelected)
            {
                m_Controller.DeleteRemoteSetting(entityId);
            }
            else
            {
                m_Controller.DeleteSettingFromRule(m_SelectedRuleId, entityId);
            }
        }

        private void OnUpdateSetting(JObject oldItem, JObject newitem)
        {
            if (string.IsNullOrEmpty(m_SelectedRuleId) || isConfigSelected)
            {
                m_Controller.UpdateRemoteSetting(oldItem, newitem);
            }
            else
            {
                m_Controller.UpdateSettingForRule(m_SelectedRuleId, newitem);
            }
        }

        private void AddLoadingMessage()
        {
            if (IsLoading())
            {
                GUI.Label(new Rect(0, position.height - k_LineHeight, position.width, k_LineHeight), m_loadingMessage);
            }
        }

        private bool IsLoading()
        {
            bool isLoading = m_Controller.isLoading;
            settingsTreeview.isLoading = isLoading;
            segmentationRulesTreeview.isLoading = isLoading;
            return isLoading;
        }

        private bool EnvironmentWindowIsOpen()
        {
            var envWindows = Resources.FindObjectsOfTypeAll<RemoteConfigEnvironmentWindow>();
            return envWindows != null && envWindows.Length > 0;
        }

        private void OnDeleteRule(string ruleId)
        {
            m_SelectedRuleId = null;
            isConfigSelected = true;
            m_ConfigsTreeView.SetFocus();
            m_Controller.DeleteRule(ruleId);
            m_RulesTreeView.SetSelection(new List<int> ());
        }

        private void OnRuleAttributesChanged(string ruleId, JObject newRule)
        {
             m_Controller.UpdateRuleAttributes(ruleId, newRule);
        }

        private void OnRuleEnabledOrDisabled(string ruleId, bool enabled)
        {
            m_Controller.EnableOrDisableRule(ruleId, enabled);
        }

        private void OnRulesDataStoreChanged()
        {
            m_RulesTreeView.rulesList = m_Controller.GetRulesList();
            m_RulesTreeView.Reload();
            OnRemoteSettingsStoreChanged();
        }
        
        private void OnRemoteSettingsStoreChanged()
        {
            if (string.IsNullOrEmpty(m_SelectedRuleId) || isConfigSelected)
            {
                settingsTreeview.settingsList = m_Controller.GetSettingsList();
                settingsTreeview.activeSettingsList = m_Controller.GetSettingsList();
            }
            else
            {
                segmentationRulesTreeview.settingsList = m_Controller.GetSettingsList();
                var rule = m_Controller.GetRuleById(m_SelectedRuleId);
                if(rule["type"].Value<string>() == "segmentation")
                {
                    segmentationRulesTreeview.activeSettingsList = (JArray)rule["value"].DeepClone();
                }
            }
        }

        private void DrawEnvironmentDropdown(float currentY)
        {
            var totalWidth = position.width / 2;
            EditorGUI.BeginDisabledGroup(m_Controller.GetEnvironmentsCount() <= 1 || IsLoading());
            GUI.Label(new Rect(0, currentY, 120, 20), m_EnvironmentsLabelContent);
            var environmentName = string.IsNullOrEmpty(m_Controller.environmentName) ? "" : m_Controller.environmentName;
            GUIContent ddBtnContent = new GUIContent(Regex.Replace(environmentName, @"[/]+", "\u2215"));
            Rect ddRect = new Rect(120 , currentY, totalWidth - 120, 20);
            if (GUI.Button(ddRect, ddBtnContent, EditorStyles.popup))
            {
                m_Controller.BuildPopupListForEnvironments().DropDown(ddRect);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawEnvironmentDetails(float currentY)
        {
            var envIdLabelLength = 120;
            var envIdValueLength = 260;
            var envIsDefaultLength = 58;
            var isDefaultIcon = m_Controller.environmentIsDefault ? EditorGUIUtility.FindTexture("CollabNew") : EditorGUIUtility.FindTexture("Grid.BoxTool");
            GUI.Label(new Rect(0, currentY, envIdLabelLength, k_LineHeight), m_EnvironmentsIdContent);
            var envIdRect = new Rect(envIdLabelLength, currentY, envIdValueLength, k_LineHeight);
            EditorGUI.SelectableLabel(envIdRect, m_Controller.environmentId);
            GUI.Label(new Rect(envIdLabelLength + envIdValueLength, currentY, envIsDefaultLength, 20), m_EnvironmentsIsDefaultContent);
            GUI.DrawTexture(new Rect(envIdLabelLength + envIdValueLength + envIsDefaultLength, currentY + 2, 12, 12), isDefaultIcon);
        }

        private void DrawNoEnvironmentsWarning(float currentY)
        {
            showMessage(new Rect(0, currentY, 380f, k_LineHeight), "Please click Create above to create your first environment.");
        }

        private void DrawPushPullButtons(float currentY)
        {
            float boundingBoxPadding = 8;
            var paddedRect = new Rect((position.width / 2) + boundingBoxPadding, currentY,(position.width / 2) - (2 * boundingBoxPadding), 20);
            var buttonWidth = (paddedRect.width / 4);

            EditorGUI.BeginDisabledGroup(m_Controller.GetEnvironmentsCount() == 0);

            if (GUI.Button(new Rect(paddedRect.x, paddedRect.y, (buttonWidth - boundingBoxPadding), 20), m_editEnvironmentButtonContent))
            {
                CreateNewRCEnvironmentWindow(m_Controller.environmentId, m_Controller.environmentName, m_Controller.environmentIsDefault);
            }
            EditorGUI.EndDisabledGroup();

            if (GUI.Button(new Rect(paddedRect.x + (buttonWidth - boundingBoxPadding), paddedRect.y, (buttonWidth - boundingBoxPadding), 20), m_createEnvironmentButtonContent))
            {
                CreateNewRCEnvironmentWindow("", "", false);
            }

            if (GUI.Button(new Rect(paddedRect.x + 2*(buttonWidth + (2 * boundingBoxPadding)), paddedRect.y, buttonWidth - (2 * boundingBoxPadding), 20),
                m_pushRulesButtonContent))
            {
                m_Controller.Push();
                m_SelectedRuleId = null;
                isConfigSelected = true;
                m_RulesTreeView.SetSelection(new List<int>());
                m_ConfigsTreeView.SetFocus();
            }

            if (GUI.Button(new Rect(paddedRect.x + 3*(buttonWidth) + (2.2f*boundingBoxPadding), paddedRect.y, buttonWidth - (2 * boundingBoxPadding), 20),
                    m_pullRulesButtonContent))
            {
                m_Controller.Fetch();
                m_SelectedRuleId = null;
                isConfigSelected = true;
                m_RulesTreeView.SetSelection(new List<int>());
                m_ConfigsTreeView.SetFocus();
            }

        }

        private void DrawConfigsSettingsPane(Rect configsDetailsRect)
        {
            DrawConfigsSettingsTreeView(new Rect(configsDetailsRect.x, configsDetailsRect.y, configsDetailsRect.width, configsDetailsRect.height));
        }

        void DrawConfigsSettingsTreeView(Rect treeViewRect)
        {
            settingsTreeview.enableEditingSettingsKeys = true;
            settingsTreeview.rulesList = m_Controller.GetRulesList();
            settingsTreeview.settingsList = m_Controller.GetSettingsList();
            settingsTreeview.activeSettingsList = m_Controller.GetSettingsList();

            //TODO: Figure out what to do here
            if (!m_Controller.GetSettingsList().Any())
            {
                settingsTreeview.settingsList = null;
                var messageRect = new Rect(treeViewRect.x + 1f, treeViewRect.y + k_LineHeight + 6f, treeViewRect.width - 3f, k_LineHeight);
                showMessage(messageRect, m_NoSettingsContent);
            }
            settingsTreeview.OnGUI(treeViewRect);
        }

        private void DrawPaneSeparator(Rect rulesTreeViewRect)
        {
            EditorGUI.DrawRect(new Rect(rulesTreeViewRect.width - 1, rulesTreeViewRect.y, 1, rulesTreeViewRect.height), Color.black);
        }

        private void DrawRulesDetailsPane(JObject rule, Rect ruleDetailsRect)
        {
            var rawStartDate = string.IsNullOrEmpty(rule["startDate"].Value<string>()) ? "" : JsonConvert.SerializeObject(rule["startDate"], rawDateSettings).Replace("\"", "");
            var rawEndDate = string.IsNullOrEmpty(rule["endDate"].Value<string>()) ? "" : JsonConvert.SerializeObject(rule["endDate"], rawDateSettings).Replace("\"", "");

            var currentY = ruleDetailsRect.y;
            var nameRect = new Rect(ruleDetailsRect.x, ruleDetailsRect.y, ruleDetailsRect.width * .65f, ruleDetailsRect.height);
            var ruleName = CreateLabelAndTextField("Name: ", rule["name"].Value<string>(), currentY, nameRect);

            CreateRuleTypeDropdown("Type", rule, currentY, new Rect(ruleDetailsRect.x + nameRect.width, ruleDetailsRect.y, ruleDetailsRect.width - nameRect.width, ruleDetailsRect.height), 40f);
            currentY += k_LineHeight;

            var condition = CreateLabelWithSubLabelTextFieldAndHelpButton("Condition: ", ruleConditionFormat, rule["condition"].Value<string>(), currentY, ruleDetailsRect, RemoteConfigEnvConf.apiDocsBasePath + RemoteConfigEnvConf.apiDocsRulesPath);
            currentY += 1.4f*k_LineHeight;

            var rolloutPercentage = CreateLabelAndSlider("Rollout Percentage: ", rule["rolloutPercentage"].Value<int>(), 1, 100, currentY, ruleDetailsRect);
            currentY += k_LineHeight;

            var startDate = CreateLabelWithSubLabelAndTextField("Start Date & Time: ", utcDateFormat, rawStartDate, currentY, ruleDetailsRect);
            currentY += 1.4f*k_LineHeight;

            var endDate = CreateLabelWithSubLabelAndTextField("End Date & Time: ", utcDateFormat, rawEndDate, currentY, ruleDetailsRect);
            currentY += 1.4f * k_LineHeight;

            if (ruleName != rule["name"].Value<string>() || condition != rule["condition"].Value<string>() || rolloutPercentage != rule["rolloutPercentage"].Value<int>() || startDate != rule["startDate"].Value<string>() || endDate != rule["endDate"].Value<string>())
            {
                var newRule = new JObject();
                newRule["id"] = rule["id"];
                newRule["name"] = ruleName;
                newRule["type"] = rule["type"];
                newRule["condition"] = condition;
                newRule["rolloutPercentage"] = rolloutPercentage;
                newRule["startDate"] = startDate;
                newRule["endDate"] = endDate;
                newRule["priority"] = rule["priority"];
                newRule["enabled"] = rule["enabled"];
                newRule["value"] = rule["value"];
                m_Controller.UpdateRuleAttributes(rule["id"].Value<string>(), newRule);
            }

            if (m_Controller.GetSettingsListForRule(rule["id"].Value<string>()).Count == 0)
            {
                var messageRect = new Rect(ruleDetailsRect.x + 6f, currentY, ruleDetailsRect.width - 12f, k_LineHeight);
                showMessage(messageRect, m_NoSettingsForTheRuleContent);
            }
        }

        private void DrawRuleSettingsRect(Rect treeViewRect)
        {
            var headerRect = new Rect(treeViewRect.x, treeViewRect.y, treeViewRect.width, k_LineHeight);
            var treeViewBodyRect = new Rect(treeViewRect.x, treeViewRect.y + headerRect.height, treeViewRect.width, treeViewRect.height - headerRect.height);
            GUI.Label(headerRect, "Settings", EditorStyles.boldLabel);

            segmentationRulesTreeview.enableEditingSettingsKeys = false;
            segmentationRulesTreeview.rulesList = m_Controller.GetRulesList();
            segmentationRulesTreeview.settingsList = m_Controller.GetSettingsList();
            segmentationRulesTreeview.OnGUI(treeViewBodyRect);
        }

        private void OnAddSettingToRule(string entityId, bool active)
        {
            if (active)
            {
                m_Controller.AddSettingToRule(m_SelectedRuleId, entityId);
            }
            else
            {
                m_Controller.DeleteSettingFromRule(m_SelectedRuleId, entityId);
            }
        }

        private void CreateRuleTypeDropdown(string labelText, JObject rule, float currentY, Rect configPaneRect, float labelWidth = 125f)
        {
            var labelX = configPaneRect.x + 5;
            var textFieldX = labelX + labelWidth + 5;
            var textFieldWidth = configPaneRect.width - labelWidth - 15;

            GUI.Label(new Rect(labelX, currentY, labelWidth, k_LineHeightBuffer), labelText);
            var textFieldRect = new Rect(textFieldX, currentY, textFieldWidth, k_LineHeightBuffer);
            EditorGUIUtility.AddCursorRect(textFieldRect, MouseCursor.Text);

            if (GUI.Button(textFieldRect, rule["type"].Value<string>(), EditorStyles.popup))
            {
                var menu = new GenericMenu();
                CreateDropdownItemForRuleType("segmentation", rule, menu);
                CreateDropdownItemForRuleType("variant", rule, menu);
                menu.DropDown(textFieldRect);
            }
        }

        private void CreateDropdownItemForRuleType(string type, JObject rule, GenericMenu menu)
        {
            menu.AddItem(new GUIContent(type), string.Equals(type, rule["type"].Value<string>()), (val) => {
                string ruleType = (string)val;
                if (ruleType != rule["type"].Value<string>())
                {
                    //TODO: Come back to this
                    m_Controller.UpdateRuleType(rule["id"].Value<string>(), ruleType);
                }
            }, type);
        }

        private string CreateLabelAndTextField(string labelText, string textFieldText, float currentY, Rect configPaneRect, float labelWidth = 125f)
        {
            var labelX = configPaneRect.x + 5;
            var textFieldX = labelX + labelWidth + 5;
            var textFieldWidth = configPaneRect.width - labelWidth - 15;

            GUI.Label(new Rect(labelX, currentY, labelWidth, k_LineHeightBuffer), labelText);
            var textFieldRect = new Rect(textFieldX, currentY, textFieldWidth, k_LineHeightBuffer);
            EditorGUIUtility.AddCursorRect(textFieldRect, MouseCursor.Text);
            return GUI.TextField(textFieldRect, textFieldText);
        }

        private string CreateLabelWithSubLabelAndTextField(string labelText, string subLabelText, string textFieldText, float currentY, Rect configPaneRect)
        {
            var labelX = configPaneRect.x + 5;
            var labelWidth = 125f;
            var textFieldX = labelX + labelWidth + 5;
            var textFieldWidth = configPaneRect.width - labelWidth - 15;
            var labelHeight = (k_LineHeightBuffer * 0.8f);
            var subLabelHeight = (k_LineHeightBuffer * 0.8f);
            var subLabelColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);

            guiStyleLabel = GUI.skin.label;
            guiStyleSubLabel.fontSize = 8;
            guiStyleSubLabel.normal.textColor = subLabelColor;
            guiStyleSubLabel.alignment = TextAnchor.UpperLeft;
            guiStyleSubLabel.padding = new RectOffset(2,0,0,2);

            GUI.Label(new Rect(labelX, currentY, labelWidth, labelHeight), labelText, guiStyleLabel);
            GUI.Label(new Rect(labelX, currentY+labelHeight, labelWidth, subLabelHeight), subLabelText, guiStyleSubLabel);
            var textFieldRect = new Rect(textFieldX, currentY, textFieldWidth, k_LineHeightBuffer);
            EditorGUIUtility.AddCursorRect(textFieldRect, MouseCursor.Text);
            return GUI.TextField(textFieldRect, textFieldText);
        }

        private string CreateLabelWithSubLabelTextFieldAndHelpButton(string labelText, string subLabelText, string textFieldText, float currentY, Rect configPaneRect, string helpButtonPath)
        {
            var labelX = configPaneRect.x + 5;
            var labelWidth = 125f;
            var textFieldX = labelX + labelWidth + 5;
            var textFieldWidth = configPaneRect.width - labelWidth - 15;
            var labelHeight = (k_LineHeightBuffer * 0.8f);
            var subLabelHeight = (k_LineHeightBuffer * 0.8f);
            var subLabelColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
            var buttonSize = 25f;

            guiStyleLabel = GUI.skin.label;
            guiStyleSubLabel.fontSize = 8;
            guiStyleSubLabel.normal.textColor = subLabelColor;
            guiStyleSubLabel.alignment = TextAnchor.UpperLeft;
            guiStyleSubLabel.padding = new RectOffset(2,0,0,2);
            Texture helpButtonTexture = EditorGUIUtility.FindTexture("_Help");

            GUI.Label(new Rect(labelX, currentY, labelWidth, labelHeight), labelText, guiStyleLabel);
            GUI.Label(new Rect(labelX, currentY+labelHeight, labelWidth, subLabelHeight), subLabelText, guiStyleSubLabel);
            var textFieldRect = new Rect(textFieldX, currentY, textFieldWidth, k_LineHeightBuffer);
            EditorGUIUtility.AddCursorRect(textFieldRect, MouseCursor.Text);
            if (GUI.Button(new Rect(textFieldX - (2f*buttonSize), currentY, buttonSize, buttonSize), new GUIContent(helpButtonTexture, "Jexl Syntax Help\nOpens the Remote Config API Documentation in a Web Browser"), new GUIStyle(GUIStyle.none)))
            {
                Help.BrowseURL(helpButtonPath);
            }

            return GUI.TextField(textFieldRect, textFieldText);
        }

        private int CreateLabelAndSlider(string labelText, int hSliderValue, int leftValue, int rightValue, float currentY, Rect configPaneRect)
        {
            var labelX = configPaneRect.x + 5;
            var labelWidth = 125f;
            var sliderFieldX = labelX + labelWidth + 5;
            var sliderFieldWidth = configPaneRect.width - 15 - labelWidth;

            GUI.Label(new Rect(labelX, currentY, labelWidth, k_LineHeightBuffer), labelText);
            hSliderValue = EditorGUI.IntSlider(new Rect(sliderFieldX, currentY, sliderFieldWidth, k_LineHeightBuffer), hSliderValue, leftValue, rightValue);
            return hSliderValue;
        }

        private void showMessage(Rect messageRect, string messageText)
        {
            EditorGUI.HelpBox(messageRect, messageText, MessageType.Warning);
        }

        public static MultiColumnHeaderState CreateRulesMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Enabled"),
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 16,
                    minWidth = 16,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 28,
                    minWidth = 28,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Priority"),
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 16,
                    minWidth = 16,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Type"),
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 16,
                    minWidth = 16,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 30,
                    minWidth = 30,
                    maxWidth = 30,
                    autoResize = true,
                    allowToggleVisibility = false
                }
            };
            var state = new MultiColumnHeaderState(columns);
            return state;
        }
    }

    internal class ConfigsTreeView : TreeView
    {
        public event Action<IList<int>> SelectionChangedEvent;

        public ConfigsTreeView(TreeViewState state) : base(state)
        {
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem<string>(0, -1, "Root", "");
            var id = 0;
            var allItems = new List<TreeViewItem>();
            allItems.Add(new TreeViewItem<string>(id++, 0, "Settings Config", "Settings Config"));
            SetupParentsAndChildrenFromDepths(root, allItems);

            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            if (SelectionChangedEvent != null)
            {
                SelectionChangedEvent(selectedIds);
            }
        }
    }

    // Displays all the rules
    internal class RulesTreeView : TreeView
    {
        public JArray rulesList;

        public event Action<string> SelectionChangedEvent;
        public event Action<string> DeleteRule;
        public event Action<string, bool> RuleEnabledOrDisabled;
        public event Action<string, JObject> RuleAttributesChanged;

        public RulesTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, JArray rulesList) : base(state, multiColumnHeader)
        {
            this.rulesList = rulesList;
            useScrollView = true;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem<JObject>(0, -1, "Root", new JObject());
            var id = 0;
            var allItems = new List<TreeViewItem>();
            if (rulesList != null)
            {
                allItems.AddRange(rulesList.Select(x => new TreeViewItem<JObject>(id++, 0, x["name"].Value<string>(), (JObject)x))
                    .ToList<TreeViewItem>());
            }
            SetupParentsAndChildrenFromDepths(root, allItems);

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<JObject>) args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

        private void CellGUI(Rect cellRect, TreeViewItem<JObject> item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case 0:
                    Rect toggleRect = cellRect;
                    toggleRect.x += cellRect.width - 18;
                    var toggle = GUI.Toggle(toggleRect, item.data["enabled"].Value<bool>(), "");
                    if (toggle != item.data["enabled"].Value<bool>())
                    {
                        RuleEnabledOrDisabled?.Invoke(item.data["id"].Value<string>(), !item.data["enabled"].Value<bool>());
                    }
                    break;
                case 1:
                    var ruleNameStyle = EditorStyles.label;
                    ruleNameStyle.wordWrap = false;
                    GUI.Label(cellRect, item.displayName, ruleNameStyle);
                    break;
                case 2:
                    EditorGUI.BeginDisabledGroup(item.data["enabled"].Value<bool>());
                    var newPriority = EditorGUI.IntField(cellRect, item.data["priority"].Value<int>());
                    if (newPriority != item.data["priority"].Value<int>())
                    {
                        var rule = item.data;
                        rule["priority"] = newPriority;
                        RuleAttributesChanged?.Invoke(item.data["id"].Value<string>(), rule);
                    }
                    EditorGUI.EndDisabledGroup();
                    break;
                case 3:
                        EditorGUI.LabelField(cellRect, item.data["type"].Value<string>());
                    break;
                case 4:
                    EditorGUI.BeginDisabledGroup(item.data["enabled"].Value<bool>());
                    if (GUI.Button(cellRect, EditorGUIUtility.FindTexture("d_TreeEditor.Trash")))
                    {
                        DeleteRule?.Invoke(item.data["id"].Value<string>());
                    }
                    EditorGUI.EndDisabledGroup();
                    break;
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            var treeViewItems = GetRows() as List<TreeViewItem>;
            var treeViewItem = treeViewItems.Find(x => x.id == selectedIds[0]) as TreeViewItem<JObject>;
            string ruleId = null;
            if(treeViewItem == null)
            {
                return;
            }
            else
            {
                ruleId = treeViewItem.data["id"].Value<string>();
            }
            if (SelectionChangedEvent != null)
            {
                SelectionChangedEvent(ruleId);
            }
        }

        public void SetSelection(string selectRuleId)
        {
            var treeViewItems = GetRows() as List<TreeViewItem>;
            var selections = new List<int>();
            foreach (TreeViewItem<JObject> treeViewitem in treeViewItems)
            {
                if (selectRuleId == treeViewitem.data["id"].Value<string>())
                {
                    selections.Add(treeViewitem.id);
                }
            }
            SetSelection(selections, TreeViewSelectionOptions.FireSelectionChanged);

        }
    }

    internal class RulesMultiColumnHeader : MultiColumnHeader
    {
        public RulesMultiColumnHeader(MultiColumnHeaderState state) : base(state)
        {
            canSort = false;
            this.ResizeToFit();
        }
    }


}

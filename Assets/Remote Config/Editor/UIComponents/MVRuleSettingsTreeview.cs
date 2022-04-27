using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Linq;

namespace Unity.RemoteConfig.Editor.UIComponents
{
    internal class MVRuleSettingsView
    {
        MVRuleSettingsTreeView treeView;
        JObject rule;
        JArray settings;

        public event Action<JObject, JObject> MvRuleUpdated;

        public MVRuleSettingsView()
        {
            treeView = new MVRuleSettingsTreeView(new TreeViewState(), new MVRuleSettingsMultiColumnHeader(CreateSettingsMultiColumnHeaderState(1), rule, settings), null, false);
            treeView.OnAddVariantClicked += TreeView_OnAddVariantClicked;
            treeView.OnVariantUpdated += TreeView_OnVariantUpdated;
            treeView.OnVariantDeleted += TreeView_OnVariantDeleted;
            treeView.UpdateSetting += TreeView_UpdateSetting;
            treeView.SetActiveOnSettingChanged += TreeView_SetActiveOnSettingChanged;
            treeView.OnSelectAllSettingsToggleClicked += TreeView_OnSelectAllSettingsToggleClicked;
        }

        private void TreeView_SetActiveOnSettingChanged(string entityId, bool isActive)
        {
            var newRule = (JObject)rule.DeepClone();
            var ruleVariants = (JArray)newRule["value"];
            for(int i = 0; i < ruleVariants.Count; i++)
            {
                var variantValues = (JArray)ruleVariants[i]["values"];
                if (isActive)
                {
                    for(int j = 0; j < settings.Count; j++)
                    {
                        if(settings[j]["metadata"]["entityId"].Value<string>() == entityId)
                        {
                            JObject newSetting = (JObject)settings[j].DeepClone();
                            variantValues.Add(newSetting);
                        }
                    }
                }
                else
                {
                    for(int j = 0; j < variantValues.Count; j++)
                    {
                        if(variantValues[j]["metadata"]["entityId"].Value<string>() == entityId)
                        {
                            variantValues.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
            MvRuleUpdated?.Invoke(rule, newRule);
        }

        private void TreeView_UpdateSetting(int whichVariant, string entityId, JToken newValue)
        {
            var newRule = (JObject)rule.DeepClone();
            var ruleVariants = (JArray)newRule["value"];

            var variantValues = (JArray)ruleVariants[whichVariant]["values"];
            for (int i = 0; i < variantValues.Count; i++)
            {
                if (variantValues[i]["metadata"]["entityId"].Value<string>() == entityId)
                {
                    variantValues[i]["rs"]["value"] = newValue;
                    break;
                }
            }

            MvRuleUpdated?.Invoke(rule, newRule);
        }

        private void TreeView_OnVariantDeleted(string variantId)
        {
            var newRule = (JObject)rule.DeepClone();
            var ruleVariants = (JArray)newRule["value"];

            if (ruleVariants.Count > 1) {
                for(int i = 0; i < ruleVariants.Count; i++)
                {
                    if(ruleVariants[i]["id"].Value<string>() == variantId)
                    {
                        ruleVariants.RemoveAt(i);
                        MvRuleUpdated?.Invoke(rule, newRule);
                        break;
                    }
                }
            }
        }

        private void TreeView_OnVariantUpdated(JObject oldVariant, JObject newVariant)
        {
            var newRule = (JObject)rule.DeepClone();
            var ruleVariants = (JArray)newRule["value"];
            var currentVariantIds = ruleVariants.Select(var => var["id"].Value<string>()).ToArray();
            if (currentVariantIds.Contains(newVariant["id"].Value<string>()) && (newVariant["weight"].Value<string>()) == oldVariant["weight"].Value<string>())
            {
                Debug.LogWarning("Variant id: '" + newVariant["id"].Value<string>() + "' is already used.");
            }
            else
            {
                for(int i = 0; i < ruleVariants.Count; i++)
                {
                    if(ruleVariants[i].ToString() == oldVariant.ToString())
                    {
                        ruleVariants[i] = newVariant;
                    }
                }
                MvRuleUpdated?.Invoke(rule, newRule);
            }
        }

        private void TreeView_OnAddVariantClicked()
        {
            var newRule = (JObject)rule.DeepClone();
            var ruleVariants = (JArray)newRule["value"];

            var newVariant = new JObject();

            var currentVariantIndexes = new int[ruleVariants.Count];
            int currentIndex = 0;
            for (int i = 0; i < ruleVariants.Count; i++){
                if (Int32.TryParse(ruleVariants[i]["id"].Value<string>().Replace("variant-",""), out currentIndex)){
                    currentVariantIndexes[i] = currentIndex;
                }
            }
            var idEndingIndex = currentVariantIndexes.Max()+1;
            newVariant["id"] = "variant-" + idEndingIndex;
            newVariant["type"] = "variant";
            newVariant["values"] = ruleVariants[0]["values"].DeepClone();
            newVariant["weight"] = null;

            ruleVariants.Add(newVariant);
            MvRuleUpdated?.Invoke(rule, newRule);
        }

        private void TreeView_OnSelectAllSettingsToggleClicked(bool allSettingsToggle)
        {
            var newRule = (JObject)rule.DeepClone();
            var ruleVariants = (JArray)newRule["value"];
            var newRuleVariants = new JArray();

            // go through variants, add all the settings from settingsList if toggleState
            for (int i = 0; i < ruleVariants.Count; i++)
            {
                var currVariant = (JObject)ruleVariants[i].DeepClone();
                var currVariantValues = (JArray)currVariant["values"];
                if (allSettingsToggle)
                {
                    for (int j = 0; j < treeView.settingsList.Count; j++)
                    {
                        var valAdded = false;
                        for(int k = 0; k < currVariantValues.Count; k++)
                        {
                            if(currVariantValues[k]["metadata"]["entityId"].Value<string>() == treeView.settingsList[j]["metadata"]["entityId"].Value<string>())
                            {
                                valAdded = true;
                            }
                        }
                        if (!valAdded)
                        {
                            var currSetting = (JObject)treeView.settingsList[j].DeepClone();
                            currVariantValues.Add(currSetting);
                            TreeView_SetActiveOnSettingChanged(currSetting["metadata"]["entityId"].Value<string>(), true);
                        }
                    }
                }
                else
                {
                    currVariantValues.Clear();
                }
                currVariant["values"] = currVariantValues;
                newRuleVariants.Add(currVariant);
            }
            newRule["value"] = newRuleVariants;
            MvRuleUpdated?.Invoke(rule, newRule);
        }

        public void OnGUI(Rect viewRect)
        {
            treeView.OnGUI(viewRect);
        }

        public void UpdateView(JArray settings, JObject rule)
        {
            this.settings = settings;
            treeView.settingsList = settings;
            treeView.rule = rule;
            this.rule = rule;
            treeView.UpdateHeader(new MVRuleSettingsMultiColumnHeader(CreateSettingsMultiColumnHeaderState(((JArray)rule["value"]).Count), rule, settings));
            treeView.Reload();
        }

        public static MultiColumnHeaderState CreateSettingsMultiColumnHeaderState(int numberOfVariants)
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[numberOfVariants + 4];
            columns[0] = new MultiColumnHeaderState.Column
            {
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Center,
                width = 30,
                minWidth = 30,
                maxWidth = 30,
                autoResize = false,
                allowToggleVisibility = false
            };
            columns[1] = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Key"),
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Center,
                width = 150,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = false
            };
            columns[2] = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Type"),
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Center,
                width = 150,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = false
            };
            for(int i = 1; i <= numberOfVariants; i++)
            {
                columns[2 + i] = new MultiColumnHeaderState.Column
                {
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                };
            }
            columns[2 + numberOfVariants + 1] = new MultiColumnHeaderState.Column
            {
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Center,
                width = 150,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = false
            };
            var state = new MultiColumnHeaderState(columns);
            return state;
        }
    }

    internal class MVRuleSettingsMultiColumnHeader : MultiColumnHeader
    {
        public event Action OnAddVariantClicked;
        public JObject rule;
        public JArray settings;
        public event Action<JObject, JObject> OnVariantUpdated;
        public event Action<string> OnDeleteVariant;
        public string m_variantWeightTooltip = "Sum of all variant weights must be 100.\nIf all variant weights are left empty,\nthe weights will be evenly distributed.";

        public event Action<bool> OnSelectAllSettingsClicked;
        public MVRuleSettingsMultiColumnHeader(MultiColumnHeaderState state, JObject rule, JArray settings) : base(state)
        {
            canSort = false;
            this.rule = rule;
            this.settings = settings;
        }

        private bool GetAllSettingsToggleState()
        {
            var ruleVariants = (JArray)rule["value"];
            var variantValues = (JArray)ruleVariants[0]["values"];
            var activeSettingIds = new List<string>(variantValues.Count);
            var allSettingsToggle = true;

            for (int i = 0; i < variantValues.Count; i++)
            {
                activeSettingIds.Add(variantValues[i]["metadata"]["entityId"].Value<string>());
            }

            for (int i = 0; i < settings.Count; i++)
            {
                var settingEnabled = activeSettingIds.Contains(settings[i]["metadata"]["entityId"].Value<string>());
                if (!settingEnabled)
                {
                    allSettingsToggle = false;
                    break;
                }
            }

            return allSettingsToggle;
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            var ruleVariants = (JArray)rule["value"];
            if (columnIndex == 0)
            {
                var allSettingsToggle = GetAllSettingsToggleState();
                var newAllSettingsToggle = GUI.Toggle (new Rect (0,3,18,18), allSettingsToggle, "");
                if (allSettingsToggle != newAllSettingsToggle)
                {
                    OnSelectAllSettingsClicked?.Invoke(newAllSettingsToggle);
                }
            }
            if(columnIndex > 2 && columnIndex != state.columns.Length - 1)
            {
                var guiStyle = new GUIStyle(GUI.skin.textField);
                guiStyle.alignment = TextAnchor.LowerLeft;
                //override the rendering of column two for now
                var variantNameRect = new Rect(headerRect.x, headerRect.y + headerRect.height - 24f, headerRect.width * 0.6f, 22f);
                var variantAllocRect = new Rect(headerRect.x + variantNameRect.width, headerRect.y + headerRect.height - 24f, (headerRect.width * 0.2f) - 1f, 22f);

                var variant = (JObject)((JArray)rule["value"])[columnIndex - 3];
                var variantName = variant["id"].Value<string>();
                var newVariantName = EditorGUI.TextField(variantNameRect, variantName, guiStyle);

                int variantWeightInt;
                string variantWeight;
                if (string.IsNullOrEmpty(variant["weight"].ToString()))
                {
                    variantWeight = null;
                }
                else if (Int32.TryParse(variant["weight"].ToString(), out variantWeightInt))
                {
                    variantWeight = variantWeightInt.ToString();
                }
                else
                {
                    variantWeight = null;
                }

                var newVariantWeight = EditorGUI.TextField(variantAllocRect, variantWeight, guiStyle);

                EditorGUI.BeginDisabledGroup(ruleVariants.Count == 1);
                if (GUI.Button(new Rect(headerRect.x + variantNameRect.width + variantAllocRect.width, headerRect.y + headerRect.height - 24f, headerRect.width * 0.2f - 1f, 22f),
                    new GUIContent("X", m_variantWeightTooltip)))
                {
                    OnDeleteVariant?.Invoke(variantName);
                }
                EditorGUI.EndDisabledGroup();
                int newVariantWeightInt;
                if(Int32.TryParse(newVariantWeight, out newVariantWeightInt) || string.IsNullOrEmpty(newVariantWeight))
                {
                    if (newVariantName != variantName || newVariantWeight != variantWeight)
                    {
                        var newVariant = (JObject)variant.DeepClone();
                        newVariant["id"] = newVariantName;
                        if(string.IsNullOrEmpty(newVariantWeight))
                        {
                            newVariant["weight"] = null;
                        }
                        else
                        {
                            newVariant["weight"] = newVariantWeightInt;
                        }

                        OnVariantUpdated?.Invoke(variant, newVariant);
                    }
                }
            }
            else if(columnIndex == state.columns.Length - 1)
            {
                var addVariantBtnRect = new Rect(headerRect.x, headerRect.y+2, headerRect.width, 22f);
                if (GUI.Button(addVariantBtnRect, new GUIContent("Add Variant", m_variantWeightTooltip)))
                {
                    OnAddVariantClicked?.Invoke();
                }
            }
            else
            {
                base.ColumnHeaderGUI(column, headerRect, columnIndex);
            }
        }
    }

    internal class MVRuleSettingsTreeView : TreeView
    {
        public JArray settingsList;
        public JObject rule;

        private MVRuleJsonModal m_MVRuleJsonModal;
        JsonSerializerSettings rawDateSettings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };

        public event Action<int, string, JToken> UpdateSetting;
        public event Action<string, bool> SetActiveOnSettingChanged;
        public event Action<JObject, JObject> OnVariantUpdated;
        public event Action<string> OnVariantDeleted;

        public bool enableEditingSettingsKeys;

        public event Action OnAddVariantClicked;
        public event Action<bool> OnSelectAllSettingsToggleClicked;

        public void UpdateHeader(MVRuleSettingsMultiColumnHeader header)
        {
            header.OnAddVariantClicked += () => OnAddVariantClicked?.Invoke();
            header.OnVariantUpdated += (oldVariant, newVariant) => OnVariantUpdated?.Invoke(oldVariant, newVariant);
            header.OnDeleteVariant += (variantId) => OnVariantDeleted(variantId);
            header.OnSelectAllSettingsClicked += (allSettingsToggle) => OnSelectAllSettingsToggleClicked?.Invoke(allSettingsToggle);
            multiColumnHeader = header;
        }

        public MVRuleSettingsTreeView(TreeViewState state, MVRuleSettingsMultiColumnHeader multiColumnHeader, JArray settingsList,
            bool enableEditingSettingsKeys = true) : base(state, multiColumnHeader)
        {
            this.rowHeight = 18f;
            this.settingsList = settingsList;
            this.enableEditingSettingsKeys = enableEditingSettingsKeys;
            multiColumnHeader.OnAddVariantClicked += () => OnAddVariantClicked?.Invoke();
            multiColumnHeader.OnSelectAllSettingsClicked += (allSettingsToggle) => OnSelectAllSettingsToggleClicked?.Invoke(allSettingsToggle);
            useScrollView = true;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem<JObject>(0, -1, "Root", new JObject());
            var id = 0;
            var allItems = new List<TreeViewItem>();
            Dictionary<string, JObject> settingDict = new Dictionary<string, JObject>();

            if (settingsList != null && ((JArray)rule["value"]).Count > 0)
            {
                var ruleValue = (JArray)rule["value"];
                var variantValues = (JArray)ruleValue[0]["values"];
                var activeSettingIds = new List<string>(variantValues.Count);
                for (int i = 0; i < variantValues.Count; i++)
                {
                    activeSettingIds.Add(variantValues[i]["metadata"]["entityId"].Value<string>());
                }

                for (int i = 0; i < settingsList.Count; i++)
                {
                    var settingEnabled = activeSettingIds.Contains(settingsList[i]["metadata"]["entityId"].Value<string>());
                    if (settingEnabled)
                    {
                        activeSettingIds.Remove(settingsList[i]["metadata"]["entityId"].Value<string>());
                    }
                    var itemToAdd = new JObject();
                    itemToAdd["enabled"] = settingEnabled;
                    itemToAdd["entityId"] = settingsList[i]["metadata"]["entityId"];
                    itemToAdd["key"] = settingsList[i]["rs"]["key"];
                    itemToAdd["type"] = settingsList[i]["rs"]["type"];
                    itemToAdd["values"] = new JArray();
                    itemToAdd["defaultValue"] = settingsList[i]["rs"]["value"];
                    settingDict.Add(itemToAdd["entityId"].Value<string>(), itemToAdd);
                }
            }
            if (rule != null && rule["value"].HasValues)
            {
                var ruleValueArr = (JArray)rule["value"];
                for (int i = 0; i < ruleValueArr.Count; i++)
                {
                    var ruleVariantValueArr = (JArray)ruleValueArr[i]["values"];
                    for (int j = 0; j < ruleVariantValueArr.Count; j++)
                    {
                        //deleted setting, add to dict
                        if (!settingDict.ContainsKey(ruleVariantValueArr[j]["metadata"]["entityId"].Value<string>()))
                        {
                            var itemToAdd = new JObject();
                            itemToAdd["enabled"] = true;
                            itemToAdd["entityId"] = ruleVariantValueArr[j]["metadata"]["entityId"];
                            itemToAdd["key"] = ruleVariantValueArr[j]["rs"]["key"];
                            itemToAdd["type"] = ruleVariantValueArr[j]["rs"]["type"];
                            itemToAdd["values"] = new JArray();
                            settingDict.Add(itemToAdd["entityId"].Value<string>(), itemToAdd);
                        }
                        ((JArray)settingDict[ruleVariantValueArr[j]["metadata"]["entityId"].Value<string>()]["values"]).Add(ruleVariantValueArr[j]["rs"]["value"]);
                    }
                }

                foreach (var setting in settingDict.Values)
                {
                    allItems.Add(new TreeViewItem<JObject>(id++, 0, setting["key"].Value<string>(), setting, setting["enabled"].Value<bool>()));
                }
            }

            SetupParentsAndChildrenFromDepths(root, allItems);

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<JObject>)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, TreeViewItem<JObject> item, int column, ref RowGUIArgs args)
        {
            if(column <= 2)
            {
                switch (column)
                {
                    case 0:
                        CenterRectUsingSingleLineHeight(ref cellRect);
                        var toggle = GUI.Toggle(cellRect, item.enabled, "");
                        if (toggle != item.enabled)
                        {
                            SetActiveOnSettingChanged?.Invoke(item.data["entityId"].Value<string>(), toggle);
                        }
                        break;
                    case 1:
                        EditorGUI.BeginDisabledGroup(true);
                        GUI.TextField(cellRect, item.data["key"].Value<string>());
                        EditorGUI.EndDisabledGroup();
                        break;
                    case 2:
                        CenterRectUsingSingleLineHeight(ref cellRect);
                        EditorGUI.BeginDisabledGroup(true);
                        GUIContent ddBtnContent = new GUIContent(string.IsNullOrEmpty(item.data["type"].Value<string>()) ? "Select a type" : item.data["type"].Value<string>());
                        if (GUI.Button(cellRect, ddBtnContent, EditorStyles.popup))
                        {
                        }
                        EditorGUI.EndDisabledGroup();

                        break;
                }

            }
            else if(column != ((JArray)rule["value"]).Count + 3)
            {
                var whichVariant = column - 3;
                EditorGUI.BeginDisabledGroup(item.enabled == false);
                var valuesArr = (JArray)item.data["values"];
                var useDefaultValue = false;
                if(valuesArr.Count <= whichVariant)
                {
                    useDefaultValue = true;
                }
                bool itemUpdated = false;
                JToken newValue = null;
                switch (item.data["type"].Value<string>())
                {
                    case "string":
                        var oldStringVal = "";
                        DateTime dateValue;
                        if (useDefaultValue)
                        {
                            oldStringVal = string.IsNullOrEmpty(item.data["defaultValue"].Value<string>()) ? "" : item.data["defaultValue"].Value<string>();
                            if (DateTime.TryParse(oldStringVal, out dateValue))
                            {
                                oldStringVal = JsonConvert.SerializeObject(item.data["defaultValue"], rawDateSettings).Replace("\"", "");
                            }
                        }
                        else
                        {
                            oldStringVal = string.IsNullOrEmpty(valuesArr[whichVariant].Value<string>()) ? "" : valuesArr[whichVariant].Value<string>();
                            if (DateTime.TryParse(oldStringVal, out dateValue))
                            {
                                oldStringVal =  JsonConvert.SerializeObject(valuesArr[whichVariant], rawDateSettings).Replace("\"", "");
                            }
                        }
                        var newStringVal = GUI.TextField(cellRect, oldStringVal);
                        if (item.enabled)
                        {
                            EditorGUIUtility.AddCursorRect(cellRect, MouseCursor.Text);
                        }
                        if(newStringVal != oldStringVal)
                        {
                            itemUpdated = true;
                            newValue = newStringVal;
                        }
                        break;
                    case "bool":
                        var boolVal = useDefaultValue ? item.data["defaultValue"].Value<bool>() : valuesArr[whichVariant].Value<bool>();
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("True"), boolVal == true, () => UpdateSetting?.Invoke(whichVariant, item.data["entityId"].Value<string>(), true));
                        menu.AddItem(new GUIContent("False"), boolVal == false, () => UpdateSetting?.Invoke(whichVariant, item.data["entityId"].Value<string>(), false));
                        GUIContent boolDdBtnContent = new GUIContent(boolVal.ToString());
                        if (GUI.Button(cellRect, boolDdBtnContent, EditorStyles.popup))
                        {
                            menu.DropDown(cellRect);
                        }
                        break;
                    case "float":
                        var oldFloatval = useDefaultValue ? item.data["defaultValue"].Value<float>() : valuesArr[whichVariant].Value<float>();
                        var newFloatVal = EditorGUI.FloatField(cellRect, oldFloatval);
                        if (Math.Abs(oldFloatval - newFloatVal) > float.Epsilon)
                        {
                            itemUpdated = true;
                            newValue = newFloatVal;
                        }
                        break;
                    case "int":
                        var oldIntVal = useDefaultValue ? item.data["defaultValue"].Value<int>() : valuesArr[whichVariant].Value<int>();
                        var newIntVal = EditorGUI.IntField(cellRect, oldIntVal);
                        if(oldIntVal != newIntVal)
                        {
                            itemUpdated = true;
                            newValue = newIntVal;
                        }
                        break;
                    case "long":
                        var oldLongVal = useDefaultValue ? item.data["defaultValue"].Value<long>() : valuesArr[whichVariant].Value<long>();
                        var newLongVal = EditorGUI.LongField(cellRect, oldLongVal);
                        if(oldLongVal != newLongVal)
                        {
                            itemUpdated = true;
                            newValue = newLongVal;
                        }
                        break;
                    case "json":
                        var oldJsonVal = useDefaultValue ? item.data["defaultValue"].ToString() : valuesArr[whichVariant].ToString();
                        var newJsonVal = oldJsonVal;
                        if (GUI.Button(new Rect(cellRect.x, cellRect.y, cellRect.width, cellRect.height),
                            new GUIContent("Edit",EditorGUIUtility.FindTexture("vcs_document")), GUI.skin.button))
                        {
                            var keyName = item.data["key"].Value<string>() +" "+ ((JArray)rule["value"])[whichVariant]["id"];
                            m_MVRuleJsonModal = MVRuleJsonModal.CreateInstance(newJsonVal, keyName, item.data["entityId"].ToString());
                            m_MVRuleJsonModal.currentVariantIndex = whichVariant;
                        }

                        Event e = Event.current;
                        if (m_MVRuleJsonModal != null && e.commandName == "JsonSubmitted")
                        {
                            e.commandName = null;

                            newJsonVal =  m_MVRuleJsonModal.currentText;
                            whichVariant = m_MVRuleJsonModal.currentVariantIndex;
                        }
                        if(newJsonVal != oldJsonVal)
                        {
                            // find item with corresponding entityId before updating
                            for (int i = 0; i < settingsList.Count; i++)
                            {
                                if (settingsList[i]["metadata"]["entityId"].ToString() == m_MVRuleJsonModal.keyEntityId)
                                {
                                    item.data["entityId"] = settingsList[i]["metadata"]["entityId"].ToString();
                                    itemUpdated = true;
                                    newValue = JToken.Parse(newJsonVal);
                                    break;
                                }
                            }
                        }

                        break;
                }

                EditorGUI.EndDisabledGroup();
                if (itemUpdated && newValue != null)
                {
                    UpdateSetting?.Invoke(whichVariant, item.data["entityId"].Value<string>(), newValue);
                }
            }
        }
    }
}
